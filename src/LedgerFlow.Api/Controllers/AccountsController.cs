using LedgerFlow.Api.Extensions;
using LedgerFlow.Application.Dtos.Common;
using LedgerFlow.Application.Dtos.Financial;
using LedgerFlow.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LedgerFlow.Api.Controllers;

/// <summary>Manages the authenticated user's financial accounts.</summary>
[ApiController]
[Authorize]
[Route("api/accounts")]
[Produces("application/json")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status503ServiceUnavailable)]
public sealed class AccountsController(IFinancialService service) : ControllerBase
{
    /// <summary>Lists all accounts with balances calculated from cleared transactions.</summary>
    [HttpGet]
    [ProducesResponseType<ApiResponseDto<IReadOnlyList<AccountResponseDto>>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponseDto<IReadOnlyList<AccountResponseDto>>>> Get(
        CancellationToken cancellationToken)
    {
        var accounts = await service.GetAccountsAsync(User.GetUserId(), cancellationToken);
        return Ok(ApiResponseFactory.Success(
            accounts,
            "Accounts retrieved successfully."));
    }

    /// <summary>Creates an account for the authenticated user.</summary>
    [HttpPost]
    [ProducesResponseType<ApiResponseDto<AccountResponseDto>>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<ApiResponseDto<AccountResponseDto>>> Create(
        CreateAccountRequestDto request,
        CancellationToken cancellationToken)
    {
        var account = await service.CreateAccountAsync(User.GetUserId(), request, cancellationToken);
        return Created(
            $"/api/accounts/{account.Id}",
            ApiResponseFactory.Success(account, "Account created successfully."));
    }

    /// <summary>Updates an active account owned by the authenticated user.</summary>
    [HttpPut("{accountId:guid}")]
    [ProducesResponseType<ApiResponseDto<AccountResponseDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponseDto<AccountResponseDto>>> Update(
        Guid accountId,
        UpdateAccountRequestDto request,
        CancellationToken cancellationToken)
    {
        var account = await service.UpdateAccountAsync(
            User.GetUserId(), accountId, request, cancellationToken);
        return Ok(ApiResponseFactory.Success(account, "Account updated successfully."));
    }

    /// <summary>Logically deletes an account while preserving its historical data.</summary>
    [HttpDelete("{accountId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid accountId, CancellationToken cancellationToken)
    {
        await service.DeleteAccountAsync(User.GetUserId(), accountId, cancellationToken);
        return NoContent();
    }
}
