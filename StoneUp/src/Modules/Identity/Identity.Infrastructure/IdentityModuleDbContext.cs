using Identity.Domain;
using Identity.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure;

public sealed class IdentityModuleDbContext(DbContextOptions<IdentityModuleDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<SellerProfile> SellerProfiles => Set<SellerProfile>();
    public DbSet<BuyerProfile> BuyerProfiles => Set<BuyerProfile>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.HasDefaultSchema("identity");

        builder.ApplyConfiguration(new SellerProfileConfiguration());
        builder.ApplyConfiguration(new BuyerProfileConfiguration());
    }
}
