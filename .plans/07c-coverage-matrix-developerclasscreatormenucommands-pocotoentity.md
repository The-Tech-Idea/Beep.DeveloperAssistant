# 07c - Coverage Matrix: `DeveloperClassCreatorMenuCommands` vs `POCOtoEntityCodeConverter`

## Objective

Provide the first concrete, code-based coverage matrix for:

- `Beep.DeveloperAssistant.Logic.MenuCommands/DeveloperClassCreatorMenuCommands.cs`
- `Beep.DeveloperAssistant.Nodes/POCOtoEntityCodeConverter.cs`

This matrix captures **current mappings** and **verified missing items** from the current implementation.

## Current Command Surface (MenuCommands)

`DeveloperClassCreatorMenuCommands` currently exposes these menu commands via `[CommandAttribute]`:

1. `CreatePOCOClassCmd`
2. `CreateINotifyClassCmd`
3. `CreateEntityClassCmd`
4. `CreateDLLFromEntitiesCmd`
5. `CreateDLLFromFilesCmd`
6. `GenerateInterfaceCmd`
7. `GeneratePartialClassCmd`
8. `GenerateClassWithAttributesCmd`
9. `MergePartialClassCmd`
10. `CreateRecordClassCmd`
11. `CreateSealedClassCmd`
12. `CreateAbstractClassCmd`
13. `GenerateWinFormsFormCmd`
14. `GenerateMVCControllerCmd`
15. `GenerateRazorPageCmd`
16. `GenerateBlazorComponentCmd`
17. `GenerateSolutionStructureCmd`
18. `ValidateCodeCmd`
19. `MergePropertiesCmd`
20. `ConvertPOCOToEntityCmd`

## Current Node Surface (`POCOtoEntityCodeConverter`)

`POCOtoEntityCodeConverter` currently exposes:

- Node addin metadata (`AddinAttribute`) for "POCO To Entity".
- One command method with `[CommandAttribute]`: `openPocotoEntity`.
- Stub/non-routed methods:
  - `CreateChildNodes` (returns `ErrorObject`)
  - `ExecuteBranchAction` (returns `ErrorObject`)
  - `MenuItemClicked` (returns `ErrorObject`)
  - `RemoveChildNodes` (returns `ErrorObject`)

## Concrete Mapping Matrix

| Capability | MenuCommands implementation | Node implementation | Current mapping state | Gap |
|---|---|---|---|---|
| Open POCO conversion UI | Indirect (not a dedicated "open page" command) | `openPocotoEntity` -> `Visutil.ShowPage("uc_CodeConverter", ...)` | **Node-only** entry exists | No explicit menu command that opens same UI page |
| Convert POCO file to Entity code | `ConvertPOCOToEntityCmd` uses `_classCreator.ConvertPOCOClassToEntity(...)` | No direct conversion execution in node | **Unlinked duplicates** (UI open vs direct conversion command) | No shared routing contract between node action and menu command |
| Action routing from node context | Not applicable | `ExecuteBranchAction` / `MenuItemClicked` are stubs | **Not implemented** | Node cannot dispatch to `ConvertPOCOToEntityCmd` or other menu commands |
| Child node discovery for related classcreator tasks | Rich menu command set available | `CreateChildNodes` is stub | **Not implemented** | Node tree does not surface menu command breadth |
| Error/result contract consistency | Uses `DMEEditor.ErrorObject` + logs + MessageBox | Returns `ErrorObject`, minimal behavior | **Partially aligned** | No standardized result payload or diagnostics parity |
| Context binding safety | Constructor sets `DMEEditor` from app manager | `SetConfig` sets `DMEEditor`/`TreeEditor` only | **At risk** | `Visutil` is used by `openPocotoEntity` but not assigned in `SetConfig` |

## Implementation Status (Current)

- [x] `POCOtoEntityCodeConverter`: populate `BranchActions` with open + convert actions.
- [x] `POCOtoEntityCodeConverter`: route `MenuItemClicked` -> `ExecuteBranchAction`.
- [x] `POCOtoEntityCodeConverter`: route `ConvertPOCOToEntityCmd` through `DeveloperClassCreatorMenuCommands`.
- [x] `POCOtoEntityCodeConverter`: add null-safe logging for UI open when app manager is unavailable.
- [ ] Add automated test coverage for node routing behavior.
- [ ] Expand node routing beyond POCO conversion for additional classcreator operations.

## Verified Missing Items (Priority)

1. **Missing node-to-command routing (partially resolved)**
   - Basic routing is now implemented for `openPocotoEntity` and `ConvertPOCOToEntityCmd`.
   - Remaining gap: standardized routing model for additional actions and other node classes.

2. **Missing node action coverage**
   - Node exposes only "Open" while menu class exposes 20 concrete operations; no parity bridge exists.

3. **Missing direct mapping metadata**
   - No explicit map artifact from node command names to menu command names/methods.

4. **Potential runtime null risk (partially mitigated)**
   - `openPocotoEntity` now guards against null app manager.
   - Remaining gap: consistent app-manager/context assignment contract across node initialization paths.

## Concrete TODO List (for phases `07a` and `07b`)

1. **Node routing infrastructure**
   - [ ] Add a common routing helper method for node actions (`string action -> Func<IErrorsInfo>` map).
   - [ ] Move hard-coded action names to constants used by both command attributes and `BranchActions`.
   - [ ] Add unknown-action logging with actionable message.

2. **POCO node parity expansion**
   - [ ] Add node action for `CreatePOCOClassCmd`.
   - [ ] Add node action for `CreateEntityClassCmd`.
   - [ ] Add node action for `ValidateCodeCmd`.
   - [ ] Update `BranchActions` to include the new routed actions.

3. **Context initialization hardening**
   - [ ] In `SetConfig`, resolve and assign an app manager reference for node runtime (or define fallback provider).
   - [ ] Standardize null-check + failure message format in all node command methods.

4. **Tests**
   - [ ] Add test: `MenuItemClicked("ConvertPOCOToEntityCmd")` executes without exception and logs expected flow.
   - [ ] Add test: `ExecuteBranchAction("unknown")` returns error object and logs unsupported action.
   - [ ] Add test: `openPocotoEntity` with null app manager does not throw.

## Acceptance Criteria for This Matrix

- Every row above is traceable to current code in the two target files.
- Missing items are specific enough to convert into implementation tasks without re-discovery.
