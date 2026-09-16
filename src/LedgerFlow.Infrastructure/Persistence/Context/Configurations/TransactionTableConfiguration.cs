namespace LedgerFlow.Infrastructure.Persistence.Context.Configurations;

public static class TransactionTableConfiguration
{
    public const string TableName = "[dbo].[Transactions]";
    public const string ReadProjection =
        "t.Id, t.AccountId, a.Name AS AccountName, t.CategoryId, c.Name AS CategoryName, " +
        "c.Color AS CategoryColor, t.Type, t.Description, t.Amount, t.OccurredOn, t.Status, t.Notes";
}
