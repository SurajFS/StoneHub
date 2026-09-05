using Catalog.Application.Categories;
using Catalog.Application.Dtos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;

namespace StoneHub.Api.Controllers;

[ApiController]
[Route("api/v1/categories")]
public sealed class CategoriesController(IMediator mediator) : ControllerBase
{
    // Public: active categories + subcategories, ordered for a two-level picker.
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CategoryDto>>> GetAll(CancellationToken ct)
    {
        var categories = await mediator.Send(new GetCategoriesQuery(IncludeInactive: false), ct);
        return Ok(categories);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateCategoryRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(
            new CreateCategoryCommand(request.Name, request.ParentId, request.DisplayOrder), ct);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetAll), null, new { id = result.Value })
            : result.ToActionResult();
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateCategoryRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new UpdateCategoryCommand(id, request.Name, request.DisplayOrder), ct);
        return result.IsSuccess ? NoContent() : result.ToActionResult();
    }

    [Authorize(Roles = "Admin")]
    [HttpPatch("{id:guid}/active")]
    public async Task<IActionResult> SetActive(Guid id, SetCategoryActiveRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new SetCategoryActiveCommand(id, request.IsActive), ct);
        return result.IsSuccess ? NoContent() : result.ToActionResult();
    }
}

public sealed record CreateCategoryRequest(string Name, Guid? ParentId, int DisplayOrder);

public sealed record UpdateCategoryRequest(string Name, int DisplayOrder);

public sealed record SetCategoryActiveRequest(bool IsActive);
