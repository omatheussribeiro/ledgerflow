# ADR 006: Use logical deletion for financial records

## Context

Accounts, categories and transactions participate in financial history and reporting. Physically deleting them would remove audit context or require cascading destructive changes across related records.

## Decision

Store a nullable UTC `DeletedAt` on accounts, categories and transactions. The authenticated `DELETE` endpoints set this timestamp instead of issuing SQL `DELETE`; accounts are also marked inactive. EF Core global query filters protect entity operations, and every Dapper read that represents active data explicitly filters deleted rows. Existing foreign keys remain unchanged so historical relationships are retained.

Unique account and category indexes are filtered by `DeletedAt IS NULL`. This preserves uniqueness among active records while allowing a name to be reused after logical deletion.

## Consequences

Deleted accounts and categories no longer appear in active lists or selectors, and deleted accounts leave the current total balance. Deleted transactions no longer appear or affect balances, dashboards and reports. A non-deleted historical transaction continues to retain and display its account/category relationship if that parent is later deleted. All rows remain available for audit or a future restore workflow. Any new read query must deliberately decide whether it needs active-only data or historical data, and the EF model, Dapper SQL and DbUp schema must remain synchronized.
