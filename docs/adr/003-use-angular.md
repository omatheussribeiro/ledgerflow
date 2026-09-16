# ADR 003: Use Angular

## Context

The client needs structured routing, forms, HTTP integration, reusable UI primitives and a scalable feature layout.

## Decision

Use Angular 22 with standalone components, lazy routes, signals for local reactive state, reactive forms, functional guards/interceptors and strict TypeScript.

## Consequences

Angular supplies consistent conventions for a growing application and strong test tooling. Its framework surface is larger than a lightweight view library, but that cost is justified for a multi-feature financial workspace.
