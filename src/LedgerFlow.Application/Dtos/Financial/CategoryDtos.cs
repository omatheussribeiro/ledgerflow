using System.ComponentModel.DataAnnotations;
using LedgerFlow.Domain.Enums;

namespace LedgerFlow.Application.Dtos.Financial;

/// <summary>Data required to create an income or expense category.</summary>
public sealed record CreateCategoryRequestDto(
    [Required, StringLength(80)] string Name,
    [EnumDataType(typeof(TransactionType))] TransactionType Type,
    [RegularExpression("^#[0-9a-fA-F]{6}$")] string? Color);

/// <summary>Data required to update an existing category.</summary>
public sealed record UpdateCategoryRequestDto(
    [Required, StringLength(80)] string Name,
    [EnumDataType(typeof(TransactionType))] TransactionType Type,
    [RegularExpression("^#[0-9a-fA-F]{6}$")] string? Color);

/// <summary>Category details returned by category and transaction operations.</summary>
public sealed record CategoryResponseDto(Guid Id, string Name, TransactionType Type, string Color);
