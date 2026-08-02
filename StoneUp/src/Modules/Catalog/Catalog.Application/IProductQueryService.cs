using Catalog.Application.Dtos;

namespace Catalog.Application;

// Read-side abstraction: implemented in Infrastructure by querying the DbContext directly
// and projecting to DTOs, bypassing the aggregate repository entirely. Search/browse needs
// query flexibility that a write-side repository shouldn't have to support.
public interface IProductQueryService
{
    Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<ProductDto>> SearchAsync(ProductSearchFilter filter, CancellationToken ct = default);
}
