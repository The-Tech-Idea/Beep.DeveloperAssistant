# 09 - Testing, Diagnostics, and Dry-Run Modes

## Objective

Increase confidence and developer usability through deterministic diagnostics and testability.

## Planned Enhancements

- Snapshot/golden-output testing for generation flows.
- Parser/conversion regression tests for entity and annotation scenarios.
- Dry-run mode to preview outputs and file writes without changes.
- Diagnostic report model:
  - inferred keys/relationships
  - helper/provider decisions
  - warnings/fallback paths

## Acceptance Criteria

- Every generation operation can run in dry-run mode.
- Diagnostics are available in UI and command outputs.
