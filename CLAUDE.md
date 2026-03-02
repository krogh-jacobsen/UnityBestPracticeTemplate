# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Package identity

- Package ID: `com.unity.best-practices`
- Unity minimum version: 6000.3
- Installed via git URL: `https://github.com/krogh-jacobsen/UnityBestPracticeTemplate.git`
- Two assemblies: `com.unity.best-practices.Runtime.asmdef` and `com.unity.best-practices.Editor.asmdef`

## Development workflow

This is a Unity package with no standalone build or test runner. All validation happens inside a host Unity project that has the package installed locally or via git.

- **Install locally**: Add `"com.unity.best-practices": "file:../LocalPackages/UnityBestPracticeTemplate"` to the host project's `Packages/manifest.json`.
- **Script templates**: Changes take effect only after restarting the Unity Editor — templates are registered at startup.
- **Import presets**: Must be wired up in **Edit > Project Settings > Preset Manager** with folder-path filters. They are not auto-applied by the package itself.
- **Editor utilities**: Available at **Window > Best Practices** in the Unity menu bar.
- **Folder scaffold**: `EditorController.cs` — menu item "Setup Project Folders" — creates `_Project/{Art,Audio,Prefabs,Scripts,Scenes,Settings,UI}` in the host project.

## Architecture

### Runtime vs Editor split
`Runtime/` contains only things needed at runtime (e.g. `CodeStyleGuide.cs` as a reference class). `Editor/` contains everything that should be stripped from builds: utilities, presets, script templates, and all LLM instruction files.

### LLM instruction files (`Editor/LLMInstructions/`)
The primary product of this package. Two consumption patterns:
1. **Individual files** — reference specific files in AI tool config (e.g. `CLAUDE.md`, `.github/copilot-instructions.md`) for targeted guidance.
2. **`copilot-instructions.md`** — a single consolidated file (~1400 lines) that merges all instruction topics. Use this when you need everything in one place.

The other files are not duplicates of each other — each covers a distinct domain:

| File | Domain |
|---|---|
| `UnityCodeStyleInstructions.md` | Naming, formatting, class organisation |
| `UnityPerformanceOptimizationInstructions.md` | Allocation avoidance, caching, hot-path patterns |
| `UnityDesignPatternsInstructions.md` | SOLID, Observer, State, Object Pool, Service Locator |
| `UnityDebuggingInstructions.md` | Triage order, per-system debugging strategies |
| `UnityUIToolkitInstructions.md` | USS/UXML, BEM naming, data binding, MVP pattern |
| `UnityProjectConfigurationInstructions.md` | Preset Manager, Enter Play Mode settings, Burst |
| `DocumentationTemplate.md` | README structure and tone guide |
| `GameDesignDocument.md` | Full GDD template |
| `custom-packages.md` | Package manifest conventions |

### Script templates (`Editor/ScriptTemplates/`)
File naming follows Unity's convention: `{priority}-{menu path}-{default filename}.cs.txt`. Menu separators use `__` and item separators use `_`. The UI Toolkit View template depends on `UITKBaseClass` from a `Core.UI` namespace — that base class must exist in the host project.

## C# conventions enforced by this package

These apply to all `.cs` files in this repo and in host projects using the LLM instructions.

**Field prefixes**
- `m_` — private instance fields
- `s_` — static fields
- `k_` — constants

**Class member order**
1. Using statements → namespace → fields → properties → events → MonoBehaviour methods (Awake, OnEnable, Start, OnDisable, Update order) → public methods → private methods

**Method naming**
- `ProcessX` — game logic
- `HandleX` — event callbacks
- `IsX` / `HasX` / `CanX` — boolean queries

**Events**
- Past-tense names (e.g. `PlayerDied`, `ScoreChanged`)
- Subscribe in `OnEnable`, unsubscribe in `OnDisable`

**Braces**: Allman style (opening brace on its own line).

**UXML/USS**: kebab-case with BEM pattern (`block__element--modifier`).

## Adding new content

- **New LLM instruction file**: create in `Editor/LLMInstructions/`, add a `.meta` file with `DefaultImporter` block and a unique GUID (generate with `python3 -c "import uuid; print(uuid.uuid4().hex)"`), and update `copilot-instructions.md` to include the content.
- **New preset**: add `.preset` file under `Editor/Presets/{category}/` with a matching `.meta` file; document it in `Editor/Presets/readme.md`.
- **New script template**: follow the `{priority}-{menu}__{submenu}-{filename}.cs.txt` naming pattern; update `Editor/ScriptTemplates/readme.md`.
- **Version bump**: update `package.json` version and add an entry to `CHANGELOG.md`.

## Meta files

Every file Unity tracks requires a `.meta` sibling. When creating files manually:
- Use `DefaultImporter` for plain text/markdown files
- Empty YAML values must have a trailing space: `userData: ` not `userData:`
- Generate GUIDs with `python3 -c "import uuid; print(uuid.uuid4().hex)"`