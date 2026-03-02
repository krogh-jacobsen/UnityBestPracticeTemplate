# Preset Manager Setup Guide

This package includes import presets for textures and audio. To apply them automatically when assets are imported into specific folders, wire them up in the Preset Manager.

## Step 1: Open Preset Manager

**Edit > Project Settings > Preset Manager**

## Step 2: Add texture presets

Click **+** next to **Texture Importer** and add one row per preset with the recommended folder filter:

| Preset Asset | Folder Filter |
|---|---|
| `AlbedoTextureImporter` | `Art/Textures/Albedo` |
| `NormalTextureImporter` | `Art/Textures/Normals` |
| `MaskTextureImporter` | `Art/Textures/Masks` |
| `RoughnessTextureImporter` | `Art/Textures/Roughness` |
| `HDRITextureImporter` | `Art/Textures/HDRI` |
| `SingleSpriteTextureImporter` | `Art/Sprites/Single` |
| `SpriteAtlasTextureImporter` | `Art/Sprites/Atlases` |
| `UISpriteTextureImporter` | `Art/UI` |

## Step 3: Add audio presets

Click **+** next to **Audio Importer**:

| Preset Asset | Folder Filter |
|---|---|
| `SFXAudioImporter` | `Audio/SFX` |
| `MusicAudioImporter` | `Audio/Music` |
| `AmbienceAudioImporter` | `Audio/Ambience` |
| `UIAudioImporter` | `Audio/UI` |

## Step 4: Locate the preset assets

All preset assets are in the package:
```
Packages/Unity Best Practices/Editor/Presets/Textures/
Packages/Unity Best Practices/Editor/Presets/Audio/
```

Drag them directly from the Project window into the Preset Manager fields.

## Step 5: Create your folder structure

If you used **Window > Best Practices > Setup Project Folders**, your `_Project/` folder contains `Art/` and `Audio/`. Create subfolders matching the filters above inside those.

## Notes

- The folder filter is a partial path match — `Art/Textures/Albedo` will match `Assets/_Project/Art/Textures/Albedo/`
- Presets are applied when assets are first imported. Re-import existing assets to apply the preset retroactively (right-click asset > Reimport)
- You can override a preset on individual assets by selecting the asset and clicking **Preset** in the Inspector
