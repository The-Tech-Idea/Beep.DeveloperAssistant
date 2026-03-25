# Risk Register and Cutover Checklists

## Risk Register

| Risk | Impact | Mitigation | Phase |
|---|---|---|---|
| Capability duplication across projects | High | Enforce ownership model and wrapper boundaries | 01, 05 |
| Inconsistent generation outputs | High | Template/profile standards + snapshot tests | 08, 09 |
| Provider/helper resolution errors | High | Runtime capability checks + fallback diagnostics | 03 |
| Bootstrap side effects/re-registration | Medium | EnvironmentService idempotent guardrails | 04 |
| Menu/node discoverability gaps | Medium | Coverage mapping and thin-adapter standard | 07 |
| Breaking user workflows | High | Additive rollout and compatibility stages | 10 |

## Cutover Checklist

- Architecture boundaries documented and reviewed.
- ClassCreator integration wrappers validated by dry-run.
- Universal helper resolution and fallback diagnostics verified.
- UI workflows include validation + preview + progress/cancel.
- MenuCommands and Nodes coverage map completed.
- Compatibility verification completed before deprecation.
