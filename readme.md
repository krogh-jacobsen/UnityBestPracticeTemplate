# Unity Best Practices — `com.unity.best-practices`

A Unity package that enforces consistent C# coding standards, ships curated LLM instruction files to supercharge AI-assisted development, and provides performance-optimised import presets and editor utilities to accelerate day-to-day workflows.

Supports Unity 6000.3+.

---

## What's included

### C# Coding Style Guide
A living `CodeStyleGuide.cs` reference file that documents naming conventions, formatting rules, and architectural patterns for Unity C# projects. Acts as the single source of truth for the team's coding standards.

### LLM Instruction Files
Ready-to-use instruction files for AI coding assistants (Claude Code, GitHub Copilot, Gemini CLI, Codex). Drop them into your project's AI configuration to give your LLM deep context about your Unity codebase.

| File | Purpose |
|---|---|
| `UnityCodeStyleInstructions.md` | Enforces the project's C# style guide |
| `UnityPerformanceOptimizationInstructions.md` | Guides the LLM toward allocation-free, performant Unity patterns |
| `UnityDesignPatternsInstructions.md` | Promotes idiomatic Unity architecture |
| `UnityDebuggingInstructions.md` | Debugging approach and tooling conventions |
| `UnityUIToolkitInstructions.md` | UI Toolkit-specific patterns and best practices |
| `UnityProjectConfigurationInstructions.md` | Project settings and configuration guidance |
| `DocumentationTemplate.md` | Standard format for in-code documentation |
| `GameDesignDocument.md` | GDD template for consistent design documentation |
| `copilot-instructions.md` | GitHub Copilot workspace instructions |
| `custom-packages.md` | Documents custom package conventions |

**How to use:** Add the relevant files as context in your AI tool's configuration (e.g. reference them in `CLAUDE.md`, `.github/copilot-instructions.md`, or your IDE's LLM context settings).

### Import Presets
Performance-optimised importer presets that apply consistent, best-practice settings automatically when assets are imported.

**Textures**
- `AlbedoTextureImporter` — Albedo/diffuse maps
- `NormalTextureImporter` — Normal maps
- `SingleSpriteTextureImporter` — Single sprites
- `SpriteAtlasTextureImporter` — Sprite atlases
- `UISpriteTextureImporter` — UI sprites

**Audio**
- `SFXAudioImporter` — Sound effects
- `MusicAudioImporter` — Music tracks
- `AmbienceAudioImporter` — Ambience loops
- `UIAudioImporter` — UI feedback sounds

### Script Templates
Custom script templates available via **Assets > Create > Scripting** that generate boilerplate matching the project's coding style guide out of the box.

- MonoBehaviour Script
- ScriptableObject Script
- Empty C# Script
- UI Toolkit View

### Editor Utilities
Editor tooling to speed up common workflows, including folder structure generation to scaffold new Unity projects or feature modules consistently.

---

## Installation

### Via Unity Package Manager (recommended)

1. Open **Window > Package Manager**
2. Click the **+** button in the top-left corner
3. Select **Add package from git URL...**
4. Enter the following URL and click **Add**:

```
https://github.com/krogh-jacobsen/UnityBestPracticeTemplate.git
```

To lock to a specific version, append the tag:

```
https://github.com/krogh-jacobsen/UnityBestPracticeTemplate.git#1.0.2
```

### Via manifest.json

Add the following entry to your project's `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.unity.best-practices": "https://github.com/krogh-jacobsen/UnityBestPracticeTemplate.git"
  }
}
```

---

## Getting started

1. After installation, copy the LLM instruction files you need from `Packages/Unity Best Practices/Editor/LLMInstructions/` into your project and reference them in your AI tool's configuration (e.g. `CLAUDE.md`, `.github/copilot-instructions.md`).
2. Configure the import presets in **Edit > Project Settings > Preset Manager**, mapping each preset to the appropriate folder filter.
3. Use the script templates from **Assets > Create > Scripting** when creating new scripts.
