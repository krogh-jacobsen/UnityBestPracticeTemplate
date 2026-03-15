# Unity Project Configurator — `com.unity.best-practices`

A Unity editor toolkit that accelerates project setup, enforces consistent C# coding standards, and supercharges AI-assisted development with curated LLM instruction files and agent skills.

Supports **Unity 6000.3+** · License: **MIT**

---

## Quick Start

Open the dashboard from the top menu — it shows live status of every setup step:

**Tools → Unity Project Configurator → Open Dashboard**

For a one-click full setup, use the wizard:

**Tools → Unity Project Configurator → Setup → New Project Wizard**

Or run individual steps:

| Step | Action | Menu Path | Description |
|------|--------|-----------|-------------|
| 1 | **Open Dashboard** | Tools → Unity Project Configurator | Project health overview — live status of all steps |
| 2 | **Setup Project Folders** | Tools → Unity Project Configurator → Setup | Scaffolds the recommended folder structure |
| 3 | **Configure Project Settings** | Tools → Unity Project Configurator → Setup | Applies version control, enter play mode options, etc. |
| 4 | **Configure Import Presets** | Tools → Unity Project Configurator → Setup | Registers import presets in Preset Manager |
| 5 | **Setup Tags and Layers** | Tools → Unity Project Configurator → Setup | Applies tags and layers from a config asset |
| 6 | **Generate .gitignore** | Tools → Unity Project Configurator → Version Control | Creates a Unity-optimised `.gitignore` |
| 7 | **Generate .editorconfig** | Tools → Unity Project Configurator → Code | Creates an `.editorconfig` enforcing the C# style guide |
| 8 | **Copy AI Files to Project** | Tools → Unity Project Configurator → AI | Copies LLM instruction files and agent skills into your project |

---

## What's Included

### Project Dashboard
A single-window overview of your project's setup health. Each card shows the current status and lets you apply the corresponding setting or tool without leaving the window. Every card has a `?` button with a brief explainer.

**Open:** Tools → Unity Project Configurator → Open Dashboard

### New Project Wizard
Runs all setup steps in one click: creates the folder structure, applies project settings, configures import presets, sets up tags and layers, and generates `.gitignore` and `.editorconfig`.

**Open:** Tools → Unity Project Configurator → Setup → New Project Wizard

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
| `UnityAudioInstructions.md` | Audio system patterns and best practices |
| `UnityAnimationInstructions.md` | Animation system patterns and best practices |
| `UnityInputSystemInstructions.md` | Input System usage and conventions |
| `DocumentationTemplate.md` | Standard format for in-code documentation |
| `GameDesignDocument.md` | GDD template for consistent design documentation |
| `copilot-instructions.md` | GitHub Copilot workspace instructions |
| `custom-packages.md` | Documents custom package conventions |

**How to use per tool:**

- **Claude Code** — Copy the files you need and reference them in your `CLAUDE.md`
- **GitHub Copilot** — Copy into `.github/copilot-instructions.md` or reference via workspace settings
- **Gemini CLI / Codex** — Add to your IDE's LLM context configuration

Run **Tools → Unity Project Configurator → AI → Copy AI Files to Project** to copy all instruction files into your project with one click.

### Agent Skills

The package includes a VS Code / GitHub Copilot agent skill file (`SKILL.md`) that exposes the instruction files as a named skill for use in agent mode. Once copied into a `.github/` folder that Copilot can discover, the skill can be invoked by name to pull in deep Unity context on demand.

### Import Presets
Performance-optimised importer presets that apply consistent, best-practice settings automatically when assets are dropped into the matching folder.

**Textures**

| Preset | Matches |
|--------|----------|
| `AlbedoTextureImporter` | `Assets/**/Textures/Albedo/**` |
| `NormalTextureImporter` | `Assets/**/Textures/Normal/**` |
| `RoughnessTextureImporter` | `Assets/**/Textures/Roughness/**` |
| `MaskTextureImporter` | `Assets/**/Textures/Mask/**` |
| `HDRITextureImporter` | `Assets/**/Textures/HDRI/**` |
| `SingleSpriteTextureImporter` | `Assets/**/UI/Sprites/**` |
| `SpriteAtlasTextureImporter` | `Assets/**/UI/Sprites/Atlas/**` |

**Audio**

| Preset | Matches |
|--------|----------|
| `SFXAudioImporter` | `Assets/**/Audio/SFX/**` |
| `MusicAudioImporter` | `Assets/**/Audio/Music/**` |
| `AmbienceAudioImporter` | `Assets/**/Audio/Ambience/**` |
| `UIAudioImporter` | `Assets/**/Audio/UI/**` |

**Models & Animations**

| Preset | Matches |
|--------|----------|
| `FBXModelImporter` | `Assets/**/Art/Models/**` |
| `FBXAnimationImporter` | `Assets/**/Art/Animations/**` |

> Run **Tools → Unity Project Configurator → Setup → Configure Import Presets** to register all of the above automatically. You can review and edit them in **Edit → Project Settings → Preset Manager**.

### Script Templates
Custom script templates available via **Assets → Create → Scripting** that generate boilerplate matching the project's coding style guide out of the box.

- MonoBehaviour Script
- ScriptableObject Script
- Empty C# Script
- UI Toolkit View

### Tags & Layers Setup

Register a standard set of tags, sorting layers, and physics layers from a configurable ScriptableObject.

**Default Tags:** `Player`, `Enemy`, `NPC`, `Projectile`, `Pickup`, `Interactable`, `Checkpoint`, `SpawnPoint`, `Trigger`, `MainCamera`

**Default Sorting Layers (render order):**

| Order | Layer |
|-------|-------|
| 1 | `Background` |
| 2 | `Environment` |
| 3 | `Props` |
| 4 | `Characters` |
| 5 | `Foreground` |
| 6 | `UI` |
| 7 | `Overlay` |

**Default Physics Layers:**

| Slot | Layer |
|------|-------|
| 6 | `Player` |
| 7 | `Enemy` |
| 8 | `NPC` |
| 9 | `Projectile` |
| 10 | `Pickup` |
| 11 | `Interactable` |
| 12 | `Ground` |
| 13 | `Environment` |
| 14 | `Trigger` |
| 15 | `Ragdoll` |

**How to customise:**

1. Create a config asset: **Assets → Create → Project Configurator → Project Tags and Layers**
2. Edit the tags, sorting layers, and physics layers in the Inspector
3. Run **Tools → Unity Project Configurator → Setup → Setup Tags and Layers**

> If no config asset is found in the project, you'll be prompted to use the package defaults. Existing tags and layers are preserved — only new entries are added.

## Menu Reference

| Submenu | Item | Description |
|---------|------|-------------|
| *(root)* | **Open Dashboard** | Project health overview — live status of all setup steps with one-click apply buttons and `?` explainer cards |
| *(root)* | **Project Health** | Compact project health window |
| **Setup** | **New Project Wizard** | One-click guided wizard that runs all setup steps in sequence |
| **Setup** | **Setup Project Folders** | Scaffolds the recommended folder structure under `Assets/` |
| **Setup** | **Configure Project Settings** | Applies version control mode, enter play mode options, and other project settings |
| **Setup** | **Configure Import Presets** | Registers all import presets in Preset Manager with `**` wildcard folder filters |
| **Setup** | **Setup Tags and Layers** | Applies tags, sorting layers, and physics layers from a config asset |
| **Code** | **Assembly Definitions** | Opens the assembly definitions inspector window |
| **Code** | **Generate Assembly Definitions** | Creates `.asmdef` files using `CompanyName.ProductName` as the root namespace |
| **Code** | **Generate .editorconfig** | Creates an `.editorconfig` enforcing the C# style guide |
| **Version Control** | **Generate .gitignore** | Creates a Unity-optimised `.gitignore` at the project root |
| **Version Control** | **Generate .gitattributes** | Creates a `.gitattributes` file for Git LFS |
| **AI** | **Copy AI Files to Project** | Copies LLM instruction files and agent skills into your project |
| **Utilities** | **Layer Collision Matrix** | Visual editor for the physics layer collision matrix |
| **Utilities** | **PlayerPrefs Inspector** | Inspect and edit PlayerPrefs at runtime |
| **Utilities** | **Test Editor Log** | Verifies the editor assembly is loaded |
| **Utilities** | **Test Editor Log (calls runtime)** | Verifies editor → runtime assembly reference |

---

## Project Folder Structure

Running **Tools → Unity Project Configurator → Setup → Setup Project Folders** creates the following layout:

```
Assets/
├── Editor/
├── Resources/
├── Documentation/
├── ThirdParty/
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
    ├── StreamingAssets/
    └── Plugins/
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
https://github.com/krogh-jacobsen/UnityBestPracticeTemplate.git#1.4.2
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
| **Dashboard shows all sections as errors** | Click **Refresh** — the window auto-refreshes on open but styles may not have initialised on first paint. |
| **Package fails to install from Git** | Ensure you have Git installed and accessible from your system PATH. Check the Unity Console for detailed error messages. |
| **Presets not applying to imported assets** | Verify the glob paths in **Edit → Project Settings → Preset Manager**. Presets use `**` wildcard patterns and match the target folder name anywhere under `Assets/` — no manual filter updates needed after renaming. |
| **Script templates not appearing** | Restart Unity after installation. Templates appear under **Assets → Create → Scripting**. |
| **"Test Editor Log (calls runtime)" logs an error** | The runtime assembly may not be referenced. Check that the Editor `.asmdef` references the Runtime `.asmdef`. |
| **Folder structure already partially exists** | Safe to re-run — `Setup Project Folders` skips folders that already exist. |
| **Assembly definitions not generating** | Run `Setup Project Folders` first. The generator requires `Assets/_ProjectName` to exist. |
| **`.gitignore` / `.editorconfig` not appearing** | Check the project root folder (parent of `Assets/`). The files are created outside the `Assets/` directory and won't appear in the Unity Project window. |
| **Tags and layers not appearing after setup** | Check `Edit → Project Settings → Tags and Layers`. If a physics layer slot was already occupied, a warning is logged in the Console. |
| **Multiple ProjectTagsAndLayers configs found** | The tool uses the first one found. Keep only one config asset in your project. |

---

## Contributing

1. Clone the repository
2. Open the dev project at `UnityBestPracticesPackage-Dev/` in Unity
3. The package source lives in `LocalPackages/UnityBestPracticeTemplate/`
4. Make your changes and test via the menu items under **Tools → Unity Project Configurator**
5. Submit a pull request

---

## License

MIT — see [LICENSE](LICENSE) for details.
