 # Changelog

All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this package adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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