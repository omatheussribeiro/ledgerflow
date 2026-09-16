# ADR 002: Use SQL Server

## Context

Financial data needs precise decimal types, strong constraints, transactions, mature indexing and a database that is straightforward to run in development and CI.

## Decision

Use SQL Server 2022 with `decimal(19,2)` monetary values, UTC `datetimeoffset`, foreign keys, unique constraints, check constraints and workload-driven nonclustered indexes.

## Consequences

SQL Server provides a strong operational baseline and rich query tooling. Local development requires SQL Server or Docker, and SQL dialect-specific queries reduce portability. Portability is not currently worth weakening database-level guarantees.
