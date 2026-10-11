using Locations.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Locations.Infrastructure.Persistence;

public sealed class DatasetImportConfiguration : IEntityTypeConfiguration<DatasetImport>
{
    public void Configure(EntityTypeBuilder<DatasetImport> builder)
    {
        builder.ToTable("DatasetImports");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasMaxLength(100);
        builder.Property(x => x.Version).IsRequired().HasMaxLength(64);
        builder.Property(x => x.ImportedAt).IsRequired();
    }
}
