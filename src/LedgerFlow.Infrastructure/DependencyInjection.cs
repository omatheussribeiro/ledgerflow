using LedgerFlow.Application.Interfaces.Repositories;
using LedgerFlow.Application.Interfaces.Services;
using LedgerFlow.Infrastructure.Persistence.Context;
using LedgerFlow.Infrastructure.Persistence.Repositories;
using LedgerFlow.Infrastructure.Security;
using Microsoft.Extensions.DependencyInjection;

namespace LedgerFlow.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        Func<string> connectionStringFactory)
    {
        ArgumentNullException.ThrowIfNull(connectionStringFactory);

        services.AddSingleton(_ => new LedgerFlowDbContext(connectionStringFactory()));
        services.AddSingleton<DatabaseInitializer>();
        services.AddScoped<IFinancialRepository, FinancialRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddSingleton<IPasswordService, Pbkdf2PasswordService>();
        services.AddScoped<DevelopmentDataSeeder>();
        return services;
    }
}
