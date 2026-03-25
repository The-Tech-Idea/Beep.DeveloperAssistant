# 02 - ClassCreator Integration Surface

## Objective

Integrate BeepDM `ClassCreator` capabilities into `Beep.DeveloperAssistant.Logic` with consistent API contracts.

## Scope

- `ClassCreator.Core.cs`
- `ClassCreator.Advanced.cs`
- `ClassCreator.Database.cs`
- `ClassCreator.DllCreation.cs`
- `ClassCreator.EntityExtensions.cs`
- `ClassCreator.PocoToEntity.cs`
- `ClassCreator.Testing.cs`
- `ClassCreator.WebApi.cs`

## Planned Enhancements

- Introduce a unified result contract for generation operations (`success`, outputs, diagnostics, warnings).
- Provide capability wrappers grouped by concern:
  - entity/poco conversion
  - database and repository scaffolding
  - webapi generation
  - testing/validators
  - documentation and advanced outputs
- Add dry-run preview support before file emission.

## File-by-File Mapping (Utility -> ClassCreator Surface)

| Beep.DeveloperAssistant.Logic Utility | Primary ClassCreator Method Groups | ClassCreator Partial/File Source |
|---|---|---|
| `DeveloperClassCreatorUtilities` | Core class generation: POCO, entity, record, nullable, DDD aggregate, runtime type compile | `ClassCreator.Core.cs` |
| `DeveloperClassCreatorUtilities` | Entity conversion bridge: `Type`/`DataTable`/list -> `EntityStructure`, entity class generation | `ClassCreator.EntityExtensions.cs` |
| `DeveloperClassCreatorUtilities` | POCO/EF conversion and scanning, namespace/file/source conversion to entities | `ClassCreator.PocoToEntity.cs` |
| `DeveloperClassCreatorUtilities` | Database scaffolding: DAL, DbContext, EF config, repository, migration templates | `ClassCreator.Database.cs` |
| `DeveloperClassCreatorUtilities` | Web API scaffolding: controllers + minimal API | `ClassCreator.WebApi.cs` |
| `DeveloperClassCreatorUtilities` | Test and validator generation | `ClassCreator.Testing.cs` |
| `DeveloperClassCreatorUtilities` | Advanced outputs: docs, diff reports, GraphQL schema, Blazor, gRPC proto/service | `ClassCreator.Advanced.cs` |
| `DeveloperClassCreatorUtilities` | DLL/assembly flows: generate + compile to DLL, folder-to-DLL compile | `ClassCreator.DllCreation.cs` |
| `DeveloperConversionUtilities` | Type/schema conversion workflows and mapping wrappers around entity conversion methods | `ClassCreator.EntityExtensions.cs`, `ClassCreator.PocoToEntity.cs` |
| `DeveloperReflectionUtilities` | Runtime compile/load/inspect class outputs and generated assemblies | `ClassCreator.Core.cs`, `ClassCreator.DllCreation.cs` |
| `DeveloperWebUtilities` | API and transport-oriented code generation wrappers (controllers/minimal APIs) | `ClassCreator.WebApi.cs`, `ClassCreator.Advanced.cs` |
| `DeveloperTextFileUtilities` | Source template input/output, preview/export, generated artifact persistence wrappers | `ClassCreator.Core.cs`, `ClassCreator.DllCreation.cs` |
| `RoslynCompiler` | Low-level compile hooks used by runtime generation and DLL creation orchestration | `ClassCreator.Core.cs`, `ClassCreator.DllCreation.cs`, `ClassCreator.PocoToEntity.cs` |

## Integration Contract Notes

- `DeveloperClassCreatorUtilities` is the primary façade and should expose concern-based methods that internally route to the correct `ClassCreator` partial.
- Conversion utilities should not call database/webapi/testing generators directly; keep conversion and generation concerns separated.
- UI/menu/nodes must call utility façade methods, not `ClassCreator` partials directly.
