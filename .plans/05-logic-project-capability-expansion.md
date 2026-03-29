# 05 - Logic Project Capability Expansion

## Objective

Enhance `Beep.DeveloperAssistant.Logic` to expose coherent high-level developer workflows.

## Scope

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

## Planned Deliverables

- Unified workflow services for generation/conversion tasks.
- Standardized diagnostics and operation status models.
- Reusable template/profile model integration.
- Detailed utility-class enhancement execution is tracked in `05a-logic-utilities-modernization-and-coverage.md`.

## Execution TODOs

- [ ] Create `Logic-Capability-Matrix.md` (class -> capability -> caller surfaces).
- [ ] Group utility classes into domains:
  - generation/compile
  - conversion/reflection
  - web/network
  - text/file
  - security/compression/localization/scheduling
- [ ] Define one public façade entry per domain used by MenuCommands/Nodes/Winform.
- [ ] Normalize error and status reporting across all domain façades.
- [ ] Link each domain to its detailed phase doc (`05a`, `05b`, `06a`, `07a`, `07b`).

## Verification Criteria

- [ ] Every domain has explicit owner file(s) and façade methods.
- [ ] No domain relies on hidden side effects from UI layer.
- [ ] Capability matrix is current and referenced by higher phases.
