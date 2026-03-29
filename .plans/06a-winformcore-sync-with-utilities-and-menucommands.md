# 06a - WinformCore Sync with Utilities and MenuCommands

## Objective

Ensure `Beep.DeveloperAssistant.WinformCore` is updated to reflect utility and MenuCommands enhancements so UI workflows expose the same capabilities and contracts.

## Scope

- `Beep.DeveloperAssistant.WinformCore/*`
- Integration points that invoke:
  - `Beep.DeveloperAssistant.Logic/*` utilities
  - `Beep.DeveloperAssistant.Logic.MenuCommands/*`

## Enhancement Workstreams

1. **UI Capability Parity**
   - Add/refresh UI actions for utility and classcreator flows introduced in phases `05a` and `07a`.
   - Ensure all high-value menu operations are reachable from Winform workflows.

2. **Shared Result and Diagnostics Contract**
   - Bind UI status/notifications to standardized utility/menu result model.
   - Add consistent diagnostics panel output for warnings, dry-run summaries, and execution errors.

3. **Workflow Orchestration**
   - Use step-based flow with prerequisite validation before generation/compile/network/file actions.
   - Add pre-execution summary and post-execution artifact/result summary.

4. **Long-running Operation UX**
   - Add cancellation, progress, and timeout display for compile, generation, web/network, and file-heavy operations.
   - Prevent duplicate execution clicks and race conditions in UI actions.

5. **Metadata and Label Alignment**
   - Keep Winform labels/categories consistent with MenuCommands and Nodes naming.
   - Ensure command descriptions shown in UI match command metadata from `07a`.

## Deliverables

- Updated Winform controls/forms reflecting new utility + menu capabilities.
- Unified diagnostics and result rendering components in WinformCore.
- Mapping document: Winform action -> MenuCommand -> Logic utility/service.

## Acceptance Criteria

- Users can execute utility/classcreator scenarios end-to-end from WinformCore with the same behavior as MenuCommands.
- UI diagnostics clearly show success/failure, reasons, and next actions.
- No major capability exists in MenuCommands that is invisible in WinformCore (or it is explicitly deferred).

## Dependencies

- `05a-logic-utilities-modernization-and-coverage.md`
- `05b-developerclasscreatorutilities-tools-alignment-and-partialization.md`
- `07a-menucommands-modernization-and-coverage.md`
- `09-testing-diagnostics-and-dry-run-modes.md`

## Execution Plan (Concrete TODOs)

### Sprint A - UI Coverage Inventory

- [ ] Create `WinformCore-Action-Matrix.md` with rows:
  - Winform control/page
  - UI action/button
  - mapped MenuCommand
  - mapped utility/service method
  - status (implemented/missing)
- [ ] Start with:
  - `uc_CodeConverter`
  - `uc_DeveloperAssistantTemplateDesigner`

### Sprint B - Missing UI Action Implementation

- [ ] Add missing UI actions for top classcreator flows:
  - Create POCO
  - Create Entity
  - Convert POCO -> Entity
  - Validate Code
  - Create DLL
- [ ] Ensure each action invokes the same command/service path as MenuCommands (no duplicated logic).

### Sprint C - Diagnostics and UX Consistency

- [ ] Add standardized operation status panel (start/progress/success/failure).
- [ ] Add pre-run summary and confirmation for mutating operations.
- [ ] Add cancel support for long-running operations where async methods exist.
- [ ] Prevent duplicate clicks during in-flight operations.

### Sprint D - Label and Metadata Alignment

- [ ] Align UI labels with command captions from `DeveloperClassCreatorMenuCommands`.
- [ ] Align icon usage with node/menu icon names for the same capability.
- [ ] Document any intentional label differences in matrix notes.

### Sprint E - Verification

- [ ] Manual verification scenarios:
  - open UI from node and run conversion
  - run equivalent menu command path and compare outputs
  - validate error rendering for invalid input
- [ ] Capture screenshots or notes for each scenario in the matrix doc.

## Definition of Done

- [ ] Winform matrix completed with implemented/missing status.
- [ ] Top 5 classcreator actions are available from Winform and map to existing command/service paths.
- [ ] Diagnostics UX is consistent across at least two major controls/pages.
