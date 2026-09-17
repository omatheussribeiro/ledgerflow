using LedgerFlow.Application.Interfaces.Repositories;
using LedgerFlow.Application.Interfaces.Services;
using LedgerFlow.Infrastructure.Persistence.Context;
using LedgerFlow.Infrastructure.Persistence.Repositories;
using LedgerFlow.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LedgerFlow.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        Func<string> connectionStringFactory)
    {
        ArgumentNullException.ThrowIfNull(connectionStringFactory);

        services.AddDbContext<LedgerFlowDbContext>((_, options) =>
        {
            var connectionString = connectionStringFactory();
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException("A database connection string is required.");

            options.UseSqlServer(connectionString);
        });
        services.AddScoped<DatabaseInitializer>();
        services.AddScoped<IFinancialRepository, FinancialRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddSingleton<IPasswordService, Pbkdf2PasswordService>();
        services.AddScoped<DevelopmentDataSeeder>();
        return services;
    }
}
