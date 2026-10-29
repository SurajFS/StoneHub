using Billing.Application;
using Catalog.Application;
using Identity.Application;
using Inquiries.Application;
using Inquiries.Application.Dtos;
using Inquiries.Domain;
using SharedKernel;
using Xunit;

namespace Inquiries.Tests;

// Wholesale inquiries follow the chat rule: a Free seller can't send one, a Free wholesaler sees
// that one arrived but not its message, and can't answer it until they subscribe (402).
public sealed class InquiryPlanGateTests
{
    private static readonly Guid Seller = Guid.NewGuid();
    private static readonly Guid Wholesaler = Guid.NewGuid();

    private readonly FakeInquiryStore _store = new();

    [Fact]
    public async Task FreeSeller_CannotSendInquiry_NothingSaved()
    {
        var result = await Create(new FakeEntitlements(Seller));

        Assert.Equal(ErrorType.PaymentRequired, result.ErrorType);
        Assert.Equal(0, _store.SaveCount);
    }

    [Fact]
    public async Task PayingSeller_CanSendInquiry_EvenToFreeWholesaler()
    {
        var result = await Create(new FakeEntitlements(Wholesaler));

        Assert.True(result.IsSuccess);
        Assert.Equal(1, _store.SaveCount);
    }

    [Fact]
    public async Task FreeWholesaler_SeesInquiryButNotMessage()
    {
        var sent = await Create(new FakeEntitlements());

        var locked = await new GetReceivedInquiriesQueryHandler(_store, new FakeEntitlements(Wholesaler))
            .Handle(new GetReceivedInquiriesQuery(Wholesaler), CancellationToken.None);
        var paid = await new GetReceivedInquiriesQueryHandler(_store, new FakeEntitlements())
            .Handle(new GetReceivedInquiriesQuery(Wholesaler), CancellationToken.None);

        var row = Assert.Single(locked);
        Assert.Equal(sent.Value, row.Id);
        Assert.True(row.IsLocked);
        Assert.Null(row.Message);
        Assert.Equal("Need 200 sq ft", Assert.Single(paid).Message);
    }

    [Fact]
    public async Task FreeWholesaler_CannotAnswerInquiry()
    {
        var sent = await Create(new FakeEntitlements());
        var savesBefore = _store.SaveCount;

        var result = await new UpdateInquiryStatusCommandHandler(_store, new FakeEntitlements(Wholesaler))
            .Handle(new UpdateInquiryStatusCommand(sent.Value, Wholesaler, "accepted"), CancellationToken.None);

        Assert.Equal(ErrorType.PaymentRequired, result.ErrorType);
        Assert.Equal(savesBefore, _store.SaveCount);
    }

    private Task<Result<Guid>> Create(FakeEntitlements entitlements) =>
        new CreateInquiryCommandHandler(_store, _store, _store, entitlements)
            .Handle(new CreateInquiryCommand(Seller, FakeInquiryStore.ListingId, 200m, "Need 200 sq ft"), CancellationToken.None);

    private sealed class FakeEntitlements(params Guid[] lockedUsers) : IPlanEntitlements
    {
        public Task<bool> CanUseOwnerFeaturesAsync(Guid userId, CancellationToken ct = default) =>
            Task.FromResult(!lockedUsers.Contains(userId));
    }

    // One wholesale listing owned by Wholesaler, and the inquiries sent about it.
    private sealed class FakeInquiryStore : IInquiryRepository, IInquiryQueryService, IProductLookup, ISellerDirectory
    {
        public static readonly Guid ListingId = Guid.NewGuid();

        private readonly List<Inquiry> _inquiries = [];

        public int SaveCount { get; private set; }

        public Task<Inquiry?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
            Task.FromResult(_inquiries.FirstOrDefault(i => i.Id == id));

        public void Add(Inquiry inquiry) => _inquiries.Add(inquiry);

        public Task SaveChangesAsync(CancellationToken ct = default)
        {
            SaveCount++;
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<InquiryDto>> GetReceivedAsync(Guid wholesalerId, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<InquiryDto>>(_inquiries
                .Where(i => i.WholesalerId == wholesalerId)
                .Select(i => new InquiryDto(i.Id, i.SellerId, i.WholesalerId, i.ProductId, i.ProductTitle, i.SellerName,
                    i.Quantity, i.Message, i.Status.ToString(), i.CreatedAt))
                .ToList());

        public Task<IReadOnlyList<InquiryDto>> GetSentAsync(Guid sellerId, CancellationToken ct = default) =>
            throw new NotSupportedException("Not used by these tests.");

        public Task<ProductOwnerInfo?> GetOwnerInfoAsync(Guid productId, CancellationToken ct = default) =>
            Task.FromResult<ProductOwnerInfo?>(productId == ListingId
                ? new ProductOwnerInfo(Wholesaler, "Wholesaler", "Rajasthan green, bulk", IsActive: true)
                : null);

        public Task<SellerDirectoryEntry?> GetByUserIdAsync(Guid userId, CancellationToken ct = default) =>
            Task.FromResult<SellerDirectoryEntry?>(new SellerDirectoryEntry("Ramesh Marbles", "Kishangarh"));
    }
}
