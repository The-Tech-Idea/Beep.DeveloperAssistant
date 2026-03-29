# 06 - WinformCore Experience and Workflows

## Objective

Upgrade Winform UX flows for guided generation, validation, and diagnostics.

## Scope

- `uc_CodeConverter`
- `uc_DeveloperAssistantTemplateDesigner`

## Planned Enhancements

- Add step-based workflow guidance with prerequisite checks.
- Add preview pane for generated outputs before write/compile.
- Add diagnostics panel for errors, warnings, and helper/provider decisions.
- Ensure operation cancellation/progress for long-running tasks.
- Detailed Winform synchronization with utilities/menu commands is tracked in `06a-winformcore-sync-with-utilities-and-menucommands.md`.

## Execution TODOs

- [ ] Create `Winform-Workflow-Spec.md` for each major page/control.
- [ ] Implement consistent workflow stages in UI:
  - Input
  - Validate
  - Preview
  - Execute
  - Result
- [ ] Add progress + cancel wiring to all long-running operations.
- [ ] Add unified diagnostics panel component for reusable display.
- [ ] Validate UI action parity with command/node mappings.

## Verification Criteria

- [ ] At least 2 primary Winform controls implement the full stage flow.
- [ ] UI preview appears before mutation for generation actions.
- [ ] Progress/cancel works on at least one compile and one conversion flow.
