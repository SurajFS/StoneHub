namespace Catalog.Application.Dtos;

// Flat category row (top-level when ParentId is null, otherwise a subcategory). The client
// composes the two-level picker from ParentId — the API returns an ordered flat list.
public sealed record CategoryDto(
    Guid Id,
    string Name,
    string Slug,
    Guid? ParentId,
    int DisplayOrder,
    bool IsActive);
