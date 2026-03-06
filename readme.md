# Unity Best Practices — `com.unity.best-practices`

A Unity package that enforces consistent C# coding standards, ships curated LLM instruction files to supercharge AI-assisted development, and provides performance-optimised import presets and editor utilities to accelerate day-to-day workflows.

Supports **Unity 6000.3+** · License: **MIT**

---

## Quick Start

After installing the package, run these steps from the Unity menu bar:

| Step | Menu Path | What it does |
|------|-----------|--------------|
| 1 | **Window → Best Practices → Setup Project Folders** | Scaffolds the recommended folder structure under `Assets/_ProjectName` |
| 2 | **Window → Best Practices → Generate Assembly Definitions** | Creates `.asmdef` files in Scripts, Editor, and Tests folders |
| 3 | **Window → Best Practices → Configure Import Presets** | Registers all import presets in the Preset Manager with matching folder filters |
| 4 | **Window → Best Practices → Generate .gitignore** | Creates a Unity-optimised `.gitignore` at the project root |
| 5 | **Window → Best Practices → Generate .editorconfig** | Creates an `.editorconfig` matching the C# style guide |
| 6 | Copy LLM instruction files (see [below](#llm-instruction-files)) | Gives your AI assistant deep Unity context |

> **Tip:** Rename the `_ProjectName` root folder to your actual project name after scaffolding, then update the preset glob paths in **Edit → Project Settings → Preset Manager** to match.

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

**How to use per tool:**

- **Claude Code** — Copy the files you need and reference them in your `CLAUDE.md`
- **GitHub Copilot** — Copy into `.github/copilot-instructions.md` or reference via workspace settings
- **Gemini CLI / Codex** — Add to your IDE's LLM context configuration

The instruction files are located in `Packages/Unity Best Practices/Editor/LLMInstructions/`.

### Import Presets
Performance-optimised importer presets that apply consistent, best-practice settings automatically when assets are dropped into the matching folder.

**Textures**

| Preset | Target Folder |
|--------|---------------|
| `AlbedoTextureImporter` | `Art/Textures/Albedo/` |
| `NormalTextureImporter` | `Art/Textures/Normal/` |
| `RoughnessTextureImporter` | `Art/Textures/Roughness/` |
| `MaskTextureImporter` | `Art/Textures/Mask/` |
| `HDRITextureImporter` | `Art/Textures/HDRI/` |
| `SingleSpriteTextureImporter` | `UI/Sprites/` |
| `SpriteAtlasTextureImporter` | `UI/Sprites/Atlas/` |

**Audio**

| Preset | Target Folder |
|--------|---------------|
| `SFXAudioImporter` | `Art/Audio/SFX/` |
| `MusicAudioImporter` | `Art/Audio/Music/` |
| `AmbienceAudioImporter` | `Art/Audio/Ambience/` |
| `UIAudioImporter` | `Art/Audio/UI/` |

**Models & Animations**

| Preset | Target Folder |
|--------|---------------|
| `FBXModelImporter` | `Art/Models/` |
| `FBXAnimationImporter` | `Art/Animations/` |

> Run **Window → Best Practices → Configure Import Presets** to register all of the above automatically. You can review and edit them in **Edit → Project Settings → Preset Manager**.

### Script Templates
Custom script templates available via **Assets → Create → Scripting** that generate boilerplate matching the project's coding style guide out of the box.

- MonoBehaviour Script
- ScriptableObject Script
- Empty C# Script
- UI Toolkit View

### Editor Utilities

| Menu Item | Description |
|-----------|-------------|
| **Window → Best Practices → Setup Project Folders** | Creates the full folder structure (see below) |
| **Window → Best Practices → Generate Assembly Definitions** | Creates `.asmdef` files using `CompanyName.ProductName` as the root namespace |
| **Window → Best Practices → Configure Import Presets** | Registers presets in the Preset Manager |
| **Window → Best Practices → Generate .gitignore** | Creates a Unity-optimised `.gitignore` at the project root |
| **Window → Best Practices → Generate .editorconfig** | Creates an `.editorconfig` enforcing the C# style guide in IDEs |
| **Window → Best Practices → Hello editor** | Verifies the editor assembly is loaded |
| **Window → Best Practices → Hello editor using package** | Verifies editor → runtime assembly reference |

---

## Project Folder Structure

Running **Setup Project Folders** creates the following layout:

```
Assets/
└── _ProjectName/
    ├── Art/
    │   ├── Animations/
    │   │   ├── Clips/
    │   │   └── Controllers/
    │   ├── Audio/
    │   │   ├── Ambience/
    │   │   ├── Music/
    │   │   ├── SFX/
    │   │   └── UI/
    │   ├── Fonts/
    │   ├── Materials/
    │   │   └── Physics/
    │   ├── Models/
    │   ├── Shaders/
    │   ├── Sprites/
    │   ├── Textures/
    │   │   ├── Albedo/
    │   │   ├── Normal/
    │   │   ├── Roughness/
    │   │   ├── Mask/
    │   │   └── HDRI/
    │   └── VFX/
    ├── Scripts/
    │   ├── Core/
    │   ├── UI/
    │   └── Utilities/
    ├── Editor/
    ├── UI/
    │   ├── Sprites/
    │   │   └── Atlas/
    │   ├── UXML/
    │   ├── USS/
    │   └── Settings/
    ├── Data/
    ├── Prefabs/
    ├── Scenes/
    ├── Settings/
    ├── Resources/
    ├── StreamingAssets/
    ├── Documentation/
    ├── Plugins/
    └── ThirdParty/
```

> UI sprites live under `UI/Sprites/` (not `Art/Textures/`) because they are logically part of the UI system rather than general art assets.

---

## Installation

### Via Unity Package Manager (recommended)

1. Open **Window → Package Manager**
2. Click the **+** button in the top-left corner
3. Select **Add package from git URL…**
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

## Troubleshooting

| Problem | Solution |
|---------|----------|
| **Package fails to install from Git** | Ensure you have Git installed and accessible from your system PATH. Check the Unity Console for detailed error messages. |
| **Presets not applying to imported assets** | Verify the glob paths in **Edit → Project Settings → Preset Manager** match your actual folder names. If you renamed `_ProjectName`, update the filters. |
| **Script templates not appearing** | Restart Unity after installation. Templates appear under **Assets → Create → Scripting**. |
| **"Hello editor using package" logs an error** | The runtime assembly may not be referenced. Check that the Editor `.asmdef` references the Runtime `.asmdef`. |
| **Folder structure already partially exists** | Safe to re-run — `Setup Project Folders` skips folders that already exist. |
| **Assembly definitions not generating** | Run `Setup Project Folders` first. The generator requires `Assets/_ProjectName` to exist. |
| **`.gitignore` / `.editorconfig` not appearing** | Check the project root folder (parent of `Assets/`). The files are created outside the `Assets/` directory and won't appear in the Unity Project window. |

---

## Contributing

1. Clone the repository
2. Open the dev project at `UnityBestPracticesPackage-Dev/` in Unity
3. The package source lives in `LocalPackages/UnityBestPracticeTemplate/`
4. Make your changes and test via the menu items under **Window → Best Practices**
5. Submit a pull request

---

## License

MIT — see [LICENSE](LICENSE) for details.
