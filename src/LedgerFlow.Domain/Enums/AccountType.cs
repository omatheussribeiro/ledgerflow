namespace LedgerFlow.Domain.Enums;

/// <summary>Defines the supported types of financial accounts.</summary>
public enum AccountType : byte
{
    /// <summary>A checking account used for everyday financial transactions.</summary>
    Checking = 1,

    /// <summary>A savings account intended for storing reserved funds.</summary>
    Savings = 2,

    /// <summary>Physical cash or another balance held outside a financial institution.</summary>
    Cash = 3,

    /// <summary>An investment account containing assets intended to generate returns.</summary>
    Investment = 4
}
