---
description: Unity Editor and project configuration for optimal workflow
applyTo: "**"
---

# Unity Project Configuration

This document covers Unity Editor settings and project configuration optimized for faster iteration and consistent asset imports.

## Table of Contents

- [Version Control Settings](#version-control-settings)
- [Enter Play Mode Options](#enter-play-mode-options)
- [Script Compilation Settings](#script-compilation-settings)
- [Asset Pipeline Settings](#asset-pipeline-settings)
- [Burst Compiler Settings](#burst-compiler-settings)
- [Presets](#presets)
- [Rendering Configuration](#rendering-configuration)
- [Build Settings](#build-settings)
- [Assembly Definitions](#assembly-definitions)
- [Quick Reference](#quick-reference)
- [Troubleshooting](#troubleshooting)

---

## Version Control Settings

**Location:** Edit → Project Settings → Editor → Version Control / Asset Serialization

| Setting | Value | Why |
|---------|-------|-----|
| Mode (Version Control) | Visible Meta Files | Ensures `.meta` files are written to disk so source control can track them |
| Asset Serialization Mode | Force Text | Stores assets as readable YAML — makes diffs meaningful and merges possible |

- ✅ Always configure both settings before the first commit on a new project.
- ❌ Never use Binary serialization on a team — binary asset files produce unresolvable merge conflicts.
- ℹ️ If `.meta` files are hidden, Unity regenerates them on import and changes GUIDs — this breaks all references to those assets.

---

## Enter Play Mode Options

**Location:** Edit → Project Settings → Editor

| Setting | Value | Why |
|---------|-------|-----|
| Enter Play Mode Options | ✅ Enabled | Allows customizing what reloads on play |
| Reload Domain | ❌ Disabled | Skips C# domain reload, saves 2-5 seconds per play |
| Reload Scene | ❌ Disabled | Keeps scene state, faster iteration |

### Caveats When Domain Reload is Disabled

With domain reload disabled, **static fields persist between play sessions**:

```csharp
// This value WON'T reset when you stop and start play mode
private static int s_playerCount = 0;

// Fix: Reset in RuntimeInitializeOnLoadMethod
[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
private static void ResetStatics()
{
    s_playerCount = 0;
}
```

**Also affected:**
- Static events (subscribers accumulate across plays)
- Singleton instances
- Static collections and caches

See [Unity Documentation: Domain Reloading](https://docs.unity3d.com/Manual/DomainReloading.html) for details.

---

## Script Compilation Settings

### Script Changes While Playing

**Location:** Edit → Project Settings → Editor → Script Changes While Playing

| Value | Behaviour | Recommended For |
|-------|-----------|-----------------|
| Recompile And Continue Playing | Recompiles in background, applies on next domain reload | Not recommended — can cause subtle state bugs |
| Recompile After Finished Playing | Queues recompile until you stop play mode | Safe default for most projects |
| **Stop Playing And Recompile** | Stops play mode immediately when a script change is detected | ✅ Recommended — avoids inconsistent editor state |

- ✅ Use **Stop Playing And Recompile** to prevent stale compiled code running while you edit.

### .editorconfig (Code Style)

Unity 6 respects an `.editorconfig` file at the project root for C# formatting rules. Rider and Visual Studio both use it.

```ini
# .editorconfig — place at project root (next to Assets/)
root = true

[*.cs]
indent_style = space
indent_size = 4
end_of_line = lf
charset = utf-8
trim_trailing_whitespace = true
insert_final_newline = true

# Enforce Allman brace style
csharp_new_line_before_open_brace = all
```

- ✅ Commit `.editorconfig` to version control so all team members share the same formatting rules.
- ✅ Unity's own C# templates respect `.editorconfig` when generating new scripts.

---

## Asset Pipeline Settings

**Location:** Unity → Settings (macOS) or Edit → Preferences (Windows)

| Setting | Value | Why |
|---------|-------|-----|
| Auto Refresh | ❌ Disabled | Manual control over when assets reimport |

**Manual refresh:** Press `Cmd+R` (macOS) or `Ctrl+R` (Windows) to refresh assets.

**Benefits:**
- No unexpected pauses during work
- Control when large imports happen
- Faster switching between IDE and Unity

---

## Burst Compiler Settings

**Location:** Edit → Project Settings → Burst AOT Settings

| Setting | Value | Why |
|---------|-------|-----|
| Enable Burst Compilation | ✅ Enabled | Significant performance gains for Jobs |
| Synchronous Compilation | ✅ Enabled | Compiles Burst jobs immediately, avoids first-frame stutters |

- ⚠️ **Synchronous Compilation** blocks the Editor thread during Burst compilation. This can slow Editor startup on large projects with many Burst jobs. Disable it if cold Editor startup time is noticeably slow, and re-enable when profiling runtime performance.

---

## Presets

Presets standardize import settings for assets. Located at `Assets/Settings/Presets/`.

### Texture Import Presets

| Preset | Use Case | Key Settings |
|--------|----------|--------------|
| `AlbedoTextureImporter` | Diffuse/color textures | sRGB, compressed |
| `NormalTextureImporter` | Normal maps | Linear, normal map format |
| `SingleSpriteTextureImporter` | Individual UI sprites | Sprite mode single |
| `SpriteAtlasTextureImporter` | Sprite atlas textures | Sprite mode multiple |

### Audio Import Presets

Located at `Assets/Settings/Presets/Audio/`:

| Preset | Use Case | Typical Settings |
|--------|----------|------------------|
| `MusicAudioImporter` | Background music | Streaming, high quality, no compression |
| `AmbienceAudioImporter` | Environmental loops | Compressed in memory, loop-friendly |
| `SFXAudioImporter` | Sound effects | Decompress on load, low latency |
| `UIAudioImporter` | UI feedback sounds | Small files, decompress on load |

### Applying Presets

**Automatic (via Preset Manager):**
1. Edit → Project Settings → Preset Manager
2. Add filter (e.g., folder path or name pattern)
3. Assign preset to filter

**Manual:**
1. Select asset in Project window
2. In Inspector, click preset icon (slider bars) at top right
3. Choose preset from dropdown

### Creating New Presets

1. Configure an asset's import settings as desired
2. Click preset icon → Save Current To...
3. Save to `Assets/Settings/Presets/`

---

## Rendering Configuration

**Location:** `Assets/Settings/Rendering/`

### URP Quality Tiers

Three quality levels for different target hardware:

| Asset | Target | Use Case |
|-------|--------|----------|
| `URP-Performant` | Mobile, low-end | Maximum performance, reduced features |
| `URP-Balanced` | Mid-range, default | Good balance of quality and performance |
| `URP-HighFidelity` | Desktop, high-end | Maximum visual quality |

Each quality tier has a matching Renderer asset (e.g., `URP-Balanced-Renderer`).

### Volume Profiles

| Asset | Purpose |
|-------|---------|
| `DefaultVolumeProfile` | Global post-processing defaults |
| `SampleSceneProfile` | Scene-specific overrides |

### Switching Quality at Runtime

```csharp
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// Switch to high quality
QualitySettings.SetQualityLevel(2); // Index depends on quality settings order
```

---

## Build Settings

### Platform Modules

Remove unused platform modules to reduce Editor overhead:

**Location:** Unity Hub → Installs → Add Modules

Keep only platforms you're targeting. Common setups:
- **Desktop only:** Windows, macOS, Linux
- **Mobile:** Android, iOS
- **Console:** Platform-specific SDKs

### Build Profiles

Located at `Assets/Settings/Build Profiles/`:

Build profiles store platform-specific build configurations including:
- Target platform
- Development/Release mode
- Scripting backend (Mono/IL2CPP)
- Compression settings

---

## Assembly Definitions

Assembly definitions reduce recompilation scope and enforce code boundaries. For naming conventions, recommended project structure, dependency rules, and editor-only assembly patterns, see **`UnityAssemblyDefinitionsInstructions.md`**.

**Quick decision guide:**
- ✅ Add assembly definitions when a feature module is stable and unlikely to change frequently.
- ✅ Always create a separate Editor assembly (with `Editor` platform constraint) for any Editor-only code.
- ❌ Don't create an assembly per script — the overhead outweighs the benefit for small file counts.
- ℹ️ Unity compiles one default assembly (`Assembly-CSharp`) for all scripts without an `.asmdef`. Starting without assembly definitions is fine; add them incrementally as the project grows.

---

## Quick Reference

### Keyboard Shortcuts

| Action | macOS | Windows |
|--------|-------|---------|
| Refresh Assets | `Cmd+R` | `Ctrl+R` |
| Enter Play Mode | `Cmd+P` | `Ctrl+P` |
| Pause | `Cmd+Shift+P` | `Ctrl+Shift+P` |
| Step Frame | `Cmd+Alt+P` | `Ctrl+Alt+P` |

### Settings Locations Summary

| Setting | Path |
|---------|------|
| Enter Play Mode | Edit → Project Settings → Editor |
| Auto Refresh | Unity → Settings (Preferences) |
| Burst | Edit → Project Settings → Burst AOT Settings |
| Quality Levels | Edit → Project Settings → Quality |
| Preset Manager | Edit → Project Settings → Preset Manager |
| URP Assets | Assets/Settings/Rendering/ |
| Presets | Assets/Settings/Presets/ |

---

## Troubleshooting

### Static fields not resetting between Play sessions

**Symptom:** Values from a previous Play session persist when you press Play again — singletons, counters, or caches have stale state.

**Cause:** Domain Reload is disabled (Enter Play Mode Options).

**Fix:** Reset statics explicitly with `[RuntimeInitializeOnLoadMethod]`:

```csharp
[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
private static void ResetStatics()
{
    s_instance = null;
    s_playerCount = 0;
    s_cache.Clear();
}
```

Also watch for **static events** — subscribers accumulate across Play sessions if not unsubscribed. Always unsubscribe in `OnDisable`/`OnDestroy`.

### Preset not auto-applying on import

**Symptom:** Assets imported into a folder don't pick up the expected preset.

**Check:**
1. Edit → Project Settings → Preset Manager — verify the filter path exactly matches the import folder (paths are case-sensitive on macOS/Linux).
2. The preset filter uses **folder path** matching — the asset must be *inside* the specified folder, not in a parent folder.
3. If multiple presets match, the first matching rule wins — check the order.

### Burst jobs stuttering on first use in Editor

**Symptom:** First execution of a Burst-compiled job has a noticeable hitch.

**Fix:** Enable **Synchronous Compilation** in Edit → Project Settings → Burst AOT Settings. Note this slows Editor startup — see the [Burst Compiler Settings](#burst-compiler-settings) caveat above.

### `.meta` files regenerated / GUIDs changed after pull

**Symptom:** After pulling changes, assets have new GUIDs and all scene/prefab references to them are broken.

**Cause:** `.meta` files were not committed (Version Control Mode was not set to Visible Meta Files), or `.meta` files were in `.gitignore`.

**Fix:**
1. Set Version Control Mode to **Visible Meta Files** (Edit → Project Settings → Editor).
2. Ensure `.meta` files are **not** in `.gitignore`.
3. Commit all `.meta` files alongside their assets.

---

## Version Information

- **Target Unity Version:** Unity 6.3 (6000.3.x)
- **Render Pipeline:** URP 17.3.0
- **Last Updated:** February 2026
