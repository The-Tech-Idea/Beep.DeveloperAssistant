# 05a - Logic Utilities Modernization and Coverage

## Objective

Define explicit enhancement work for utility classes under `Beep.DeveloperAssistant.Logic` so they move from "helper collection" to a consistent, testable capability layer.

## Target Files

- `DeveloperClassCreatorUtilities.cs`
- `DeveloperConversionUtilities.cs`
- `DeveloperReflectionUtilities.cs`
- `DeveloperWebUtilities.cs`
- `DeveloperTextFileUtilities.cs`
- `DeveloperCompressionUtilities.cs`
- `DeveloperEncryptionUtilities.cs`
- `DeveloperLocalizationUtilities.cs`
- `DeveloperSchedulingUtilities.cs`
- `DeveloperNetworkUtilities.cs`
- `RoslynCompiler.cs`

## Enhancement Workstreams

1. **Contract and Naming Consistency**
   - Define standard result model for all utility operations (`Success`, `Message`, `Details`, `Exception`, `ElapsedMs`).
   - Normalize method naming to action-first conventions and predictable async suffix usage.
   - Align parameter order patterns (`input`, `options`, `cancellationToken`) across utilities.

2. **Safety and Validation Layer**
   - Add centralized guards for file paths, URI validation, null/empty checks, and secure defaults.
   - Standardize timeout and cancellation behavior for network/web/compiler operations.
   - Add clear redaction rules for logs and error outputs (especially encryption/text/file paths).

3. **Shared Options and Profiles**
   - Introduce option objects per utility domain (conversion, web, compression, compiler).
   - Support profile-based defaults consumed by MenuCommands and Nodes.
   - Ensure option objects are serializable where command replay/dry-run needs it.

4. **Roslyn and Code-Gen Reliability**
   - Split `RoslynCompiler.cs` by concern (parse, compile, diagnostics, output handling) using partial classes if needed.
   - Add deterministic compile pipeline configuration (references, language version, nullable mode, emit options).
   - Normalize diagnostic mapping so UI/CLI can show actionable compiler feedback.

5. **Observability and Diagnostics**
   - Add operation correlation id and timing to long-running utility calls.
   - Expose dry-run diagnostics for mutating tasks (file changes, generated artifacts, web actions).
   - Add lightweight health/introspection endpoints consumed by diagnostics commands.

6. **Test Coverage Expansion**
   - Unit tests per utility class for success/failure paths and edge cases.
   - Golden-file tests for text/code transformation outputs.
   - Compiler tests for representative snippets and failure diagnostics.

## Deliverables

- A shared utility base pattern (or helper module) used by all utility classes.
- Refactored utility methods with unified result and error contracts.
- Utility-specific option objects and profile defaults.
- Test suite additions with baseline coverage thresholds for `Beep.DeveloperAssistant.Logic`.

## Acceptance Criteria

- Every utility class exposes a predictable API shape and result contract.
- Critical utility methods are covered by automated tests for both happy-path and failure-path.
- Nodes/MenuCommands integration can invoke utility methods without special-case adapters.
- `RoslynCompiler` diagnostics are consistently consumable by UI and CLI surfaces.

## Dependencies

- Phase `05` (logic capability expansion baseline).
- Phase `08` (partial-class and template quality standards).
- Phase `09` (diagnostics and dry-run execution model).
- Detailed class-creator specific execution plan: `05b-developerclasscreatorutilities-tools-alignment-and-partialization.md`.

## Execution Plan (Concrete TODOs)

### Sprint A - Utility Inventory and Baseline

- [ ] Create `Logic-Utilities-Inventory.md` with columns:
  - class name
  - public methods count
  - async method count
  - current return type(s)
  - mutating vs non-mutating
  - test coverage status
- [ ] Baseline first 5 classes:
  - `DeveloperClassCreatorUtilities.cs`
  - `DeveloperTextFileUtilities.cs`
  - `DeveloperWebUtilities.cs`
  - `DeveloperConversionUtilities.cs`
  - `RoslynCompiler.cs`

### Sprint B - Unified Result Contract

- [ ] Add a shared operation result model under `Beep.DeveloperAssistant.Logic.Models`.
- [ ] Implement adapter wrappers in:
  - `DeveloperClassCreatorUtilities`
  - `DeveloperTextFileUtilities`
  - `DeveloperWebUtilities`
- [ ] Keep backward-compatible overloads where current callers expect legacy return types.

### Sprint C - Validation and Safety Hardening

- [ ] Add shared validation helper(s) for path/URI/input checks.
- [ ] Replace duplicated guard code in utilities with helper usage.
- [ ] Add timeout + cancellation options for long-running utility methods.
- [ ] Add logging redaction helper for sensitive values.

### Sprint D - RoslynCompiler Refactor

- [ ] Split `RoslynCompiler.cs` into partials:
  - `RoslynCompiler.Parsing.cs`
  - `RoslynCompiler.Compilation.cs`
  - `RoslynCompiler.Diagnostics.cs`
  - `RoslynCompiler.Output.cs`
- [ ] Centralize reference resolution and diagnostic formatting.
- [ ] Add stable compile options object and defaults.

### Sprint E - Test Implementation

- [ ] Add tests for high-risk methods:
  - file writes/merges
  - compiler validate/compile
  - web/network calls (mocked)
- [ ] Add golden-file tests for class generation outputs.
- [ ] Add negative tests (invalid input, missing file/path, bad C# code).

## Definition of Done

- [ ] Inventory complete and linked from `.plans`.
- [ ] Shared result model used by at least 3 utility classes.
- [ ] RoslynCompiler partial split completed without behavior regressions.
- [ ] Test coverage exists for critical utility flows and failure paths.
