# ADR 001: Use EF Core and Dapper behind the persistence boundary

## Context

LedgerFlow needs predictable report-oriented queries while also benefiting from a strongly typed entity model for aggregate mutations.

## Decision

Use a hybrid persistence implementation behind application-owned repository interfaces:

- EF Core `DbSet`s perform financial entity creation, editing and logical deletion.
- `ModelBuilder.Entity` configurations define keys, property types, relationships, indexes, constraints and logical-deletion query filters.
- Dapper handles aggregate, dashboard, paginated and authentication queries where explicit SQL is valuable.
- DbUp scripts, rather than EF Core migrations, remain the authoritative schema history.
- Explicit database transactions remain in use for refresh-token rotation and other multi-write operations.

## Consequences

The team must keep the EF model, Dapper projections and DbUp scripts synchronized. In exchange, entity mutations use domain behavior and change tracking while complex joins, aggregation, CTEs and query plans stay visible. Both paths remain internal to Infrastructure and share the same scoped `LedgerFlowDbContext` connection configuration.
