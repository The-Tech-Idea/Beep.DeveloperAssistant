# 10 - Rollout, Compatibility, and Adoption

## Objective

Roll out enhancements safely while preserving existing plugin workflows.

## Rollout Strategy

- Stage 1: Additive integration (no API breakage).
- Stage 2: Feature flags/default profile activation.
- Stage 3: Deprecation notices for old redundant paths.
- Stage 4: Cleanup and consolidation after adoption.

## Compatibility Rules

- Preserve existing menu command signatures during migration.
- Keep node/page entry points stable.
- Keep default behavior unchanged unless user enables enhanced profiles.
