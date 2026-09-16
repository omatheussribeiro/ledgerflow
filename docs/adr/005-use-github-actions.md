# ADR 005: Use GitHub Actions and GHCR

## Context

Every pull request needs repeatable backend/frontend checks, and releases need immutable, versioned container images close to the source repository.

## Decision

Use separate backend and frontend CI jobs on pushes and pull requests. Publish API and web images to GitHub Container Registry when a semantic version tag is pushed. Authenticate with the scoped repository `GITHUB_TOKEN`.

## Consequences

Contributors get fast, isolated feedback and releases are reproducible. Testcontainers makes backend CI depend on Docker availability, which GitHub-hosted Ubuntu runners provide. Protected branches should require both jobs.
