using SharedKernel;

namespace Inquiries.Domain;

// A seller's bulk-purchase inquiry to a wholesaler about a specific product. Product title and
// seller name are denormalized at creation so list views don't cross module boundaries.
public sealed class Inquiry : AggregateRoot<Guid>
{
    public Guid SellerId { get; private set; }
    public Guid WholesalerId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductTitle { get; private set; } = string.Empty;
    public string? SellerName { get; private set; }
    public decimal Quantity { get; private set; }
    public string? Message { get; private set; }
    public InquiryStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private Inquiry() { }

    private Inquiry(
        Guid id,
        Guid sellerId,
        Guid wholesalerId,
        Guid productId,
        string productTitle,
        string? sellerName,
        decimal quantity,
        string? message) : base(id)
    {
        SellerId = sellerId;
        WholesalerId = wholesalerId;
        ProductId = productId;
        ProductTitle = productTitle;
        SellerName = sellerName;
        Quantity = quantity;
        Message = message;
        Status = InquiryStatus.Pending;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public static Result<Inquiry> Create(
        Guid sellerId,
        Guid wholesalerId,
        Guid productId,
        string productTitle,
        string? sellerName,
        decimal quantity,
        string? message)
    {
        if (sellerId == wholesalerId)
            return Result.Failure<Inquiry>("You cannot inquire about your own listing.");
        if (quantity <= 0)
            return Result.Failure<Inquiry>("Quantity must be greater than zero.");

        return Result.Success(new Inquiry(
            Guid.NewGuid(), sellerId, wholesalerId, productId, productTitle,
            string.IsNullOrWhiteSpace(sellerName) ? null : sellerName.Trim(),
            quantity,
            string.IsNullOrWhiteSpace(message) ? null : message.Trim()));
    }

    public Result Accept() => Transition(InquiryStatus.Accepted);

    public Result Decline() => Transition(InquiryStatus.Declined);

    private Result Transition(InquiryStatus target)
    {
        if (Status != InquiryStatus.Pending)
            return Result.Failure($"Only a pending inquiry can be {target.ToString().ToLowerInvariant()}.");

        Status = target;
        return Result.Success();
    }
}
