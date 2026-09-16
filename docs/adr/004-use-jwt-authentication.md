# ADR 004: Use short-lived JWT access tokens and rotating refresh tokens

## Context

The SPA needs stateless API authorization while still allowing sessions to continue without long-lived bearer tokens.

## Decision

Issue 15-minute signed access tokens and opaque seven-day refresh tokens. Store only SHA-256 refresh-token hashes, rotate them atomically, allow revocation, and hash passwords with salted PBKDF2-SHA512. Authorize API resources by the authenticated user ID and policies/roles.

## Consequences

Access-token validation is cheap and horizontally scalable. Revocation of an already-issued access token is not immediate, which is bounded by its short lifetime. Refresh-token storage and rotation add database work but reduce replay risk.
