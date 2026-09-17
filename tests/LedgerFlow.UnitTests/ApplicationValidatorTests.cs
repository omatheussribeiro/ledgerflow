using FluentAssertions;
using LedgerFlow.Application.Common;
using LedgerFlow.Application.Dtos.Auth;
using LedgerFlow.Application.Dtos.Financial;
using LedgerFlow.Application.Validators;
using LedgerFlow.Domain.Enums;

namespace LedgerFlow.UnitTests;

public sealed class ApplicationValidatorTests
{
    [Fact]
    public void Register_WithWeakPassword_ReturnsFieldValidationErrors()
    {
        var validator = new RegisterRequestValidator();
        var request = new RegisterRequestDto("Test User", "test@example.com", "weak");

        var action = () => validator.ValidateAndThrow(request);

        action.Should().Throw<ApplicationValidationException>()
            .Which.Errors.Should().ContainKey(nameof(RegisterRequestDto.Password));
    }

    [Fact]
    public void CreateTransaction_WithValidValues_PassesValidation()
    {
        var validator = new CreateTransactionRequestValidator();
        var request = new CreateTransactionRequestDto(
            Guid.NewGuid(),
            Guid.NewGuid(),
            TransactionType.Expense,
            "Cloud hosting",
            50m,
            new DateOnly(2026, 9, 15));

        var action = () => validator.ValidateAndThrow(request);

        action.Should().NotThrow();
    }

    [Fact]
    public void TransactionFilter_WithReversedDates_ReturnsValidationError()
    {
        var validator = new TransactionFilterValidator();
        var request = new TransactionFilterDto(
            new DateOnly(2026, 9, 16),
            new DateOnly(2026, 9, 15),
            null,
            null,
            null,
            null);

        var action = () => validator.ValidateAndThrow(request);

        action.Should().Throw<ApplicationValidationException>()
            .Which.Errors.Should().ContainKey(nameof(TransactionFilterDto.From));
    }

    [Fact]
    public void UpdateAccount_WithBlankName_ReturnsValidationError()
    {
        var validator = new UpdateAccountRequestValidator();
        var request = new UpdateAccountRequestDto(" ", AccountType.Checking, 0m);

        var action = () => validator.ValidateAndThrow(request);

        action.Should().Throw<ApplicationValidationException>()
            .Which.Errors.Should().ContainKey(nameof(UpdateAccountRequestDto.Name));
    }
}
