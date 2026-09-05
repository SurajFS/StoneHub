using Catalog.Application.Dtos;

namespace Catalog.Application;

public interface ICategoryQueryService
{
    Task<IReadOnlyList<CategoryDto>> GetAllAsync(bool includeInactive, CancellationToken ct = default);
}
