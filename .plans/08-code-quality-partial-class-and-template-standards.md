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

## Target Files

- `Beep.DeveloperAssistant.Logic/DeveloperClassCreatorUtilities.cs`
- high-volume classes in MenuCommands and WinformCore
- related model/template files under `Beep.DeveloperAssistant.Logic.Models`

## Execution TODOs

- [ ] Define partial-class threshold rule (e.g., >600 lines or >20 public methods).
- [ ] Create `Partialization-Backlog.md` with top 10 classes to split.
- [ ] Apply partial split to `DeveloperClassCreatorUtilities` (tracked in `05b`).
- [ ] Standardize template placeholders and naming across generation methods.
- [ ] Add style checklist for comments/docs/error messages in generated code methods.

## Verification Criteria

- [ ] Target bloated classes are split by concern with no behavior regressions.
- [ ] Template naming and placeholders are consistent across generation features.
- [ ] New enhancements follow partial-class pattern by default.
