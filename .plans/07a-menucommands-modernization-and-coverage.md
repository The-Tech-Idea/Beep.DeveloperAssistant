# 07a - MenuCommands Modernization and Coverage

## Objective

Define explicit enhancement work for `Beep.DeveloperAssistant.Logic.MenuCommands` so menu entry points are fully aligned with `ClassCreator` and utility-class capabilities.

## Scope

- `Beep.DeveloperAssistant.Logic.MenuCommands/*`
- Primary high-volume class: `DeveloperClassCreatorMenuCommands.cs`
- Cross-link with utility services in `Beep.DeveloperAssistant.Logic/*`

## Enhancement Workstreams

1. **Capability Coverage Matrix**
   - Build command-to-service matrix mapping each menu item to:
     - logic utility method,
     - ClassCreator flow,
     - expected input model,
     - output/result model.
   - Mark missing menu entries for existing utility and classcreator operations.

2. **Command Contract Standardization**
   - Normalize command metadata (name, category, description, tags, help text).
   - Standardize command parameter handling and defaulting rules.
   - Ensure every command follows a consistent return/result contract.

3. **Thin Handler Pattern**
   - Keep menu handlers as orchestration only.
   - Move heavy logic to dedicated utility/service methods where needed.
   - Remove duplicated validation/business logic embedded in menu handlers.

4. **Utility + ClassCreator Parity**
   - Add missing menu commands for utility capabilities introduced in phase `05a`.
   - Add missing command entries for advanced ClassCreator operations from phase `02`.
   - Ensure naming and grouping are consistent with Node labels and Winform UI labels.

5. **Diagnostics and Dry-Run Integration**
   - Add dry-run support for destructive/generative commands.
   - Standardize diagnostics payloads for menu command execution history and error output.
   - Provide "what will run" summaries before mutation operations.

6. **Discoverability and UX**
   - Improve command grouping and search keywords for large command sets.
   - Add command alias support where high-frequency commands are verbose.
   - Ensure menu command descriptions are actionable and user-focused.

## Deliverables

- `MenuCommands` capability matrix (doc or generated artifact).
- Initial concrete matrix: `07c-coverage-matrix-developerclasscreatormenucommands-pocotoentity.md`.
- Refactored menu command handlers following thin-handler pattern.
- Missing utility/classcreator menu items implemented or explicitly deferred.
- Shared command metadata and result model conventions adopted.

## Acceptance Criteria

- Every major utility/classcreator capability has a menu command path (or documented deferment).
- Menu command handlers delegate to logic/services without duplicated core logic.
- Dry-run/diagnostics behavior is consistent across command categories.
- Nodes and MenuCommands expose matching labels/intents for shared capabilities.

## Dependencies

- Phase `02` (ClassCreator integration surface).
- Phase `05a` (utility modernization and result contracts).
- Phase `05b` (ClassCreator tools alignment and partialization plan).
- Phase `07` (menu/node alignment baseline).
- Phase `09` (testing diagnostics and dry-run validation).

## Execution Plan (Concrete TODOs)

### Sprint A - Command Inventory and Contracts

- [ ] Create `MenuCommands-Inventory.md` listing every `[CommandAttribute]` method in:
  - `DeveloperClassCreatorMenuCommands.cs`
  - `DeveloperEncryptionMenuCommands.cs`
  - `DeveloperWebMenuCommands.cs`
  - `DeveloperTextFileMenuCommands.cs`
- [ ] Add columns: command name, caption, sync/async, underlying utility call, mutating/non-mutating, dry-run support.
- [ ] Define one result contract for all command handlers (`Success`, `Message`, `Error`, `Payload`).

### Sprint B - Refactor High-Volume Menu Class

- [ ] Split `DeveloperClassCreatorMenuCommands.cs` into partial classes by concern:
  - `...Core.cs` (constructor/helpers)
  - `...Generation.cs` (POCO/Entity/INotify/interface/partial)
  - `...Compilation.cs` (DLL/validate/merge)
  - `...UIWeb.cs` (WinForms/MVC/Razor/Blazor)
- [ ] Centralize repetitive prompt + validation code into private helper methods.
- [ ] Replace direct `MessageBox` duplication with a single notifier helper method.

### Sprint C - Coverage Completion

- [ ] Compare menu inventory with utility public methods in:
  - `DeveloperClassCreatorUtilities.cs`
  - `DeveloperConversionUtilities.cs`
  - `RoslynCompiler.cs`
- [ ] For each uncovered high-value utility method, decide one of:
  - Add menu command
  - Explicit deferment entry in plan
- [ ] Update `07c` matrix after each added command mapping.

### Sprint D - Diagnostics and Dry-Run

- [ ] Add optional dry-run mode parameter for mutating menu commands.
- [ ] Add structured diagnostic log line before and after command execution.
- [ ] Ensure all command exceptions include command name and target file/path context.

## Definition of Done

- [ ] Inventory complete and committed to `.plans`.
- [ ] `DeveloperClassCreatorMenuCommands` reduced to manageable partial files.
- [ ] Coverage gaps either implemented or marked with deferment rationale.
- [ ] At least 5 critical commands validated manually end-to-end (POCO, Entity, DLL, Validate, Convert POCO).
