using Inquiries.Domain;
using Microsoft.EntityFrameworkCore;

namespace Inquiries.Infrastructure.Persistence;

public sealed class InquiryRepository(InquiryDbContext dbContext) : IInquiryRepository
{
    public Task<Inquiry?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        dbContext.Inquiries.FirstOrDefaultAsync(x => x.Id == id, ct);

    public void Add(Inquiry inquiry) => dbContext.Inquiries.Add(inquiry);

    public Task SaveChangesAsync(CancellationToken ct = default) => dbContext.SaveChangesAsync(ct);
}
