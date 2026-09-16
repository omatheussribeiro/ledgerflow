namespace LedgerFlow.Domain.Enums;

/// <summary>Defines whether a financial transaction adds or removes money from an account.</summary>
public enum TransactionType : byte
{
    /// <summary>Money received that increases the account balance.</summary>
    Income = 1,

    /// <summary>Money spent that decreases the account balance.</summary>
    Expense = 2
}
