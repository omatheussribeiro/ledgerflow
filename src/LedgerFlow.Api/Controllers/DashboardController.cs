using System.ComponentModel.DataAnnotations;
using LedgerFlow.Api.Extensions;
using LedgerFlow.Application.Dtos.Common;
using LedgerFlow.Application.Dtos.Financial;
using LedgerFlow.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LedgerFlow.Api.Controllers;

/// <summary>Provides consolidated financial indicators and report data.</summary>
[ApiController]
[Authorize]
[Route("api/dashboard")]
[Produces("application/json")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status503ServiceUnavailable)]
public sealed class DashboardController(IFinancialService service) : ControllerBase
{
    /// <summary>Returns balance, monthly totals, six-month evolution and category concentration.</summary>
    /// <param name="year">Report year between 2000 and 2100. Defaults to the current UTC year.</param>
    /// <param name="month">Report month between 1 and 12. Defaults to the current UTC month.</param>
    /// <param name="cancellationToken">Cancels the request when the client disconnects.</param>
    [HttpGet]
    [ProducesResponseType<ApiResponseDto<DashboardResponseDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponseDto<DashboardResponseDto>>> Get(
        [FromQuery, Range(2000, 2100)] int? year,
        [FromQuery, Range(1, 12)] int? month,
        CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var selected = new DateOnly(year ?? today.Year, month ?? today.Month, 1);
        var dashboard = await service.GetDashboardAsync(User.GetUserId(), selected, cancellationToken);
        return Ok(ApiResponseFactory.Success(
            dashboard,
            "Dashboard retrieved successfully."));
    }
}
