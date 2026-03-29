# 01 - Solution Architecture and Boundaries

## Objective

Define ownership boundaries across projects and prevent feature duplication.

## Ownership Model

- `Beep.DeveloperAssistant.Logic`
  - Core orchestration services and reusable capability wrappers.
- `Beep.DeveloperAssistant.WinformCore`
  - UX shells, input validation, workflow orchestration.
- `Beep.DeveloperAssistant.Logic.MenuCommands`
  - Menu command exposure and command-to-logic binding.
- `Beep.DeveloperAssistant.Nodes`
  - Branch/node discovery and page/command launch integration.

## Boundary Rules

- Logic project must not depend on Winform controls.
- UI projects call logic through explicit service interfaces/wrappers.
- MenuCommands and Nodes must remain thin adaptors over logic services.

## Target Files

- `Beep.DeveloperAssistant.Logic/*.csproj` and namespace boundaries
- `Beep.DeveloperAssistant.WinformCore/*.csproj`
- `Beep.DeveloperAssistant.Logic.MenuCommands/*.cs`
- `Beep.DeveloperAssistant.Nodes/*.cs`

## Execution TODOs

- [ ] Create `Architecture-Boundary-Checklist.md` with allowed dependency directions.
- [ ] Validate project references do not introduce Logic -> Winform dependency.
- [ ] Add "thin adaptor" rule checks for MenuCommands and Nodes (manual checklist first).
- [ ] Identify and list methods in MenuCommands/Nodes that contain business logic and move candidates.
- [ ] Add namespace conventions section for new partial classes and helpers.

## Verification Criteria

- [ ] No direct references from `Logic` project to Winform assemblies/namespaces.
- [ ] At least one reviewed MenuCommands class and one Node class follow thin-adaptor pattern.
- [ ] Boundary checklist reviewed and linked by later phases (`06a`, `07a`, `07b`).
