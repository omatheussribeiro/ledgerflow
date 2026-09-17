using System.Text.RegularExpressions;
using LedgerFlow.Application.Dtos.Financial;
using LedgerFlow.Application.Interfaces.Services;

namespace LedgerFlow.Application.Validators;

public sealed partial class CreateAccountRequestValidator : IRequestValidator<CreateAccountRequestDto>
{
    public void ValidateAndThrow(CreateAccountRequestDto request)
    {
        var errors = new ValidationErrors();
        var name = request.Name?.Trim() ?? string.Empty;
        errors.AddIf(name.Length is < 1 or > 100, nameof(request.Name), "Account name must contain between 1 and 100 characters.");
        errors.AddIf(!Enum.IsDefined(request.Type), nameof(request.Type), "Account type is invalid.");
        errors.AddIf(request.InitialBalance is < -999999999.99m or > 999999999.99m, nameof(request.InitialBalance), "Initial balance is outside the supported range.");
        errors.ThrowIfInvalid();
    }
}

public sealed class UpdateAccountRequestValidator : IRequestValidator<UpdateAccountRequestDto>
{
    public void ValidateAndThrow(UpdateAccountRequestDto request) =>
        new CreateAccountRequestValidator().ValidateAndThrow(
            new CreateAccountRequestDto(request.Name, request.Type, request.InitialBalance));
}

public sealed partial class CreateCategoryRequestValidator : IRequestValidator<CreateCategoryRequestDto>
{
    [GeneratedRegex("^#[0-9a-fA-F]{6}$", RegexOptions.CultureInvariant)]
    private static partial Regex HexColorRegex();

    public void ValidateAndThrow(CreateCategoryRequestDto request)
    {
        var errors = new ValidationErrors();
        var name = request.Name?.Trim() ?? string.Empty;
        errors.AddIf(name.Length is < 1 or > 80, nameof(request.Name), "Category name must contain between 1 and 80 characters.");
        errors.AddIf(!Enum.IsDefined(request.Type), nameof(request.Type), "Transaction type is invalid.");
        errors.AddIf(
            !string.IsNullOrWhiteSpace(request.Color) && !HexColorRegex().IsMatch(request.Color),
            nameof(request.Color),
            "Color must be a six-digit hexadecimal value.");
        errors.ThrowIfInvalid();
    }
}

public sealed class UpdateCategoryRequestValidator : IRequestValidator<UpdateCategoryRequestDto>
{
    public void ValidateAndThrow(UpdateCategoryRequestDto request) =>
        new CreateCategoryRequestValidator().ValidateAndThrow(
            new CreateCategoryRequestDto(request.Name, request.Type, request.Color));
}

public sealed class CreateTransactionRequestValidator : IRequestValidator<CreateTransactionRequestDto>
{
    public void ValidateAndThrow(CreateTransactionRequestDto request)
    {
        var errors = new ValidationErrors();
        var description = request.Description?.Trim() ?? string.Empty;
        errors.AddIf(request.AccountId == Guid.Empty, nameof(request.AccountId), "A valid account is required.");
        errors.AddIf(request.CategoryId == Guid.Empty, nameof(request.CategoryId), "A valid category is required.");
        errors.AddIf(!Enum.IsDefined(request.Type), nameof(request.Type), "Transaction type is invalid.");
        errors.AddIf(description.Length is < 1 or > 160, nameof(request.Description), "Description must contain between 1 and 160 characters.");
        errors.AddIf(request.Amount is < 0.01m or > 999999999.99m, nameof(request.Amount), "Amount must be between 0.01 and 999999999.99.");
        errors.AddIf(request.OccurredOn == default, nameof(request.OccurredOn), "A valid occurrence date is required.");
        errors.AddIf(!Enum.IsDefined(request.Status), nameof(request.Status), "Transaction status is invalid.");
        errors.AddIf(request.Notes?.Length > 500, nameof(request.Notes), "Notes cannot exceed 500 characters.");
        errors.ThrowIfInvalid();
    }
}

public sealed class UpdateTransactionRequestValidator : IRequestValidator<UpdateTransactionRequestDto>
{
    public void ValidateAndThrow(UpdateTransactionRequestDto request) =>
        new CreateTransactionRequestValidator().ValidateAndThrow(
            new CreateTransactionRequestDto(
                request.AccountId,
                request.CategoryId,
                request.Type,
                request.Description,
                request.Amount,
                request.OccurredOn,
                request.Status,
                request.Notes));
}

public sealed class TransactionFilterValidator : IRequestValidator<TransactionFilterDto>
{
    public void ValidateAndThrow(TransactionFilterDto request)
    {
        var errors = new ValidationErrors();
        errors.AddIf(request.From > request.To, nameof(request.From), "The start date cannot be after the end date.");
        errors.AddIf(request.Type is not null && !Enum.IsDefined(request.Type.Value), nameof(request.Type), "Transaction type is invalid.");
        errors.AddIf(request.Search?.Length > 160, nameof(request.Search), "Search cannot exceed 160 characters.");
        errors.ThrowIfInvalid();
    }
}
