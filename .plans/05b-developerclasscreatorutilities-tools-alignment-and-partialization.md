# 05b - DeveloperClassCreatorUtilities Alignment with BeepDM Tools + Partialization

## Objective

Refactor `Beep.DeveloperAssistant.Logic/DeveloperClassCreatorUtilities.cs` to use BeepDM `ClassCreator` from `BeepDM/DataManagementEngineStandard/Tools`, split the utility into partial classes, and propagate capability updates to Nodes and Winform surfaces.

## Source Scan Baseline (Completed)

Scanned `BeepDM/DataManagementEngineStandard/Tools` and identified current modular surfaces:

- `ClassCreator.Core.cs`
- `ClassCreator.Advanced.cs`
- `ClassCreator.Database.cs`
- `ClassCreator.WebApi.cs`
- `ClassCreator.Testing.cs`
- `ClassCreator.DllCreation.cs`
- `ClassCreator.EntityExtensions.cs`
- `ClassCreator.PocoToEntity.cs`
- `ClassCreator.cs`
- `WebApiGenerator.cs`
- Helpers under `Tools/Helpers/*` (POCO, WebApi, Database, Validation/Testing, UI, Serverless, Documentation, POCO/EF->Entity)

## Current Problem Statement

- `DeveloperClassCreatorUtilities.cs` is large/bloated and currently mixes:
  - orchestration,
  - generation logic,
  - compilation flows,
  - validation,
  - transformation logic.
- Logic duplicates responsibilities already modularized in BeepDM `ClassCreator` + helpers.
- MenuCommands/Nodes/Winform are partially wired to current utility methods, so migration needs compatibility wrappers.

## Target Architecture

### A) Utility becomes partial and orchestration-only

Create these partial files under `Beep.DeveloperAssistant.Logic`:

- `DeveloperClassCreatorUtilities.Core.cs`
  - constructor, shared dependencies, logging, guard helpers
- `DeveloperClassCreatorUtilities.Dll.cs`
  - DLL compile/create wrappers
- `DeveloperClassCreatorUtilities.BasicGeneration.cs`
  - POCO/INotify/Entity/Interface/Partial/Record/Abstract/Sealed wrappers
- `DeveloperClassCreatorUtilities.WebUi.cs`
  - WinForms/MVC/Razor/Blazor/Web API wrappers
- `DeveloperClassCreatorUtilities.Database.cs`
  - DAL/DbContext/repository/migration wrappers
- `DeveloperClassCreatorUtilities.ValidationTesting.cs`
  - validation and unit test/validator generation wrappers
- `DeveloperClassCreatorUtilities.EntityConversion.cs`
  - Type/DataTable/List/POCO->Entity conversions and runtime type generation
- `DeveloperClassCreatorUtilities.SolutionScaffold.cs`
  - solution/project structure and filesystem scaffolding

### B) Use BeepDM ClassCreator as backend

- Inject/compose `TheTechIdea.Beep.Tools.ClassCreator` instance in core partial.
- Replace custom inline generation where equivalent Tool method exists.
- Keep compatibility signatures in existing public utility methods to avoid breaking menu/node callers.

## Capability Mapping and TODOs

| Capability cluster | Current DA utility | BeepDM Tool surface to use | TODO |
|---|---|---|---|
| DLL build from entities/files | `CreateDLL*` methods | `ClassCreator.DllCreation` | [ ] Replace compile internals with tool calls; keep progress/cancel wrappers |
| Basic class generation | POCO/INotify/Entity/class variants | `ClassCreator.Core` + helpers | [ ] Route all class text generation through tool |
| Advanced docs/schema/serverless | mixed/partial support | `ClassCreator.Advanced` | [ ] Add wrappers for documentation/GraphQL/gRPC/serverless coverage |
| Database artifacts | DbContext/repository/migration | `ClassCreator.Database` | [ ] Add/align wrappers and expose via menu |
| Web API/UI codegen | MVC/WebAPI/Razor/Blazor/WinForms | `ClassCreator.WebApi` + UI helpers | [ ] Unify generation path and output contracts |
| Validation/testing | code validation + test scaffolding | `ClassCreator.Testing` | [ ] Align validation + test generation behavior |
| Entity extensions | type/datatable/list conversions | `ClassCreator.EntityExtensions` | [ ] Replace duplicate conversion logic |
| POCO namespace/runtime conversion | POCO->Entity conversion | `ClassCreator.PocoToEntity` | [ ] Adopt namespace scan/runtime type APIs in utility wrappers |

## Detailed Execution Plan

### Sprint 1 - Foundation and Safe Refactor

- [ ] Add partial file structure listed above.
- [ ] Move existing methods into matching partials without behavior changes.
- [ ] Add central `ClassCreator` backend field in core partial.
- [ ] Add adapter helpers for common parameters (namespace/output/dry-run flags).

### Sprint 2 - Backend Alignment (Method by Method)

- [ ] Migrate DLL flows to `ClassCreator.DllCreation` methods.
- [ ] Migrate class generation flows to `ClassCreator.Core`.
- [ ] Migrate POCO conversion and runtime generation to `ClassCreator.PocoToEntity`.
- [ ] Migrate conversion helpers to `ClassCreator.EntityExtensions`.
- [ ] Add TODO deferments for any utility method with no direct Tool equivalent.

### Sprint 3 - MenuCommands Enhancement

Target file: `Beep.DeveloperAssistant.Logic.MenuCommands/DeveloperClassCreatorMenuCommands.cs`

- [ ] Keep existing command names, but route internals through updated utility wrappers.
- [ ] Add missing commands for newly exposed Tool capabilities:
  - namespace POCO scan
  - runtime entity type generation
  - graph/schema and docs generation (if approved in UX)
- [ ] Add dry-run parameter support for mutating commands.

### Sprint 4 - Nodes Enhancement

Target folder: `Beep.DeveloperAssistant.Nodes/*`

- [ ] Extend `POCOtoEntityCodeConverter` routing beyond `ConvertPOCOToEntityCmd`:
  - `CreatePOCOClassCmd`
  - `CreateEntityClassCmd`
  - `ValidateCodeCmd`
- [ ] Apply same routing pattern to `CodeGeneratorBranch`.
- [ ] Add/maintain `BranchActions` lists as authoritative node action definitions.

### Sprint 5 - WinformCore Enhancement

Targets:
- `Beep.DeveloperAssistant.WinformCore/uc_CodeConverter*`
- `Beep.DeveloperAssistant.WinformCore/uc_DeveloperAssistantTemplateDesigner*`

- [ ] Replace any direct generation logic with updated utility methods.
- [ ] Add UI action parity for top menu/node flows.
- [ ] Add diagnostics/progress binding for long-running create/compile operations.
- [ ] Add pre-run summary and post-run result display using unified result model.

### Sprint 6 - Verification and Compatibility

- [ ] Build solution and run smoke tests for:
  - POCO generation
  - POCO->Entity conversion
  - DLL from files
  - code validation
- [ ] Verify node action routing and Winform button flows call same utility methods.
- [ ] Update `07c` matrix with new routing coverage status.
- [ ] Document deferred items with rationale and target phase.

## Implementation Rules

- Keep public method signatures in `DeveloperClassCreatorUtilities` backward compatible where possible.
- New logic should call BeepDM `ClassCreator` methods instead of duplicating code.
- Use partial classes to keep each file focused and under manageable size.
- All new menu/node/winform wiring should delegate to logic utility methods (thin handlers).

## Definition of Done

- [ ] `DeveloperClassCreatorUtilities` is split into partials by concern.
- [ ] Core generation/compilation/conversion paths use BeepDM `ClassCreator`.
- [ ] MenuCommands, Nodes, and Winform actions are aligned with updated utility backend.
- [ ] Coverage matrix and plan docs reflect implemented/deferred items with status.
