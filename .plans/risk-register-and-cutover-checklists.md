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

## Risk Mitigation TODOs

- [ ] Add risk owner and review cadence for each risk.
- [ ] Add probability and detection method columns.
- [ ] Add trigger signals for each risk (what indicates risk is occurring).
- [ ] Add contingency steps with rollback commands/actions.
- [ ] Review and update risk register at each rollout stage gate.

## Cutover Execution Checklist (Detailed)

- [ ] Pre-cutover:
  - [ ] test matrix green
  - [ ] dry-run validation complete
  - [ ] feature flags prepared
- [ ] Cutover:
  - [ ] enable target feature flags
  - [ ] validate menu/node/winform key paths
  - [ ] monitor diagnostics for regression signals
- [ ] Post-cutover:
  - [ ] collect issues and classify severity
  - [ ] execute rollback if critical failure threshold exceeded
  - [ ] finalize adoption note and update standards matrix
