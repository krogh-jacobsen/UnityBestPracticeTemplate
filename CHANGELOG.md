# Changelog

All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this package adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.3.3] - 2026-03-09
### Added
- `Editor/AgentSkills/` folder — agent skill prompt files for use with Claude Code and GitHub Copilot
- `UnityCodeReview.md` — skill that reviews C# code against `UnityCodeStyleInstructions.md` and `UnityPerformanceOptimizationInstructions.md`, producing a severity-rated issues table with a Pass/Fail verdict
- `UnityUIToolkitCreate.md` — skill for generating UI Toolkit UI (UXML + USS + C# View/Model/Presenter) following `UnityUIToolkitInstructions.md` conventions

### Changed
- `UnityDebuggingInstructions.md` — removed domain-specific sections (Input System, Animation, Audio, Performance, UI Toolkit debugging) now that they live in their respective domain files; added overview table pointing to each domain file
- `UnityInputSystemInstructions.md` — added Troubleshooting section (device connection, PlayerInput, Input Action debugging, common issues table)
- `UnityAnimationInstructions.md` — added Troubleshooting section (Animator state/parameter issues, Animation Events, Root Motion)
- `UnityAudioInstructions.md` — added Troubleshooting section (AudioSource, spatial audio, AudioMixer issues)
- `UnityPerformanceOptimizationInstructions.md` — added Troubleshooting section (diagnosing performance issues, Profiler setup, finding hidden allocations)
- `UnityProjectConfigurationInstructions.md` — added Version Control Settings and Script Compilation Settings sections, expanded TOC, added Troubleshooting section, corrected `applyTo` frontmatter scope, replaced hardcoded paths with general guidance

## [1.3.2] - 2026-03-04
### Added
- `Editor/Dashboard/ProjectDashboardData.cs` — stateless data collector driving all dashboard checks:
  - **Folder Structure** — verifies all expected `_ProjectName` subfolders exist
  - **Import Presets** — verifies all expected `.preset` assets exist in the package
  - **LLM Instruction Files** — verifies all expected markdown instruction files exist
  - **Git & IDE Config** — checks for `.git` repo, `.gitignore`, and `.editorconfig`
  - **Project Health** — checks assembly definitions, Tags & Layers config, scenes in Build Settings, company/product name defaults, scripting backend, oversized textures (>2048px), and uncompressed audio (PCM/ADPCM)
- `Editor/Dashboard/ProjectDashboardWindow.cs` — `EditorWindow` rendering all sections with colour-coded status icons, per-section action buttons, and a summary banner; open via **Window → Best Practices → Project Dashboard** or **Shift+Alt+D**

## [1.3.1] - 2026-03-04
### Added
- `ProjectDashboardWindow.cs` — **Window → Best Practices → Project Dashboard** opens a single EditorWindow showing the health of the entire project at a glance
- `ProjectDashboardData.cs` — stateless data collector that drives the dashboard; checks are separated from rendering following single responsibility
- **Folder Structure** section — shows which expected `_ProjectName` folders exist vs are missing, with a one-click "Setup Project Folders" button
- **Import Presets** section — verifies each expected `.preset` asset exists in the package, with a one-click "Configure Import Presets" button
- **LLM Instruction Files** section — checks all expected instruction markdown files are present in the package
- **Git & IDE Config** section — checks for `.git` repo, `.gitignore`, and `.editorconfig` with generation buttons
- **Project Health** section — checks assembly definitions, Tags & Layers config, scenes in Build Settings, company/product name defaults, and scripting backend; includes buttons to open Player Settings and Build Settings
- Summary banner at the top of the window showing total error and warning counts across all sections

## [1.3.0] - 2026-03-04
### Added
- `ProjectTagsAndLayers.cs` — ScriptableObject config for defining custom tags, sorting layers, and physics layers (create via **Assets → Create → Best Practices → Project Tags and Layers**)
- `SetupTagsAndLayers.cs` — **Window → Best Practices → Setup Tags and Layers** reads the config and registers entries into `TagManager.asset`, preserving existing entries
- Default config ships with common game-development tags (`Player`, `Enemy`, `NPC`, `Projectile`, `Pickup`, `Interactable`, `Checkpoint`, `SpawnPoint`, `Trigger`), sorting layers (`Background`, `Environment`, `Props`, `Characters`, `Foreground`, `UI`, `Overlay`), and physics layers (`Player`, `Enemy`, `NPC`, `Projectile`, `Pickup`, `Interactable`, `Ground`, `Environment`, `Trigger`, `Ragdoll`)

## [1.2.0] - 2026-03-04
### Added
- `GenerateEditorConfig.cs` — **Window → Best Practices → Generate .editorconfig** creates an `.editorconfig` at the project root enforcing the package's C# naming conventions, formatting rules, and code style preferences for Rider, Visual Studio, and VS Code
- `GenerateGitIgnore.cs` — **Window → Best Practices → Generate .gitignore** creates a Unity-optimised `.gitignore` at the project root excluding Library, Temp, Logs, IDE files, OS metadata, and build artifacts
- `GenerateAssemblyDefinitions.cs` — **Window → Best Practices → Generate Assembly Definitions** scaffolds `.asmdef` files in Scripts, Editor, Tests/Runtime, and Tests/Editor using `CompanyName.ProductName` as the root namespace
- `SetupProjectFolders.cs` — extracted folder scaffolding into its own file from `EditorController.cs`
- `UI/Sprites` and `UI/Sprites/Atlas` folders — UI sprite assets now live under `UI/` rather than `Art/Textures/UI/`
- Texture subfolder creation (`Albedo`, `Normal`, `Roughness`, `Mask`, `HDRI`) to match preset filter paths
- Tests folder creation (`Tests/Runtime`, `Tests/Editor`) in folder scaffolding

### Changed
- Refactored `EditorController.cs` to follow single responsibility — generator logic extracted into `SetupProjectFolders.cs`, `GenerateEditorConfig.cs`, `GenerateGitIgnore.cs`, and `GenerateAssemblyDefinitions.cs`
- `EditorController.CreateFolder` is now `public` so it can be shared across generator classes
- Moved UI sprite preset glob filters from `Assets/_ProjectName/Art/Textures/UI/` to `Assets/_ProjectName/UI/Sprites/`
- All preset glob paths in `ConfigurePresets.cs` now consistently use the `Assets/_ProjectName/` prefix
- Updated `readme.md` with comprehensive documentation: Quick Start steps, folder structure diagram, preset tables, installation instructions, troubleshooting guide, and contributing section

### Fixed
- `autoReferenced` field in generated `.asmdef` files was always outputting `true` regardless of the configured value

## [1.1.1] - 2026-03-03
### Added
- `ConfigurePresets.cs` editor utility — **Window > Best Practices > Configure Import Presets** registers all package audio and texture import presets in the Preset Manager with glob folder-path filters in one click

## [1.1.0] - 2026-03-02
### Added
- New LLM instruction files: Testing, Input System, Addressables, Localization, Animation, Scene Management, Audio, Netcode, Assembly Definitions
- New script templates: Interface, Custom Editor, Editor Window, Property Drawer, Unit Test (EditMode), Unit Test (PlayMode), Static Event, Object Pool Handler
- New texture import presets: Mask, Roughness, HDRI
- Sample sets in `Samples/`: ScriptableObject Event System, Object Pooling, UI Toolkit MVP, State Machine
- Documentation files in `Documentation/`: QuickReference, PresetSetup, LLMSetup
- `samples` array in `package.json` for Package Manager UI integration
- `CLAUDE.md` for Claude Code project context

## [1.0.2] - 2026-03-02
### Added
- `CHANGELOG.md` to track version history

### Changed
- Updated `readme.md` to fully document all package features

## [1.0.1] - 2026 Mar 01
### Added
- LLM instruction files for Claude Code, GitHub Copilot, Gemini CLI, and Codex
  - `UnityCodeStyleInstructions.md`
  - `UnityPerformanceOptimizationInstructions.md`
  - `UnityDesignPatternsInstructions.md`
  - `UnityDebuggingInstructions.md`
  - `UnityUIToolkitInstructions.md`
  - `UnityProjectConfigurationInstructions.md`
  - `DocumentationTemplate.md`
  - `GameDesignDocument.md`
  - `copilot-instructions.md`
  - `custom-packages.md`
- Audio import presets: SFX, Music, Ambience, UI
- Texture import presets: Albedo, Normal, SingleSprite, SpriteAtlas, UISprite

## [1.0.0] - 2026 Feb 25
### Added
- Initial release
- `CodeStyleGuide.cs` Unity C# coding style guide reference
- Custom script templates: MonoBehaviour, ScriptableObject, Empty C#, UI Toolkit View
- Editor utilities including folder structure generation

### Upcoming features
- Additional Presets for models, animations, and other asset types
- Updated folder structure generation to support larger projects
- Example scripts demonstrating best practices in various domains (e.g. gameplay, UI, systems)
- Assembly Definitions
- Tests