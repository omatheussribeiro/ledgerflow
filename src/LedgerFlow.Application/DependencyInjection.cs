using LedgerFlow.Application.Dtos.Auth;
using LedgerFlow.Application.Dtos.Financial;
using LedgerFlow.Application.Interfaces.Services;
using LedgerFlow.Application.Services;
using LedgerFlow.Application.Validators;
using Microsoft.Extensions.DependencyInjection;

namespace LedgerFlow.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IFinancialService, FinancialService>();
        services.AddScoped<IRequestValidator<RegisterRequestDto>, RegisterRequestValidator>();
        services.AddScoped<IRequestValidator<LoginRequestDto>, LoginRequestValidator>();
        services.AddScoped<IRequestValidator<RefreshRequestDto>, RefreshRequestValidator>();
        services.AddScoped<IRequestValidator<CreateAccountRequestDto>, CreateAccountRequestValidator>();
        services.AddScoped<IRequestValidator<UpdateAccountRequestDto>, UpdateAccountRequestValidator>();
        services.AddScoped<IRequestValidator<CreateCategoryRequestDto>, CreateCategoryRequestValidator>();
        services.AddScoped<IRequestValidator<UpdateCategoryRequestDto>, UpdateCategoryRequestValidator>();
        services.AddScoped<IRequestValidator<CreateTransactionRequestDto>, CreateTransactionRequestValidator>();
        services.AddScoped<IRequestValidator<UpdateTransactionRequestDto>, UpdateTransactionRequestValidator>();
        services.AddScoped<IRequestValidator<TransactionFilterDto>, TransactionFilterValidator>();
        return services;
    }
}
