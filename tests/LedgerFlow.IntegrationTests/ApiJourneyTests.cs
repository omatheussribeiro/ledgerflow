using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
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
    public async Task Register_ThenCreateAndReadAccount_CompletesAuthenticatedJourney()
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

        var accountsResponse = await _client.GetFromJsonAsync<ApiResponseDto<AccountResponseDto[]>>(
            "/api/accounts",
            CancellationToken.None);
        accountsResponse.Should().NotBeNull();
        accountsResponse!.Success.Should().BeTrue();
        accountsResponse.Data.Should().ContainSingle(x => x.Name == "Business checking" && x.Balance == 1250m);
    }
}
