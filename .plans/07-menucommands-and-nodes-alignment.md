# 07 - MenuCommands and Nodes Alignment

## Objective

Align menu commands and nodes so every logic capability is discoverable from both entry surfaces.

## Scope

- `Beep.DeveloperAssistant.Logic.MenuCommands/*`
- `Beep.DeveloperAssistant.Nodes/*`

## Planned Enhancements

- Build capability-to-command-to-node coverage map.
- Standardize naming and command metadata conventions.
- Keep menu/node handlers thin and delegate to logic services.
- Add missing command/node entries for new integrated capabilities.
- Detailed MenuCommands modernization work is tracked in `07a-menucommands-modernization-and-coverage.md`.
- Detailed Nodes synchronization work is tracked in `07b-nodes-sync-with-utilities-and-winformcore.md`.

## Execution TODOs

- [ ] Create `Menu-Node-Coverage-Master-Matrix.md` with columns:
  - capability
  - menu command
  - node action
  - winform entry
  - status
- [ ] Resolve mismatched naming between menu captions and node labels.
- [ ] Replace stubbed node action handlers for top-priority nodes.
- [ ] Ensure menu/node invoke same logic utility methods.
- [ ] Add defer list for intentionally hidden or low-priority capabilities.

## Verification Criteria

- [ ] Every top-priority capability has both menu and node entry (or explicit deferment).
- [ ] Node/menu action execution paths converge on shared utility methods.
- [ ] Coverage matrix is linked from `07a`, `07b`, and `07c`.
