using Ecommerce.Shared.Abstractions;
using Ecommerce.Shared.Common;

namespace Ecommerce.Catalog.Application.Queries.GetCategoriesTree;

public record GetCategoriesTreeQuery : IQuery<Result<List<CategoryTreeDto>>>;

public record CategoryTreeDto(
    Guid Id,
    string Name,
    string Slug,
    List<CategoryTreeDto> Children);
