using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using LedgerFlow.Application.Common;
using LedgerFlow.Application.Dtos.Auth;
using LedgerFlow.Application.Dtos.Common;
using LedgerFlow.Application.Dtos.Financial;
using LedgerFlow.Domain.Enums;

namespace LedgerFlow.IntegrationTests;

public sealed class ApiJourneyTests(LedgerFlowApiFactory factory) : IClassFixture<LedgerFlowApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Health_ReturnsHealthyPayload()
    {
        using var response = await _client.GetAsync("/health", CancellationToken.None);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AuthenticatedUser_CanCreateUpdateAndSoftDeleteFinancialResources()
    {
        var email = $"journey-{Guid.NewGuid():N}@ledgerflow.dev";
        using var registration = await _client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequestDto("Integration User", email, "VeryStrong123!"), CancellationToken.None);
        registration.StatusCode.Should().Be(HttpStatusCode.Created);
        var registrationResponse = await registration.Content.ReadFromJsonAsync<ApiResponseDto<AuthResponseDto>>(CancellationToken.None);
        registrationResponse.Should().NotBeNull();
        registrationResponse!.Success.Should().BeTrue();

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", registrationResponse.Data.AccessToken);
        using var creation = await _client.PostAsJsonAsync("/api/accounts",
            new CreateAccountRequestDto("Business checking", AccountType.Checking, 1250m), CancellationToken.None);
        creation.StatusCode.Should().Be(HttpStatusCode.Created);
        var account = (await creation.Content.ReadFromJsonAsync<ApiResponseDto<AccountResponseDto>>(CancellationToken.None))!.Data;

        using var categoryCreation = await _client.PostAsJsonAsync("/api/categories",
            new CreateCategoryRequestDto("Software", TransactionType.Expense, "#123456"), CancellationToken.None);
        categoryCreation.StatusCode.Should().Be(HttpStatusCode.Created);
        var category = (await categoryCreation.Content.ReadFromJsonAsync<ApiResponseDto<CategoryResponseDto>>(CancellationToken.None))!.Data;

        using var transactionCreation = await _client.PostAsJsonAsync("/api/transactions",
            new CreateTransactionRequestDto(
                account.Id,
                category.Id,
                TransactionType.Expense,
                "IDE license",
                50m,
                new DateOnly(2026, 9, 16)), CancellationToken.None);
        transactionCreation.StatusCode.Should().Be(HttpStatusCode.Created);
        var transaction = (await transactionCreation.Content.ReadFromJsonAsync<ApiResponseDto<TransactionResponseDto>>(CancellationToken.None))!.Data;

        using var accountUpdate = await _client.PutAsJsonAsync($"/api/accounts/{account.Id}",
            new UpdateAccountRequestDto("Operating account", AccountType.Checking, 1500m), CancellationToken.None);
        accountUpdate.StatusCode.Should().Be(HttpStatusCode.OK);

        using var categoryUpdate = await _client.PutAsJsonAsync($"/api/categories/{category.Id}",
            new UpdateCategoryRequestDto("Tools", TransactionType.Expense, "#654321"), CancellationToken.None);
        categoryUpdate.StatusCode.Should().Be(HttpStatusCode.OK);

        using var transactionUpdate = await _client.PutAsJsonAsync($"/api/transactions/{transaction.Id}",
            new UpdateTransactionRequestDto(
                account.Id,
                category.Id,
                TransactionType.Expense,
                "Updated IDE license",
                75m,
                new DateOnly(2026, 9, 17)), CancellationToken.None);
        transactionUpdate.StatusCode.Should().Be(HttpStatusCode.OK);

        var transactions = await _client.GetFromJsonAsync<ApiResponseDto<PagedResult<TransactionResponseDto>>>(
            "/api/transactions", CancellationToken.None);
        transactions!.Data.Items.Should().ContainSingle(item =>
            item.Id == transaction.Id && item.Description == "Updated IDE license" && item.Amount == 75m);

        using var dashboardResponse = await _client.GetAsync(
            "/api/dashboard?year=2026&month=9",
            CancellationToken.None);
        dashboardResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var dashboard = await dashboardResponse.Content.ReadFromJsonAsync<ApiResponseDto<DashboardResponseDto>>(
            CancellationToken.None);
        dashboard!.Data.TotalBalance.Should().Be(1425m);

        using var transactionDeletion = await _client.DeleteAsync($"/api/transactions/{transaction.Id}", CancellationToken.None);
        transactionDeletion.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var categoryDeletion = await _client.DeleteAsync($"/api/categories/{category.Id}", CancellationToken.None);
        categoryDeletion.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var accountDeletion = await _client.DeleteAsync($"/api/accounts/{account.Id}", CancellationToken.None);
        accountDeletion.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var accounts = await _client.GetFromJsonAsync<ApiResponseDto<AccountResponseDto[]>>("/api/accounts", CancellationToken.None);
        var categories = await _client.GetFromJsonAsync<ApiResponseDto<CategoryResponseDto[]>>("/api/categories", CancellationToken.None);
        var remainingTransactions = await _client.GetFromJsonAsync<ApiResponseDto<PagedResult<TransactionResponseDto>>>(
            "/api/transactions", CancellationToken.None);
        accounts!.Data.Should().BeEmpty();
        categories!.Data.Should().BeEmpty();
        remainingTransactions!.Data.Items.Should().BeEmpty();
    }
}
