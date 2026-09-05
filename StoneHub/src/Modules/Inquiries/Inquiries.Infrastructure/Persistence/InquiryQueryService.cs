using System.Linq.Expressions;
using Inquiries.Application;
using Inquiries.Application.Dtos;
using Inquiries.Domain;
using Microsoft.EntityFrameworkCore;

namespace Inquiries.Infrastructure.Persistence;

public sealed class InquiryQueryService(InquiryDbContext dbContext) : IInquiryQueryService
{
    public Task<IReadOnlyList<InquiryDto>> GetReceivedAsync(Guid wholesalerId, CancellationToken ct = default) =>
        QueryAsync(i => i.WholesalerId == wholesalerId, ct);

    public Task<IReadOnlyList<InquiryDto>> GetSentAsync(Guid sellerId, CancellationToken ct = default) =>
        QueryAsync(i => i.SellerId == sellerId, ct);

    private async Task<IReadOnlyList<InquiryDto>> QueryAsync(Expression<Func<Inquiry, bool>> predicate, CancellationToken ct) =>
        await dbContext.Inquiries
            .AsNoTracking()
            .Where(predicate)
            .OrderByDescending(i => i.CreatedAt)
            .Select(i => new InquiryDto(
                i.Id, i.SellerId, i.WholesalerId, i.ProductId, i.ProductTitle,
                i.SellerName, i.Quantity, i.Message, i.Status.ToString(), i.CreatedAt))
            .ToListAsync(ct);
}
