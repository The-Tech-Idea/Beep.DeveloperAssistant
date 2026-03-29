# 07b - Nodes Sync with Utilities and WinformCore

## Objective

Update `Beep.DeveloperAssistant.Nodes` so node tree structure, actions, and metadata reflect utility, MenuCommands, and WinformCore enhancements consistently.

## Scope

- `Beep.DeveloperAssistant.Nodes/*`
- Node-to-command wiring for classcreator and utility capabilities
- Node metadata used for display/search/grouping

## Enhancement Workstreams

1. **Node Coverage Alignment**
   - Build Node -> MenuCommand -> Utility coverage map.
   - Add missing nodes for utility capabilities introduced in `05a`.
   - Add/adjust node actions for enhanced classcreator flows and dry-run diagnostics.

2. **Node Metadata Standardization**
   - Normalize node titles, categories, descriptions, and tags.
   - Align naming with Winform labels and MenuCommands metadata to reduce user confusion.

3. **Thin Node Handler Pattern**
   - Keep node classes focused on routing and context binding.
   - Delegate actual execution to MenuCommands/Logic services.
   - Remove duplicated validation and transformation logic from node classes.

4. **Context and Parameter Propagation**
   - Ensure selected node context provides required inputs (project, paths, profile/options) without ad hoc mapping.
   - Validate parameter transformation consistency between Nodes and Winform entry points.

5. **Discoverability and Usability**
   - Improve node grouping for utility-heavy scenarios.
   - Add search keywords and concise descriptions for faster discovery.

## Deliverables

- Updated nodes for utility/classcreator coverage parity.
- Coverage matrix doc linking node actions to MenuCommands and utility methods.
- Initial concrete matrix: `07c-coverage-matrix-developerclasscreatormenucommands-pocotoentity.md`.
- Standardized node metadata conventions adopted across node classes.

## Acceptance Criteria

- Every major utility/classcreator flow has a node entry path (or explicit deferment).
- Node actions call shared command/service paths, not duplicated business logic.
- Node names and behavior are consistent with Winform and MenuCommands surfaces.

## Dependencies

- `05a-logic-utilities-modernization-and-coverage.md`
- `05b-developerclasscreatorutilities-tools-alignment-and-partialization.md`
- `06a-winformcore-sync-with-utilities-and-menucommands.md`
- `07a-menucommands-modernization-and-coverage.md`

## Execution Plan (Concrete TODOs)

### Sprint A - Node Inventory and Routing Baseline

- [ ] Create `Nodes-Inventory.md` for `Beep.DeveloperAssistant.Nodes/*` with columns:
  - node class
  - `AddinAttribute` name/caption
  - node actions (`BranchActions`)
  - routed menu command (if any)
  - status (implemented/stubbed)
- [ ] Identify all node classes with stubbed `ExecuteBranchAction` / `MenuItemClicked`.
- [ ] Prioritize top 5 node classes by user visibility and command density.

### Sprint B - Implement Node Routing Pattern

- [ ] Create a consistent pattern in each prioritized node:
  - initialize `BranchActions` in constructor
  - route through `ExecuteBranchAction`
  - delegate heavy work to MenuCommands/Logic
- [ ] Apply the same pattern used in `POCOtoEntityCodeConverter` to:
  - `CodeGeneratorBranch.cs` (first)
  - next two highest-priority node classes from inventory
- [ ] Add null-safe context checks for `DMEEditor`, app manager, and tree references.

### Sprint C - Metadata and Discoverability

- [ ] Normalize node naming format:
  - action verbs in captions
  - consistent icon naming conventions
  - consistent object type naming pattern
- [ ] Ensure `BranchText` and command captions match MenuCommands labels.
- [ ] Add/clean search keywords where node metadata supports it.

### Sprint D - Verification

- [ ] Manual test matrix:
  - open node action
  - one routed generation action
  - one routed validation action
  - one error-path action (missing context)
- [ ] Update `07c` or create additional `07c-*` matrix docs for each implemented node cluster.

## Definition of Done

- [ ] At least 3 node classes have working action routing (not stubs).
- [ ] Node labels and menu labels match for routed capabilities.
- [ ] Inventory doc reflects implemented vs pending node coverage.
