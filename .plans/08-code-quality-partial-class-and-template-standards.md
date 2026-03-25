# 08 - Code Quality, Partial Class, and Template Standards

## Objective

Enforce clean structured coding with partial-class decomposition and consistent generation templates.

## Mandatory Standards

- Single responsibility per class/file.
- Partial classes by concern for large orchestrators and UI workflows.
- No mixed catch-all utility files for unrelated behaviors.
- Template/profile driven code output for repeatability.
- Public API docs and concise internal comments for non-obvious logic.

## Partial-Class Pattern

For large classes, split by concern:

- `*.Lifecycle.cs`
- `*.Generation.cs`
- `*.Validation.cs`
- `*.Diagnostics.cs`
- `*.UiBindings.cs`
