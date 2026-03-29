# 04 - EnvironmentService Bootstrap and Safety

## Objective

Integrate environment bootstrap flows so generated artifacts and configs are consistent.

## Planned Enhancements

- Add startup/bootstrap logic wrappers that:
  - create required root/app folders
  - register default mappings/connections/query repositories
  - validate call order and idempotent initialization

## Validation and Guardrails

- Validate `IDMEEditor` and path outputs before proceeding.
- Add duplicate registration guards.
- Add explicit error messages for bootstrap sequence failures.

## Target Files

- bootstrap utilities under `Beep.DeveloperAssistant.Logic`
- startup/initialization call paths used by Winform/MenuCommands/Nodes

## Execution TODOs

- [ ] Add bootstrap orchestrator method with explicit step order:
  1) ensure folders
  2) ensure mappings
  3) ensure connections
  4) ensure queries
- [ ] Add idempotency checks for each step.
- [ ] Add explicit rollback/compensation note for partial bootstrap failures.
- [ ] Add bootstrap diagnostics object with step-by-step status.
- [ ] Expose dry-run bootstrap validation mode.

## Verification Criteria

- [ ] Running bootstrap twice does not duplicate registrations.
- [ ] Bootstrap failure logs include failing step and actionable message.
- [ ] Dry-run mode reports all planned actions without mutating files/config.
