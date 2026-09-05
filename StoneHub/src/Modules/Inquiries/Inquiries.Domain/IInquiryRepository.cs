namespace Inquiries.Domain;

public interface IInquiryRepository
{
    Task<Inquiry?> GetByIdAsync(Guid id, CancellationToken ct = default);
    void Add(Inquiry inquiry);
    Task SaveChangesAsync(CancellationToken ct = default);
}
