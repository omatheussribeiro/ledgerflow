# ADR 001: Use Dapper for persistence

## Context

LedgerFlow must demonstrate explicit SQL design, predictable queries and report-oriented read models without the change-tracking behavior of a full ORM.

## Decision

Use Dapper behind application-owned repository interfaces. Keep SQL next to its repository, always parameterize values, use `CommandDefinition` for cancellation, and use explicit database transactions for token rotation and other multi-write operations. Schema evolution is handled by DbUp, not Entity Framework.

## Consequences

The team owns SQL mapping, schema compatibility and query performance. There is more code than with EF Core for simple CRUD, but complex joins, aggregation, CTEs and query plans stay visible. EF Core would remain a reasonable choice for domains dominated by aggregate persistence and high CRUD throughput.
