using FluentAssertions;
using LedgerFlow.Api.Controllers;
using LedgerFlow.Application.Interfaces.Services;
using LedgerFlow.Infrastructure.Persistence.Repositories;
using NetArchTest.Rules;

namespace LedgerFlow.ArchitectureTests;

public sealed class LayerDependencyTests
{
    [Fact]
    public void Domain_DoesNotDependOnOuterLayers()
    {
        var result = Types.InAssembly(typeof(LedgerFlow.Domain.Financial.Account).Assembly)
            .ShouldNot().HaveDependencyOnAny("LedgerFlow.Application", "LedgerFlow.Infrastructure", "LedgerFlow.Api")
            .GetResult();
        result.IsSuccessful.Should().BeTrue(string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void Application_DoesNotDependOnDeliveryOrInfrastructure()
    {
        var result = Types.InAssembly(typeof(LedgerFlow.Application.Services.FinancialService).Assembly)
            .ShouldNot().HaveDependencyOnAny("LedgerFlow.Infrastructure", "LedgerFlow.Api")
            .GetResult();
        result.IsSuccessful.Should().BeTrue(string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void Controllers_HaveControllerSuffix()
    {
        var result = Types.InAssembly(typeof(Program).Assembly)
            .That().ResideInNamespace("LedgerFlow.Api.Controllers")
            .Should().HaveNameEndingWith("Controller")
            .GetResult();
        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void DomainEnums_ResideInEnumsNamespace()
    {
        var enumTypes = typeof(LedgerFlow.Domain.Financial.Account).Assembly
            .GetTypes()
            .Where(type => type.IsEnum);

        enumTypes.Should().NotBeEmpty()
            .And.OnlyContain(type => type.Namespace == "LedgerFlow.Domain.Enums");
    }

    [Fact]
    public void Controllers_DependOnApplicationServiceInterfaces()
    {
        ConstructorDependency(typeof(AuthController)).Should().Be<IAuthService>();
        ConstructorDependency(typeof(AccountsController)).Should().Be<IFinancialService>();
        ConstructorDependency(typeof(CategoriesController)).Should().Be<IFinancialService>();
        ConstructorDependency(typeof(DashboardController)).Should().Be<IFinancialService>();
        ConstructorDependency(typeof(TransactionsController)).Should().Be<IFinancialService>();
    }

    [Fact]
    public void RepositoryImplementations_ResideInPersistenceRepositories()
    {
        var result = Types.InAssembly(typeof(FinancialRepository).Assembly)
            .That().HaveNameEndingWith("Repository")
            .Should().ResideInNamespace("LedgerFlow.Infrastructure.Persistence.Repositories")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(string.Join(", ", result.FailingTypeNames ?? []));
    }

    private static Type ConstructorDependency(Type controllerType) =>
        controllerType.GetConstructors().Single().GetParameters().Single().ParameterType;
}
