namespace LedgerFlow.Infrastructure.Persistence.Context.Configurations;

public static class UserTableConfiguration
{
    public const string TableName = "[dbo].[Users]";
    public const string ReadProjection = "Id, Name, Email, PasswordHash, Role, CreatedAt";
}
