using Inquiries.Domain;
using Inquiries.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Inquiries.Infrastructure;

public sealed class InquiryDbContext(DbContextOptions<InquiryDbContext> options) : DbContext(options)
{
    public DbSet<Inquiry> Inquiries => Set<Inquiry>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.HasDefaultSchema("inquiries");
        builder.ApplyConfiguration(new InquiryConfiguration());
    }
}
