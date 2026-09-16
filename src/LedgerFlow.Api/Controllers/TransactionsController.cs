using LedgerFlow.Api.Extensions;
using LedgerFlow.Application.Common;
using LedgerFlow.Application.Dtos.Common;
using LedgerFlow.Application.Dtos.Financial;
using LedgerFlow.Application.Interfaces.Services;
using LedgerFlow.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LedgerFlow.Api.Controllers;

/// <summary>Queries and records income and expense movements.</summary>
[ApiController]
[Authorize]
[Route("api/transactions")]
[Produces("application/json")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status503ServiceUnavailable)]
public sealed class TransactionsController(IFinancialService service) : ControllerBase
{
    /// <summary>Returns a filtered, paginated ledger for the authenticated user.</summary>
    /// <param name="from">Inclusive start date.</param>
    /// <param name="to">Inclusive end date.</param>
    /// <param name="accountId">Optional account identifier.</param>
    /// <param name="categoryId">Optional category identifier.</param>
    /// <param name="type">Optional transaction type: 1 for income or 2 for expense.</param>
    /// <param name="search">Optional description search, limited to 160 characters.</param>
    /// <param name="page">One-based page number.</param>
    /// <param name="pageSize">Page size; values are constrained to 1-100.</param>
    /// <param name="cancellationToken">Cancels the request when the client disconnects.</param>
    [HttpGet]
    [ProducesResponseType<ApiResponseDto<PagedResult<TransactionResponseDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponseDto<PagedResult<TransactionResponseDto>>>> Get(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] Guid? accountId,
        [FromQuery] Guid? categoryId,
        [FromQuery] TransactionType? type,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var transactions = await service.GetTransactionsAsync(
            User.GetUserId(),
            new TransactionFilterDto(from, to, accountId, categoryId, type, search, page, pageSize),
            cancellationToken);
        return Ok(ApiResponseFactory.Success(
            transactions,
            "Transactions retrieved successfully."));
    }

    /// <summary>Records an income or expense transaction.</summary>
    /// <remarks>The account and category must belong to the user, and category type must match transaction type.</remarks>
    [HttpPost]
    [ProducesResponseType<ApiResponseDto<TransactionResponseDto>>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<ApiResponseDto<TransactionResponseDto>>> Create(
        CreateTransactionRequestDto request,
        CancellationToken cancellationToken)
    {
        var transaction = await service.CreateTransactionAsync(
            User.GetUserId(),
            request,
            cancellationToken);
        return Created(
            $"/api/transactions/{transaction.Id}",
            ApiResponseFactory.Success(
                transaction,
                "Transaction recorded successfully."));
    }
}
