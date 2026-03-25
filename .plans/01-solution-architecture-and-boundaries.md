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
