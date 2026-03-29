# 03 - Universal Helper Factory and RDBMS Flows

## Objective

Adopt universal helper strategy for datasource-aware SQL/schema behavior.

## Planned Integration Rules

- Resolve helpers at runtime using helper factory and datasource type metadata.
- Prefer universal/general helper APIs for schema/DDL/DML operations.
- Avoid inline SQL generation where helper coverage exists.
- Add capability checks and fallback flow:
  - helper availability
  - supported datasource list
  - guarded fallback for unknown provider types

## Safety Requirements

- Validate entities before SQL generation.
- Quote identifiers through helper APIs.
- Check feature support/capabilities before optional operations.

## Target Files

- `Beep.DeveloperAssistant.Logic/*Rdbms*`
- `Beep.DeveloperAssistant.Logic/*Conversion*`
- Call sites in MenuCommands and Nodes that perform datasource operations

## Execution TODOs

- [ ] Inventory all direct SQL strings in `Beep.DeveloperAssistant.Logic`.
- [ ] Replace direct SQL generation with helper-factory calls where helper coverage exists.
- [ ] Add capability check helper:
  - datasource type detection
  - helper availability
  - fallback reason logging
- [ ] Add explicit defer list for unsupported helper scenarios.
- [ ] Add one diagnostic report output for provider/helper decision trace.

## Verification Criteria

- [ ] No new direct SQL composition in enhanced paths.
- [ ] Helper selection path is logged for at least one flow per datasource type tested.
- [ ] Fallback paths are deterministic and user-visible.
