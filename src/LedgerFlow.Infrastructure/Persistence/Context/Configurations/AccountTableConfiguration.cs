namespace LedgerFlow.Infrastructure.Persistence.Context.Configurations;

public static class AccountTableConfiguration
{
    public const string TableName = "[dbo].[Accounts]";
    public const string ReadProjection = "a.Id, a.Name, a.Type, a.InitialBalance";
}
