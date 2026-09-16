namespace LedgerFlow.Domain.Enums;

/// <summary>Defines the processing state of a financial transaction.</summary>
public enum TransactionStatus : byte
{
    /// <summary>The transaction is registered but has not yet affected the account balance.</summary>
    Pending = 1,

    /// <summary>The transaction is confirmed and affects the account balance.</summary>
    Cleared = 2,

    /// <summary>The transaction was cancelled and does not affect the account balance.</summary>
    Cancelled = 3
}
