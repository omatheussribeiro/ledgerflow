using System.ComponentModel.DataAnnotations;
using LedgerFlow.Domain.Enums;

namespace LedgerFlow.Application.Dtos.Financial;

/// <summary>Data required to create a financial account.</summary>
public sealed record CreateAccountRequestDto(
    [Required, StringLength(100)] string Name,
    [EnumDataType(typeof(AccountType))] AccountType Type,
    [Range(-999999999.99, 999999999.99)] decimal InitialBalance);

/// <summary>Data required to update an existing financial account.</summary>
public sealed record UpdateAccountRequestDto(
    [Required, StringLength(100)] string Name,
    [EnumDataType(typeof(AccountType))] AccountType Type,
    [Range(-999999999.99, 999999999.99)] decimal InitialBalance);

/// <summary>Account details with the current calculated balance.</summary>
public sealed record AccountResponseDto(
    Guid Id,
    string Name,
    AccountType Type,
    decimal InitialBalance,
    decimal Balance,
    bool IsActive);
