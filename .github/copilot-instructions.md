# Copilot Instructions for The_Monster Unity Project

## Project Overview
- This is a Unity game project. Core logic is in `Assets/Scripts/`.
- Scenes are in `Assets/Scenes/`. Editor scripts are in `Assets/Editor/`.
- The project uses Unity's Input System (`InputSystem_Actions.inputactions`).

## Architecture & Patterns
- Scripts are organized by feature (e.g., `Wagering`, `PlayerState`).
- Use Unity MonoBehaviour for gameplay logic. Avoid static managers unless necessary.
- Data/configuration assets are stored in `Assets/Settings/`.
- Scene objects are referenced via serialized fields, not `Find()` or singletons.

## Developer Workflows
- Open the project in Unity Hub for editing and playtesting.
- Build settings and platform targets are managed via `ProjectSettings/`.
- To add new input actions, edit `InputSystem_Actions.inputactions` and regenerate C# classes.
- Use Unity's built-in test runner for playmode and editmode tests (if present).

## Conventions
- Use PascalCase for class names, camelCase for fields and methods.
- Group related scripts in subfolders under `Assets/Scripts/`.
- Prefer UnityEvents and inspector wiring over hardcoded references.
- Place custom editor scripts in `Assets/Editor/`.

## Integration & Dependencies
- External packages are managed via `Packages/manifest.json`.
- Avoid direct file I/O; use Unity's APIs for asset and data management.
- Cross-component communication should use events or Unity's messaging system.

## Examples
- See `Assets/Scripts/Wagering/PlayerState.cs` for player state logic.
- Input handling is configured in `InputSystem_Actions.inputactions`.

## When in Doubt
- Follow existing folder and naming conventions.
- Reference similar scripts for implementation style.
- Ask for clarification if a pattern or workflow is unclear.
