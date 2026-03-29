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

## Target Files

- `Beep.DeveloperAssistant.Logic/*`
- `Beep.DeveloperAssistant.Logic.MenuCommands/*`
- `Beep.DeveloperAssistant.Nodes/*`
- `Beep.DeveloperAssistant.WinformCore/*`

## Execution TODOs

- [ ] Create `Testing-Matrix.md` with rows for generation, conversion, compile, routing, UI flows.
- [ ] Add dry-run flags/options for mutating utility methods and surface through menu/node/ui.
- [ ] Add diagnostic payload model shared by logic/menu/node/ui.
- [ ] Implement baseline automated tests for:
  - POCO generation
  - POCO->Entity conversion
  - DLL build from files
  - node action routing
- [ ] Add golden-output snapshots for selected generation outputs.

## Verification Criteria

- [ ] Dry-run is available for top mutating operations and reports intended file outputs.
- [ ] Diagnostics include warnings/errors + decision trace where relevant.
- [ ] Test matrix shows passing status for critical flows before rollout phase.
