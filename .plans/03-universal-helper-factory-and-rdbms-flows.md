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
