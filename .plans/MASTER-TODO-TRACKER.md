# MASTER TODO TRACKER

Consolidated execution board for all checkbox tasks across `Beep.DeveloperAssistant/.plans`.

## Status Legend

- [ ] Not done
- [x] Done

---

## Phase 00 - Overview
Source: `00-overview-capability-gap-matrix.md`

- [ ] Build `Phase-Backlog-Index.md` listing all numbered phases and current completion percentage.
- [ ] Add owner + target branch + target milestone fields for each phase doc.
- [ ] Add dependency graph section (blocking phase vs blocked phases).
- [ ] Add status legend used by all plan docs.
- [ ] Link this overview to `05b`, `06a`, `07a`, `07b`, `07c` as implementation-critical plans.
- [ ] Every phase document includes TODO checklist + target files + verification criteria.
- [ ] Every cross-phase dependency is explicit (no implied dependencies).
- [ ] Overview matches current execution order in `README.md`.

## Phase 01 - Architecture and Boundaries
Source: `01-solution-architecture-and-boundaries.md`

- [ ] Create `Architecture-Boundary-Checklist.md` with allowed dependency directions.
- [ ] Validate project references do not introduce Logic -> Winform dependency.
- [ ] Add "thin adaptor" rule checks for MenuCommands and Nodes (manual checklist first).
- [ ] Identify and list methods in MenuCommands/Nodes that contain business logic and move candidates.
- [ ] Add namespace conventions section for new partial classes and helpers.
- [ ] No direct references from `Logic` project to Winform assemblies/namespaces.
- [ ] At least one reviewed MenuCommands class and one Node class follow thin-adaptor pattern.
- [ ] Boundary checklist reviewed and linked by later phases (`06a`, `07a`, `07b`).

## Phase 02 - ClassCreator Integration
Source: `02-classcreator-integration-surface.md`

- [ ] Build `ClassCreator-Method-Mapping.md` (utility method, Tool method, migration status, compatibility risk).
- [ ] Implement adapter wrappers in `DeveloperClassCreatorUtilities` for missing Tool surfaces.
- [ ] Mark each utility method as direct/wrapper/deferred.
- [ ] Add compatibility wrappers for existing MenuCommands call sites.
- [ ] Update matrix when each cluster is migrated (Core/Database/WebApi/Testing/Advanced/Dll/EntityExtensions/PocoToEntity).
- [ ] Every high-traffic utility method has a mapped Tool backend or explicit deferment.
- [ ] No direct Tool partial calls from Winform/MenuCommands/Nodes.
- [ ] Mapping doc is complete and referenced in `05b`.

## Phase 03 - Universal Helper Factory / RDBMS
Source: `03-universal-helper-factory-and-rdbms-flows.md`

- [ ] Inventory all direct SQL strings in `Beep.DeveloperAssistant.Logic`.
- [ ] Replace direct SQL generation with helper-factory calls where helper coverage exists.
- [ ] Add capability check helper (datasource type, helper availability, fallback reason logging).
- [ ] Add explicit defer list for unsupported helper scenarios.
- [ ] Add one diagnostic report output for provider/helper decision trace.
- [ ] No new direct SQL composition in enhanced paths.
- [ ] Helper selection path is logged for at least one flow per datasource type tested.
- [ ] Fallback paths are deterministic and user-visible.

## Phase 04 - EnvironmentService Bootstrap
Source: `04-environmentservice-bootstrap-and-safety.md`

- [ ] Add bootstrap orchestrator method with explicit step order (folders, mappings, connections, queries).
- [ ] Add idempotency checks for each step.
- [ ] Add explicit rollback/compensation note for partial bootstrap failures.
- [ ] Add bootstrap diagnostics object with step-by-step status.
- [ ] Expose dry-run bootstrap validation mode.
- [ ] Running bootstrap twice does not duplicate registrations.
- [ ] Bootstrap failure logs include failing step and actionable message.
- [ ] Dry-run mode reports all planned actions without mutating files/config.

## Phase 05 - Logic Capability Expansion
Source: `05-logic-project-capability-expansion.md`

- [ ] Create `Logic-Capability-Matrix.md` (class -> capability -> caller surfaces).
- [ ] Group utility classes into domains (generation/compile, conversion/reflection, web/network, text/file, security/compression/localization/scheduling).
- [ ] Define one public facade entry per domain used by MenuCommands/Nodes/Winform.
- [ ] Normalize error and status reporting across all domain facades.
- [ ] Link each domain to detailed docs (`05a`, `05b`, `06a`, `07a`, `07b`).
- [ ] Every domain has explicit owner file(s) and facade methods.
- [ ] No domain relies on hidden side effects from UI layer.
- [ ] Capability matrix is current and referenced by higher phases.

## Phase 05a - Logic Utilities Modernization
Source: `05a-logic-utilities-modernization-and-coverage.md`

- [ ] Create `Logic-Utilities-Inventory.md` with class/method/coverage columns.
- [ ] Baseline first 5 classes (`DeveloperClassCreatorUtilities`, `DeveloperTextFileUtilities`, `DeveloperWebUtilities`, `DeveloperConversionUtilities`, `RoslynCompiler`).
- [ ] Add shared operation result model under `Beep.DeveloperAssistant.Logic.Models`.
- [ ] Implement adapter wrappers in `DeveloperClassCreatorUtilities`, `DeveloperTextFileUtilities`, `DeveloperWebUtilities`.
- [ ] Keep backward-compatible overloads where legacy callers require them.
- [ ] Add shared validation helpers for path/URI/input.
- [ ] Replace duplicated guard code in utilities.
- [ ] Add timeout + cancellation options for long-running methods.
- [ ] Add logging redaction helper for sensitive values.
- [ ] Split `RoslynCompiler.cs` into `Parsing`, `Compilation`, `Diagnostics`, `Output` partials.
- [ ] Centralize reference resolution and diagnostic formatting.
- [ ] Add stable compile options object and defaults.
- [ ] Add tests for high-risk methods (file writes/merges, compiler validate/compile, mocked web/network).
- [ ] Add golden-file tests for class generation outputs.
- [ ] Add negative tests for invalid input/path/code.
- [ ] Inventory complete and linked from `.plans`.
- [ ] Shared result model used by at least 3 utility classes.
- [ ] RoslynCompiler partial split completed without regressions.
- [ ] Test coverage exists for critical utility flows and failure paths.

## Phase 05b - ClassCreatorUtilities + Tools Alignment
Source: `05b-developerclasscreatorutilities-tools-alignment-and-partialization.md`

- [ ] Replace DLL compile internals with Tool calls; keep progress/cancel wrappers.
- [ ] Route all class text generation through Tool.
- [ ] Add wrappers for docs/GraphQL/gRPC/serverless coverage.
- [ ] Add/align database wrappers and expose via menu.
- [ ] Unify Web/API/UI generation path and output contracts.
- [ ] Align validation + test generation behavior.
- [ ] Replace duplicate conversion logic with EntityExtensions calls.
- [ ] Adopt POCO namespace/runtime type APIs in utility wrappers.
- [ ] Add partial file structure for `DeveloperClassCreatorUtilities`.
- [ ] Move existing methods into matching partials without behavior changes.
- [ ] Add central `ClassCreator` backend field in core partial.
- [ ] Add adapter helpers for common parameters (namespace/output/dry-run).
- [ ] Migrate DLL flows to `ClassCreator.DllCreation`.
- [ ] Migrate class generation flows to `ClassCreator.Core`.
- [ ] Migrate POCO conversion/runtime generation to `ClassCreator.PocoToEntity`.
- [ ] Migrate conversion helpers to `ClassCreator.EntityExtensions`.
- [ ] Add deferments where no direct Tool equivalent exists.
- [ ] Keep existing menu command names while routing through updated utility wrappers.
- [ ] Add missing commands for namespace POCO scan/runtime entity type generation/graph+docs (if approved).
- [ ] Add dry-run parameter support for mutating menu commands.
- [ ] Extend `POCOtoEntityCodeConverter` routing for `CreatePOCOClassCmd`, `CreateEntityClassCmd`, `ValidateCodeCmd`.
- [ ] Apply same routing pattern to `CodeGeneratorBranch`.
- [ ] Keep `BranchActions` lists authoritative for node actions.
- [ ] Replace direct generation logic in Winform with updated utility methods.
- [ ] Add UI action parity for top menu/node flows.
- [ ] Add diagnostics/progress binding for long-running create/compile operations.
- [ ] Add pre-run summary and post-run result display.
- [ ] Build solution and run smoke tests (POCO, POCO->Entity, DLL from files, validate).
- [ ] Verify node action routing and Winform flows call same utility methods.
- [ ] Update `07c` matrix with new routing coverage.
- [ ] Document deferred items with rationale and target phase.
- [ ] `DeveloperClassCreatorUtilities` split into partials by concern.
- [ ] Core generation/compilation/conversion paths use BeepDM `ClassCreator`.
- [ ] MenuCommands, Nodes, and Winform actions aligned with updated utility backend.
- [ ] Coverage matrix and plan docs reflect implemented/deferred status.

## Phase 06 - WinformCore Workflows
Source: `06-winformcore-experience-and-workflows.md`

- [ ] Create `Winform-Workflow-Spec.md` for each major page/control.
- [ ] Implement workflow stages: Input, Validate, Preview, Execute, Result.
- [ ] Add progress + cancel wiring to long-running operations.
- [ ] Add unified diagnostics panel component.
- [ ] Validate UI action parity with command/node mappings.
- [ ] At least 2 primary Winform controls implement full stage flow.
- [ ] UI preview appears before mutation for generation actions.
- [ ] Progress/cancel works on at least one compile and one conversion flow.

## Phase 06a - Winform Sync with Utilities/MenuCommands
Source: `06a-winformcore-sync-with-utilities-and-menucommands.md`

- [ ] Create `WinformCore-Action-Matrix.md`.
- [ ] Baseline `uc_CodeConverter` and `uc_DeveloperAssistantTemplateDesigner`.
- [ ] Add missing UI actions for Create POCO, Create Entity, Convert POCO->Entity, Validate Code, Create DLL.
- [ ] Ensure each UI action invokes the same path as MenuCommands.
- [ ] Add standardized operation status panel.
- [ ] Add pre-run summary and confirmation for mutating operations.
- [ ] Add cancel support where async methods exist.
- [ ] Prevent duplicate clicks during in-flight operations.
- [ ] Align UI labels with `DeveloperClassCreatorMenuCommands` captions.
- [ ] Align icon usage with node/menu icon names.
- [ ] Document intentional label differences.
- [ ] Manual verification: node->UI conversion flow, equivalent menu flow parity, invalid input rendering.
- [ ] Capture screenshots/notes for verification scenarios.
- [ ] Winform matrix completed with implemented/missing status.
- [ ] Top 5 classcreator actions available from Winform and mapped to existing paths.
- [ ] Diagnostics UX consistent across at least two major controls/pages.

## Phase 07 - MenuCommands and Nodes Alignment
Source: `07-menucommands-and-nodes-alignment.md`

- [ ] Create `Menu-Node-Coverage-Master-Matrix.md`.
- [ ] Resolve mismatched naming between menu captions and node labels.
- [ ] Replace stubbed node action handlers for top-priority nodes.
- [ ] Ensure menu/node invoke same logic utility methods.
- [ ] Add defer list for intentionally hidden/low-priority capabilities.
- [ ] Every top-priority capability has both menu and node entry (or explicit deferment).
- [ ] Node/menu execution paths converge on shared utility methods.
- [ ] Coverage matrix is linked from `07a`, `07b`, and `07c`.

## Phase 07a - MenuCommands Modernization
Source: `07a-menucommands-modernization-and-coverage.md`

- [ ] Create `MenuCommands-Inventory.md` for classcreator/encryption/web/text menu command files.
- [ ] Add inventory columns: name/caption/sync-async/underlying utility/mutating/dry-run.
- [ ] Define one result contract for all command handlers.
- [ ] Split `DeveloperClassCreatorMenuCommands.cs` into concern-based partial classes.
- [ ] Centralize prompt + validation helpers.
- [ ] Replace duplicated `MessageBox` usage with notifier helper.
- [ ] Compare menu inventory against utility public methods (`DeveloperClassCreatorUtilities`, `DeveloperConversionUtilities`, `RoslynCompiler`).
- [ ] For uncovered high-value utilities: add command or explicit deferment.
- [ ] Update `07c` matrix per mapping added.
- [ ] Add optional dry-run mode parameter for mutating commands.
- [ ] Add structured diagnostic log before/after command execution.
- [ ] Ensure command exceptions include command name and file/path context.
- [ ] Inventory committed to `.plans`.
- [ ] `DeveloperClassCreatorMenuCommands` reduced via partialization.
- [ ] Coverage gaps implemented or explicitly deferred.
- [ ] Validate 5 critical commands end-to-end (POCO, Entity, DLL, Validate, Convert POCO).

## Phase 07b - Nodes Sync
Source: `07b-nodes-sync-with-utilities-and-winformcore.md`

- [ ] Create `Nodes-Inventory.md`.
- [ ] Identify node classes with stubbed `ExecuteBranchAction` / `MenuItemClicked`.
- [ ] Prioritize top 5 node classes by visibility + command density.
- [ ] Apply consistent routing pattern (BranchActions + ExecuteBranchAction + delegate to logic/menu).
- [ ] Apply POCO pattern to `CodeGeneratorBranch.cs`, then next two priority nodes.
- [ ] Add null-safe context checks for `DMEEditor`, app manager, tree refs.
- [ ] Normalize node naming/icon/object-type metadata conventions.
- [ ] Align `BranchText` and command captions to menu labels.
- [ ] Add/clean node search keywords.
- [ ] Manual verification matrix (open action, generation action, validation action, error-path action).
- [ ] Update `07c` (or new `07c-*`) matrices for implemented node clusters.
- [ ] At least 3 node classes have working routing (not stubs).
- [ ] Node labels and menu labels match for routed capabilities.
- [ ] Inventory reflects implemented vs pending coverage.

## Phase 07c - Concrete Coverage Matrix (POCO Node)
Source: `07c-coverage-matrix-developerclasscreatormenucommands-pocotoentity.md`

- [x] `POCOtoEntityCodeConverter`: populate `BranchActions` with open + convert actions.
- [x] `POCOtoEntityCodeConverter`: route `MenuItemClicked` -> `ExecuteBranchAction`.
- [x] `POCOtoEntityCodeConverter`: route `ConvertPOCOToEntityCmd` through `DeveloperClassCreatorMenuCommands`.
- [x] `POCOtoEntityCodeConverter`: add null-safe logging for UI open when app manager is unavailable.
- [ ] Add automated test coverage for node routing behavior.
- [ ] Expand node routing beyond POCO conversion for additional classcreator operations.
- [ ] Add common routing helper (`string -> Func<IErrorsInfo>` map).
- [ ] Move hard-coded action names to shared constants.
- [ ] Add unknown-action logging with actionable message.
- [ ] Add node action for `CreatePOCOClassCmd`.
- [ ] Add node action for `CreateEntityClassCmd`.
- [ ] Add node action for `ValidateCodeCmd`.
- [ ] Update `BranchActions` with new routed actions.
- [ ] In `SetConfig`, resolve/assign app manager reference (or fallback provider).
- [ ] Standardize null-check + failure message format in node command methods.
- [ ] Add test: `MenuItemClicked("ConvertPOCOToEntityCmd")` runs without exception and logs expected flow.
- [ ] Add test: unknown action returns error object and logs unsupported action.
- [ ] Add test: `openPocotoEntity` with null app manager does not throw.

## Phase 08 - Quality / Partial-Class / Template Standards
Source: `08-code-quality-partial-class-and-template-standards.md`

- [ ] Define partial-class threshold rule (for example, line/method count limits).
- [ ] Create `Partialization-Backlog.md` (top 10 classes).
- [ ] Apply partial split to `DeveloperClassCreatorUtilities` (tracked in `05b`).
- [ ] Standardize template placeholders and naming.
- [ ] Add style checklist for comments/docs/error messages.
- [ ] Target bloated classes split with no behavior regressions.
- [ ] Template naming/placeholders consistent across generation features.
- [ ] New enhancements follow partial-class pattern by default.

## Phase 09 - Testing / Diagnostics / Dry-Run
Source: `09-testing-diagnostics-and-dry-run-modes.md`

- [ ] Create `Testing-Matrix.md`.
- [ ] Add dry-run flags/options for mutating utility methods and surface via menu/node/ui.
- [ ] Add shared diagnostic payload model.
- [ ] Implement baseline automated tests (POCO generation, POCO->Entity, DLL from files, node routing).
- [ ] Add golden-output snapshots.
- [ ] Dry-run available for top mutating operations with intended output reporting.
- [ ] Diagnostics include warnings/errors + decision trace where relevant.
- [ ] Test matrix passes for critical flows before rollout.

## Phase 10 - Rollout / Compatibility / Adoption
Source: `10-rollout-compatibility-and-adoption.md`

- [ ] Create `Rollout-Checklist.md` with stage gates.
- [ ] Define feature-flag keys and default values.
- [ ] Add compatibility validation list (menu command names, node action names, winform page IDs).
- [ ] Publish deprecation mapping with timeline.
- [ ] Add rollback steps per stage.
- [ ] Additive stage does not break existing command/node entry points.
- [ ] Feature flags can enable/disable enhancements without code changes.
- [ ] Deprecation notices documented before cleanup stage.

## Standards Traceability
Source: `standards-traceability-matrix.md`

- [ ] Add evidence column per standard.
- [ ] Add status column.
- [ ] Add owner and target milestone column.
- [ ] Update matrix after each phase completion.
- [ ] Every standard has at least one linked implementation artifact.
- [ ] No standard remains without owner or status.

## Risk Register and Cutover
Source: `risk-register-and-cutover-checklists.md`

- [ ] Add risk owner and review cadence for each risk.
- [ ] Add probability and detection method columns.
- [ ] Add trigger signals for each risk.
- [ ] Add contingency steps with rollback actions.
- [ ] Review/update risk register at each rollout stage gate.
- [ ] Pre-cutover.
  - [ ] test matrix green
  - [ ] dry-run validation complete
  - [ ] feature flags prepared
- [ ] Cutover.
  - [ ] enable target feature flags
  - [ ] validate menu/node/winform key paths
  - [ ] monitor diagnostics for regression signals
- [ ] Post-cutover.
  - [ ] collect issues and classify severity
  - [ ] execute rollback if critical failure threshold exceeded
  - [ ] finalize adoption note and update standards matrix
