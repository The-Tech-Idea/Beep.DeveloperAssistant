# 00 - Overview: Capability Gap Matrix

## Objective

Baseline current `Beep.DeveloperAssistant` capabilities and define phased enhancements that include referenced BeepDM tooling and helper patterns.

## Gap Matrix

| Capability Area | Current State | Gap | Target |
|---|---|---|---|
| Class generation | Utility wrappers exist | Limited unified orchestration with `ClassCreator` partials | Structured integration with advanced/database/entity/webapi/testing flows |
| RDBMS helper usage | No explicit universal helper strategy | Potential direct/inline provider branching | Runtime helper resolution via universal helper factory |
| Environment bootstrap | Not formalized in assistant flows | Folder/config/query bootstrap can be inconsistent | EnvironmentService-guided initialization and validation |
| UI workflows | Winform tools exist | Workflow and diagnostics consistency can improve | Guided, validated, dry-run capable UI flows |
| Command/node alignment | Commands and nodes exist | Coverage and discoverability not fully mapped | Feature-complete menu/node command mapping |
| Quality and maintainability | Mixed patterns | Risk of large/bloated classes | Partial-class-by-concern standards and cleaner boundaries |

## Primary Source Surfaces

- `Beep.DeveloperAssistant.WinformCore/*`
- `Beep.DeveloperAssistant.Logic/*`
- `Beep.DeveloperAssistant.Logic.MenuCommands/*`
- `Beep.DeveloperAssistant.Nodes/*`
- `BeepDM/DataManagementEngineStandard/Tools/ClassCreator*.cs`
- `BeepDM/.cursor/* universal helper/environmentservice skill guidance`

## Execution Backlog (Concrete)

- [ ] Build `Phase-Backlog-Index.md` listing all numbered phases and current completion percentage.
- [ ] Add owner + target branch + target milestone fields for each phase doc.
- [ ] Add dependency graph section (blocking phase vs blocked phases).
- [ ] Add status legend used by all plan docs:
  - `Not Started`
  - `In Progress`
  - `Blocked`
  - `Done`
- [ ] Link this overview to `05b`, `06a`, `07a`, `07b`, `07c` as implementation-critical plans.

## Verification Criteria

- [ ] Every phase document includes TODO checklist + target files + verification criteria.
- [ ] Every cross-phase dependency is explicit (no implied dependencies).
- [ ] Overview matches current execution order in `README.md`.
