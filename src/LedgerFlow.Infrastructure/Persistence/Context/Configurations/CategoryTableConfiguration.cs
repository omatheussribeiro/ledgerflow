namespace LedgerFlow.Infrastructure.Persistence.Context.Configurations;

public static class CategoryTableConfiguration
{
    public const string TableName = "[dbo].[Categories]";
    public const string ReadProjection = "c.Id, c.Name, c.Type, c.Color";
}
