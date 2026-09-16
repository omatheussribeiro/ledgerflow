using LedgerFlow.Api.Extensions;
using LedgerFlow.Application.Dtos.Common;
using LedgerFlow.Application.Dtos.Financial;
using LedgerFlow.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LedgerFlow.Api.Controllers;

/// <summary>Manages income and expense categories used to classify transactions.</summary>
[ApiController]
[Authorize]
[Route("api/categories")]
[Produces("application/json")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status503ServiceUnavailable)]
public sealed class CategoriesController(IFinancialService service) : ControllerBase
{
    /// <summary>Lists the authenticated user's categories.</summary>
    [HttpGet]
    [ProducesResponseType<ApiResponseDto<IReadOnlyList<CategoryResponseDto>>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponseDto<IReadOnlyList<CategoryResponseDto>>>> Get(
        CancellationToken cancellationToken)
    {
        var categories = await service.GetCategoriesAsync(User.GetUserId(), cancellationToken);
        return Ok(ApiResponseFactory.Success(
            categories,
            "Categories retrieved successfully."));
    }

    /// <summary>Creates an income or expense category.</summary>
    [HttpPost]
    [ProducesResponseType<ApiResponseDto<CategoryResponseDto>>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<ApiResponseDto<CategoryResponseDto>>> Create(
        CreateCategoryRequestDto request,
        CancellationToken cancellationToken)
    {
        var category = await service.CreateCategoryAsync(User.GetUserId(), request, cancellationToken);
        return Created(
            $"/api/categories/{category.Id}",
            ApiResponseFactory.Success(category, "Category created successfully."));
    }
}
