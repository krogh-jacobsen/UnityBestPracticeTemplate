## UI Sprites ##
UI textures are used frequently, UI artifacts are very noticeable but the files are usually small in terms of size. The performance bottlenecks you want to look out for in UI are rarely texture memory but usually related batching and overdraw. So in terms of optimization it’s more important that you prioritize visual quality and over aggressive compression.
Here are some generally recommended settings to textures in /Textures/UI/:

| Setting                | Value | Notes |
|------------------------|--------|--------|
| **Texture Type**       | Sprite (2D and UI) | Ensures correct sprite handling and UI batching |
| **Sprite Mode**        |  |  |
| Sprite Mode            | Single / Multiple | Depending on if you are using spritesheets |
| Mesh Type              | Full Rect | Predictable quad geometry results in more stable and efficient UI batching. Avoid Tight, which generates extra vertices. |
| Extrude Edges          | 0 | UI sprites do not require extrusion padding. |
| Pivot                  | Default | UI elements often rely on specific pivots and layout logic, so it’s best to leave as defined by the asset (do not standardize in preset). See note below on Partial Presets and how to skip it. |
| Generate Physics Shape | OFF | UI sprites rarely use physics and generating physics shapes is just overhead. |
| **Advanced**           |  |  |
| sRGB (Color Texture)   | ON | UI textures represent color data and should be gamma-corrected. |
| Alpha Source           | Input Texture Alpha | Assuming your UI sprites rely on transparency and your texture file (PNG, PSD, etc.) should use the contained alpha channel for UI. |
| Alpha Is Transparency  | ON | Improves edge filtering and reduces halo artifacts around transparent UI elements. |
| Read/Write             | OFF | Leave Read/Write OFF unless you explicitly need CPU pixel access to modify textures at runtime. Avoids doubling memory footprint. |
| Mip Maps               | OFF | UI is rendered in screen space; mipmaps waste memory and introduce blur. |
| Wrap Mode              | Clamp | Prevents edge bleeding artifacts when scaling or using sliced sprites. |
| Filter Mode            | Bilinear | Prevents pixel crawl on scaled UI. Use Point only for pixel-art UI. |
| **Compression**        |  |  |
| Max Size               | 1024–2048 (project dependent) | Prevents accidental oversized UI textures. Cap aggressively. |
| Resize Algorithm       | Mitchell | Mitchell provides a good balance between sharpness and smoothness and is a safe general-purpose default. |
| Format                 | Automatic (see platform notes below) | Format should differ per platform. See platform guidance below. |
| Compression            | None or Low Quality | UI compression artifacts (especially banding and alpha issues) are highly visible. Favor visual clarity. |
| Use Crunch Compression | Option disabled | Disabled with the above settings. |

**Platform Differences — UI:**
| Platform | Format | Compression | Notes |
|----------|--------|-------------|-------|
| **PC / Console** | RGBA32 (uncompressed) | None | Memory is less constrained. Uncompressed preserves perfect alpha and avoids all block artifacts. |
| **Mobile (Android/iOS)** | ASTC 4x4 | Low Quality | RGBA32 is too expensive on mobile — a single 2048 UI atlas at RGBA32 is 16 MB. ASTC 4x4 is visually near-lossless for UI and 4–8x smaller. Only fall back to ETC2 on devices that don't support ASTC (rare on modern hardware). |

Remember you have the option to use Partial Presets. You don't have to apply every setting. In the Preset Inspector, you can right-click specific properties and select Exclude Property. This allows you to create "surgical" presets that only enforce specific settings (like turning off "Raycast Target" on UI components) without overwriting everything else or in our example here you exclude the pivot so the control is with the designer.

---

## Albedo / World Textures ##
World textures (albedo/diffuse) are the color maps applied to 3D surfaces in your scene. Unlike UI textures, these are viewed at varying distances and oblique angles, so mipmaps and anisotropic filtering are essential for visual quality. Compression can be more aggressive here than with UI since in-world artifacts are far less noticeable than on flat screen-space elements.
Here are some generally recommended settings to textures in /Textures/Environment/Albedo/:

| Setting                | Value | Notes |
|------------------------|--------|--------|
| **Texture Type**       | Default | Standard texture import for 3D surface use. |
| **Advanced**           |  |  |
| sRGB (Color Texture)   | ON | Albedo maps represent color data and must be gamma-corrected for correct rendering in linear color space. |
| Mip Maps               | ON | Required for proper distance rendering. Without mipmaps, distant surfaces will shimmer and alias. |
| Mip Map Filtering      | Box or Kaiser | Reduces shimmering and moire patterns at distance. Kaiser produces slightly sharper results; Box is a safe default. |
| Wrap Mode              | Repeat | Most tiling materials (floors, walls, terrain) require seamless repetition. Use Clamp only for unique, non-tiling textures. |
| Filter Mode            | Bilinear | Standard filtering for most use cases. Trilinear smooths visible mipmap band transitions but has a GPU cost — see platform notes below. |
| Aniso Level            | See platform notes | Improves clarity on surfaces viewed at oblique angles (floors, roads, walls). Value should differ by platform. |
| **Compression**        |  |  |
| Max Size               | See platform notes | Prevents accidental 8K texture imports. Value should differ by platform. |
| Compression            | Normal Quality | Uses platform-specific formats automatically. Balances quality and memory. |

**Platform Differences — Albedo:**
| Setting | PC / Console | Mobile |
|---------|-------------|--------|
| **Aniso Level** | 4–8 (GPU headroom allows it; significantly improves floors, roads, walls at oblique angles) | 1–2 (anisotropic filtering is expensive on mobile GPUs; keep low or disabled) |
| **Filter Mode** | Bilinear (Trilinear acceptable where mip banding is visible) | Bilinear only (Trilinear has a measurable GPU cost on mobile) |
| **Max Size** | 2048 default, 4096 for hero assets | 1024 default, 2048 only for hero assets |
| **Compression Format** | BC7 (high quality, 4 channel support) | ASTC 4x4–6x6 (preferred), ETC2 as fallback on older devices. ASTC handles alpha better than ETC2. |

Mipmaps are mandatory for 3D world textures to prevent aliasing. A world texture without mipmaps will shimmer aggressively at any distance beyond close-up, especially on surfaces like floors and walls viewed at grazing angles.

---

## Normal Maps ##
Normal maps encode surface detail as directional data in tangent space. They are not color data — they are mathematical vectors packed into texture channels. Importing them incorrectly (wrong type or wrong color space) will produce visibly broken lighting across your entire scene.
Here are some generally recommended settings to textures in /Textures/Environment/Normal/:

| Setting                | Value | Notes |
|------------------------|--------|--------|
| **Texture Type**       | Normal Map | Critical — tells Unity to treat the texture as tangent-space normal data. Enables correct swizzling and platform-specific encoding. |
| **Advanced**           |  |  |
| sRGB (Color Texture)   | OFF (auto-set) | Normal maps are linear data. Unity automatically forces sRGB OFF when Texture Type is set to Normal Map. Documented here to explain why — applying gamma correction distorts the encoded directions and produces incorrect lighting. |
| Mip Maps               | ON | Prevents lighting shimmer and specular aliasing at distance. Normal maps without mipmaps cause noisy, flickering highlights. |
| Wrap Mode              | Repeat | Tiling normal maps must repeat seamlessly to match their corresponding albedo textures. |
| Filter Mode            | Bilinear | Standard filtering. Trilinear can help smooth mipmap transitions on normals but has a GPU cost on mobile. |
| **Compression**        |  |  |
| Max Size               | See platform notes | Normal maps can often be one step smaller than their corresponding albedo without visible loss. |
| Compression            | Normal Quality | Uses dedicated normal map formats when Texture Type is set to Normal Map. See platform notes for specifics. |

**Platform Differences — Normal Maps:**
| Setting | PC / Console | Mobile |
|---------|-------------|--------|
| **Max Size** | 2048 default (match or halve albedo size) | 1024 default (can often be half of albedo without visible loss) |
| **Compression Format** | BC5 (two-channel RG format, ideal for tangent-space normals) | ASTC 5x5 or 6x6 (normals tolerate slightly more aggressive block sizes than albedo; still visually correct). ETC2 as fallback — note ETC2 is RGB-based and less optimal for two-channel normal data. |

Incorrect import (sRGB ON or Default texture type) will produce visibly wrong lighting. Surfaces will appear flat, overly shiny, or have strange specular artifacts. If lighting looks "off" on an otherwise correct material, check the normal map import settings first.

---

## Masks (Metallic / Roughness / AO / Packed Maps) ##
Mask textures store per-pixel material properties — metallic, roughness (smoothness), ambient occlusion, height, or combinations packed into RGBA channels. Like normal maps, these are data textures, not color. The shader reads exact numeric values from each channel, so any color space conversion or heavy compression will directly corrupt your material response.
Here are some generally recommended settings to textures in /Textures/Environment/Masks/:

| Setting                | Value | Notes |
|------------------------|--------|--------|
| **Texture Type**       | Default | Standard texture import. Unity does not have a dedicated "Mask" type. |
| **Advanced**           |  |  |
| sRGB (Color Texture)   | OFF | Masks are linear data. Leaving sRGB ON applies gamma correction, which shifts the numeric values the shader reads and breaks shading accuracy (e.g., metallic boundaries shift, roughness response changes). |
| Mip Maps               | ON | Maintains correct material response at distance. Without mipmaps, roughness and metallic transitions will alias. |
| Single Channel         | ON (if applicable) | When a mask uses only one channel (e.g., a standalone roughness or AO map), enabling this reduces memory by storing only the single channel instead of full RGBA. Not applicable for packed maps using multiple channels. |
| Wrap Mode              | Repeat | Should match the wrap mode of the corresponding albedo and normal maps. |
| Filter Mode            | Bilinear | Standard filtering for data textures. |
| **Compression**        |  |  |
| Max Size               | See platform notes | Mask textures can often be smaller than albedo without visible loss, since they encode smooth gradients rather than fine detail. |
| Compression            | Normal Quality | Avoid very low quality compression — banding artifacts show easily in mask data, especially in smoothness/roughness channels where gradients are critical to specular response. |

**Platform Differences — Masks:**
| Setting | PC / Console | Mobile |
|---------|-------------|--------|
| **Max Size** | 1024–2048 (can often be half of albedo) | 512–1024 (smooth gradients compress well at lower resolutions) |
| **Compression Format** | BC7 (high quality, preserves all four channels for packed maps) | ASTC 4x4–5x5 (keep block size modest to avoid banding in smoothness/roughness gradients). Use higher quality settings than you would for albedo — banding in mask channels produces visible specular artifacts. |

Mask textures are data, not color. Leaving sRGB ON is the single most common mistake with mask imports and will break shading accuracy across every material using that texture.

---

## How to Create and Apply a Preset ##
This section walks through how to turn a configured asset's import settings into a reusable Preset, and how to assign it in the Preset Manager so Unity applies it automatically on import.

**Creating a Preset from a configured asset:**
1. Select an asset that already has the import settings you want to standardize.
2. In the Inspector, click the **Preset icon** (sliders) in the top-right corner, next to the question mark.
3. Select **Save current to new preset**. For folder organization, place Presets in `/Settings/Presets/` but any folder works.
4. Give it a clear name that communicates intent and separates it from other presets (e.g., `MusicAudioImporter`, `SFXAudioImporter`, `UITextureImporter`).

**Assigning a Preset in the Preset Manager:**
1. Navigate to **Edit > Project Settings > Preset Manager**.
2. Add your newly created Preset under the matching importer type (e.g., AudioImporter, TextureImporter, ModelImporter).
3. Define a **filter** to control which assets the preset applies to. Use glob syntax to match by folder or file pattern.

**Glob Filter Syntax:**
The filter field uses `glob:"pattern"` syntax. The field must begin with `glob:` and the pattern must be in quotation marks.

| Pattern | Matches |
|---------|---------|
| `glob:"Assets/Art/Audio/Music/**"` | Everything under the Music folder, including all subfolders. |
| `glob:"Assets/Art/Audio/Music/*"` | Only items directly inside Music/, not nested subfolders. |
| `glob:"Assets/Art/Textures/UI/*.png"` | Only PNG files inside the UI folder. |
| `glob:"**/*SpriteSheet*"` | Any file with "SpriteSheet" in its name, regardless of folder. |

Unity applies default presets **top-to-bottom** in the Preset Manager list. Later entries override earlier ones if multiple filters match the same asset. Order your presets from most general (top) to most specific (bottom).

**Testing your Preset:**
Import a new asset into the target folder. Once import completes, it should have all the preset settings applied automatically.

Default Presets in the Preset Manager are applied **during import**, not retroactively to existing assets or when moving an already-imported asset between folders. To apply presets to pre-existing assets:
- **Option A:** Right-click the asset (or use the three-dot menu) and choose **Reset**. This triggers a reimport using the current Preset Manager rules.
- **Option B:** Delete the `.meta` files for the target assets (with Unity closed). Unity will re-import them on next startup, applying any matching presets. This is faster for bulk operations across hundreds of files.

---

## Audio: Music ##
Music clips are long (often minutes), large in file size, and don't require instant playback. Players won't notice a minimal delay before a track starts. The priority is keeping runtime memory low while preserving enough fidelity that compression artifacts aren't audible — music is the category where listeners are most sensitive to quality loss.
Here are some generally recommended settings for audio clips in /Audio/Music/:

| Setting                | Value | Notes |
|------------------------|--------|--------|
| **Force To Mono**      | OFF | Keep stereo separation for music. Forcing to mono will collapse the stereo mix and break the spatial feel, especially noticeable on headphones. |
| **Load In Background** | ON | Prevents frame-rate hiccups when the clip starts loading. Music clips are large enough that synchronous loading can cause a visible stutter. |
| **Ambisonic**          | OFF | Only needed for 360° spatial soundfield clips (VR ambisonic recordings). Standard music tracks are stereo. |
| **Load Type**          | Streaming | Streams directly from disk during playback, keeping runtime memory footprint minimal. A streaming music clip uses only a small buffer regardless of total file size. This is the correct choice for any clip longer than ~10 seconds. |
| **Preload Audio Data** | OFF | Not needed for streaming clips. Preloading a large music file would defeat the purpose of streaming by loading it into memory upfront. |
| **Compression Format** | Vorbis | Lossy compression with good quality-to-size ratio. The standard choice for music and ambience in games. |
| **Quality**            | 70–85% | This is subjective — test what works for your content. Higher preserves fidelity, lower saves space. 100% quality is almost always wasted space; the difference between 85% and 100% is inaudible to most listeners but the file size difference is significant. |

**Platform Differences — Music:**
| Setting | PC / Console | Mobile |
|---------|-------------|--------|
| **Quality** | 80–85% (more storage headroom, players more likely on speakers/headphones where quality matters) | 70–80% (storage is constrained; mobile speakers and Bluetooth audio mask compression artifacts) |
| **Sample Rate Setting** | Preserve Sample Rate (keep original quality) | Optimize Sample Rate (lets Unity downsample if beneficial; reduces file size) |

**Preset Manager filter example:** `glob:"Assets/Art/Audio/Music/**"`

---

## Audio: Ambience ##
Ambience shares most characteristics with music — long, continuous clips that don't require instant startup. The key difference is that ambience is more tolerant of compression. Background environmental audio (wind, rain, crowd noise, room tone) naturally contains noise and texture that masks compression artifacts, so you can compress more aggressively than music. Projects often have significantly more ambience clips than music tracks, so the compression savings add up.

If your project is small, you could use a single preset for both music and ambience. But for larger projects, the compression difference across many files makes separate presets worthwhile.

Here are some generally recommended settings for audio clips in /Audio/Ambience/:

| Setting                | Value | Notes |
|------------------------|--------|--------|
| **Force To Mono**      | OFF | Keep stereo for environmental ambience. Stereo separation is critical for immersive environmental soundscapes (rain panning, wind direction, crowd presence). |
| **Load In Background** | ON | Same reason as music — prevents frame-rate hiccups on large clip loads. |
| **Ambisonic**          | OFF | Only enable for dedicated 360° ambisonic recordings (VR spatial audio). Standard stereo ambience does not use this. |
| **Load Type**          | Streaming | Same approach as music — streams from disk to keep memory minimal. Ambience clips are often long and may loop indefinitely, making streaming the only practical option. |
| **Preload Audio Data** | OFF | Not needed for streaming clips. |
| **Compression Format** | Vorbis | Same format as music. Consistent pipeline, good quality-to-size ratio. |
| **Quality**            | 60–70% | About 10–15% lower than music. Ambient noise (wind, rain, room tone) hides compression artifacts naturally. Test to confirm the quality loss is acceptable for your content. |

**Platform Differences — Ambience:**
| Setting | PC / Console | Mobile |
|---------|-------------|--------|
| **Quality** | 65–70% (slightly more headroom) | 60–65% (ambient layers add up quickly; compress aggressively) |
| **Sample Rate Setting** | Preserve Sample Rate | Optimize Sample Rate (mobile benefits from smaller file sizes) |

**Preset Manager filter example:** `glob:"Assets/Art/Audio/Ambience/**"`

---

## Audio: SFX ##
Sound effects are short clips (typically under 5 seconds) where latency is the enemy. A delayed gunshot, footstep, or impact sound breaks gameplay feel immediately. The approach is the opposite of music/ambience: load the full decoded clip into memory upfront so playback is instant. Because the clips are short, the memory cost of decompressing into memory is negligible compared to the gameplay benefit of zero-latency playback.
Here are some generally recommended settings for audio clips in /Audio/SFX/:

| Setting                | Value | Notes |
|------------------------|--------|--------|
| **Force To Mono**      | OFF | Keep stereo for SFX by default. Many sound effects are designed with stereo width (explosions, whooshes, impacts). If your SFX are all mono recordings or you exclusively use 3D spatialization (which collapses to mono at the AudioSource), you can enable this to save memory. |
| **Load In Background** | OFF | SFX should be available immediately. Loading in background could cause a missed sound on first trigger. |
| **Ambisonic**          | OFF | Not applicable for standard sound effects. |
| **Load Type**          | Decompress On Load | Decodes the full clip into uncompressed PCM in memory at load time. Fastest possible playback with no runtime decompression cost. Memory cost is acceptable because these clips are short. |
| **Preload Audio Data** | ON | Guarantees no hitch on the very first playback. The clip is fully decoded and ready before it's ever triggered. |
| **Compression Format** | Vorbis | Still provides good compression for the on-disk/build size. The clip is decompressed at load time, so the runtime format is uncompressed PCM regardless — this setting only affects build size. |
| **Quality**            | 80–90% | Short, sharp sounds expose compression artifacts more than ambient audio. Transients (clicks, impacts, gunshots) suffer noticeably at low quality. Keep quality high since the clips are small anyway. |

**Platform Differences — SFX:**
| Setting | PC / Console | Mobile |
|---------|-------------|--------|
| **Quality** | 85–90% (transient clarity matters) | 80–85% (slightly more aggressive; mobile speakers are less revealing) |
| **Sample Rate Setting** | Preserve Sample Rate | Optimize Sample Rate |
| **Force To Mono** | OFF (keep stereo design) | Consider ON for non-spatialized SFX (saves memory; mobile budgets are tighter) |

For clips longer than ~5 seconds that aren't ambience (e.g., a long mechanical sequence, dialogue), consider using Compressed In Memory instead of Decompress On Load to avoid large memory spikes.

**Preset Manager filter example:** `glob:"Assets/Art/Audio/SFX/**"`

---

## Audio: UI Sounds ##
UI sounds share the same latency requirements as SFX — a delayed button click or menu transition sound feels like a bug. The difference is that UI sounds are almost never spatial. They play in screen space, not world space, so stereo separation adds no value and mono cuts memory usage in half. UI sounds are also typically the shortest and simplest clips in a project, so you can afford higher quality or even no compression at all.
Here are some generally recommended settings for audio clips in /Audio/UI/:

| Setting                | Value | Notes |
|------------------------|--------|--------|
| **Force To Mono**      | ON | UI sounds are not spatial — they don't exist in 3D space. Mono cuts memory in half with no audible difference for clicks, hovers, transitions, and notifications. |
| **Load In Background** | OFF | UI sounds must be available instantly. A menu that loads silently on first interaction feels broken. |
| **Ambisonic**          | OFF | Not applicable for UI audio. |
| **Load Type**          | Decompress On Load | Same as SFX — fastest possible playback. UI clips are tiny so the memory cost is negligible. |
| **Preload Audio Data** | ON | Guarantees no hitch on the very first click. Critical for UI responsiveness. |
| **Compression Format** | PCM (no compression) | UI clips are so small that compression savings are negligible (a 0.2 second mono click is ~17 KB uncompressed). PCM eliminates any chance of compression artifacts on short, clean sounds. You can also use Vorbis at 90%+ if build size is a concern, but the savings on tiny clips are minimal. |
| **Quality**            | N/A (PCM) | Not applicable when using PCM. If using Vorbis, set to 90%+ — artifacts on short, clean UI sounds are very noticeable. |

**Platform Differences — UI Sounds:**
| Setting | PC / Console | Mobile |
|---------|-------------|--------|
| **Compression Format** | PCM (clips are tiny; no reason to compress) | PCM (same — UI clips are so small that compression savings don't matter even on mobile) |
| **Sample Rate Setting** | Preserve Sample Rate | Optimize Sample Rate (marginal savings, but consistent with other mobile audio settings) |

**Preset Manager filter example:** `glob:"Assets/Art/Audio/UI/**"`

---

## Audio: Quick Comparison ##
A side-by-side summary of how the four audio presets differ and why:

| Setting | Music | Ambience | SFX | UI |
|---------|-------|----------|-----|-----|
| **Force To Mono** | OFF | OFF | OFF (consider ON on mobile) | ON |
| **Load Type** | Streaming | Streaming | Decompress On Load | Decompress On Load |
| **Preload Audio Data** | OFF | OFF | ON | ON |
| **Load In Background** | ON | ON | OFF | OFF |
| **Compression** | Vorbis 70–85% | Vorbis 60–70% | Vorbis 80–90% | PCM |
| **Why** | Long, large, fidelity matters | Long, large, tolerates more compression | Short, latency-critical, sharp transients | Tiny, latency-critical, no spatial need |

The core split is simple: **long clips stream, short clips decompress into memory**. Within each group, the compression quality reflects how forgiving the content is — ambient noise hides artifacts, transients and clean UI sounds expose them.

---

## 3D Models (FBX) ##
Model import settings have a large impact on visual correctness, memory, and runtime performance. A bad default (wrong scale, missing tangents, unnecessary Read/Write) can silently break lighting, inflate memory, or cause physics issues across every mesh in the project. Presets are especially valuable here because model import has many tabs and settings that are easy to misconfigure.
Here are some generally recommended settings for models in /Models/:

**Model Tab:**

| Setting                | Value | Notes |
|------------------------|--------|--------|
| **Scale Factor**       | 1.0 (Blender) | Blender's FBX exporter uses meters by default, matching Unity's unit scale. For **Maya/3ds Max**, use 0.01 — these tools work in centimeters, so the scale factor converts cm to Unity's meters. |
| **Convert Units**      | ON | Lets Unity apply the FBX file's embedded unit scale. Works correctly with Blender's default export. If you see models at 100x size, this setting or Scale Factor is wrong. |
| **Import BlendShapes** | OFF (unless needed) | BlendShapes add memory and processing cost. Only enable on models that actually use them (facial animation, morph targets). |
| **Import Visibility**  | OFF | Blender visibility states rarely map correctly to Unity. Control visibility through Unity's own systems. |
| **Import Cameras**     | OFF | Cameras embedded in FBX are almost never wanted. Use Cinemachine instead. |
| **Import Lights**      | OFF | Light data from DCC tools rarely transfers correctly. Light your scenes in Unity. |
| **Mesh Compression**   | Off | Mesh compression can introduce vertex snapping artifacts, especially on smooth surfaces and small props. The memory savings are minimal compared to texture compression. Only enable for very high-poly background meshes where minor artifacts are invisible. |
| **Read/Write**         | OFF | Halves mesh memory by not keeping a CPU copy. Only enable if you need runtime mesh access (procedural deformation, mesh collider baking at runtime). |
| **Optimize Mesh**      | Everything | Reorders vertices and indices for GPU cache efficiency. No visual change, free performance. |
| **Generate Colliders** | OFF | Do not auto-generate mesh colliders on every imported model. Add colliders intentionally — most objects should use primitive colliders (Box, Sphere, Capsule) for performance. Mesh colliders are expensive and should be reserved for complex static geometry. |

**Normals & Tangents:**

| Setting                | Value | Notes |
|------------------------|--------|--------|
| **Normals**            | Import | Use the normals authored in your DCC tool. Only use Calculate if the source file has broken normals. None strips normals entirely (only for unlit meshes). |
| **Tangents**           | Calculate Mikktspace | Industry standard for PBR normal mapping. Matches Blender's default tangent space. Using a different tangent basis will produce subtle but incorrect normal map lighting. |

**Materials Tab:**

| Setting                | Value | Notes |
|------------------------|--------|--------|
| **Material Creation Mode** | None | Do not let Unity auto-create materials from FBX files. Extracted/auto-generated materials are hard to manage and usually wrong. Create and assign materials manually in Unity using your own shaders and textures. |

**Rig Tab (for static/prop models):**

| Setting                | Value | Notes |
|------------------------|--------|--------|
| **Animation Type**     | None | Static props and environment models don't need a rig. Setting this to None avoids unnecessary skinning overhead. For character models, create a separate preset with Animation Type set to Humanoid or Generic as needed. |

**Animation Tab:**

| Setting                | Value | Notes |
|------------------------|--------|--------|
| **Import Animation**   | OFF | For a general-purpose static model preset, disable animation import. For models with embedded animations, use a separate preset with this enabled and configure clip extraction, compression (Optimal), and keyframe reduction. |

Consider creating multiple model presets: one for static props/environment (Animation Type: None, Import Animation: OFF), one for characters (Animation Type: Humanoid, Rig configured), and one for animated props (Animation Type: Generic).

---

## Animation Clips ##
Animation compression and keyframe reduction are the primary levers for controlling animation memory and file size. Uncompressed animations can be surprisingly large — a single character's animation set can easily consume more memory than all of its textures combined if left at default settings. The goal is aggressive compression with error thresholds low enough that artifacts aren't visible during gameplay.
Here are some generally recommended settings for animation clips:

| Setting                | Value | Notes |
|------------------------|--------|--------|
| **Anim. Compression**  | Optimal | Unity selects the best compression per-curve. Keyframe Reduction only reduces keys; Optimal also quantizes values. The memory savings over Keyframe Reduction are significant with no visible quality loss in most cases. |
| **Rotation Error**     | 0.5 | How much rotation (in degrees) can deviate from the source before a keyframe is preserved. 0.5 is a good default — tight enough for character animation, loose enough for meaningful compression. Tighten to 0.1–0.2 for facial animation or precise hand movement. |
| **Position Error**     | 0.5 | How much position (in percent of range) can deviate. 0.5 works for most gameplay animation. Tighten for root motion or precise world-space movement. |
| **Scale Error**        | 0.5 | How much scale can deviate. Most animations don't animate scale, so this rarely matters. If you use scale animation (squash/stretch), tighten to 0.1–0.2. |
| **Animated Custom Properties** | OFF (unless needed) | Only enable if your animations drive custom material properties or script values via animation curves. Adds overhead. |
| **Resample Curves**    | ON | Resamples curves to match Unity's fixed timestep. Disabling can cause interpolation issues. Leave ON unless you have specific reasons (e.g., animations authored at exact frame rates that must be preserved). |

**Platform Differences — Animation:**
| Setting | PC / Console | Mobile |
|---------|-------------|--------|
| **Anim. Compression** | Optimal (same on all platforms) | Optimal (same — no platform-specific format) |
| **Error Thresholds** | 0.5 default (tighten for cinematics) | 0.5–1.0 (can afford slightly more aggressive reduction on mobile to save memory) |

Animation compression is not platform-specific in the same way texture compression is — the same compressed clip plays identically everywhere. The main mobile concern is total animation memory budget, so consider slightly higher error thresholds or fewer total clips rather than different compression formats.

---

## Video Clips ##
Video files are often the single largest assets in a build. A single uncompressed or poorly configured video can exceed the total size of all textures combined. Unity's video importer transcodes source files into platform-appropriate formats, but the defaults are not aggressive enough for most projects. Presets prevent accidental multi-gigabyte builds from a handful of cutscenes.
Here are some generally recommended settings for video in /Video/:

| Setting                | Value | Notes |
|------------------------|--------|--------|
| **Transcode**          | ON | Converts source video into a platform-optimized format at import. Without transcoding, Unity uses the source file as-is, which may not be playable on all platforms and is usually larger than necessary. |
| **Codec**              | H.264 | Broadest hardware decoder support across all platforms. H.265 offers better compression but has inconsistent hardware support, especially on older Android devices and WebGL. Use H.265 only if you've verified target platform support. |
| **Bitrate Mode**       | Low (or Medium for cutscenes) | Controls the quality/size tradeoff. Low is fine for UI backgrounds and tutorials. Medium for narrative cutscenes. Avoid High unless you're shipping a cinematic-quality product and have the budget for large builds. |
| **Spatial Quality**    | Low Quality (or Medium for cutscenes) | Affects spatial detail. Same tradeoff as bitrate — lower is smaller. UI/tutorial clips tolerate low quality. Full-screen cutscenes may need Medium. |
| **Import Audio**       | ON (if video has audio) | Strip audio tracks from videos that don't need them (looping backgrounds, UI animations). Keeping unnecessary audio wastes space. For videos with dialogue or SFX, leave ON and control audio through Unity's audio system. |
| **Keep Alpha**         | OFF (unless needed) | Alpha channel video is extremely expensive in file size. Only enable for transparent overlay videos (rare). Most videos are opaque. |
| **Deinterlace**        | OFF | Only relevant for interlaced source material (broadcast/TV footage). Game assets are almost always progressive. |
| **Flip Horizontally/Vertically** | OFF | Only needed to correct source material orientation issues. |

**Platform Differences — Video:**
| Setting | PC / Console | Mobile |
|---------|-------------|--------|
| **Codec** | H.264 (H.265 acceptable if targeting modern hardware) | H.264 (safest — H.265 hardware decoding is inconsistent on Android; fine on iOS) |
| **Max Resolution** | 1920x1080 for cutscenes, 1280x720 for UI/tutorial | 1280x720 max for cutscenes, 854x480 for UI/tutorial. Mobile screens are smaller — high resolution is wasted. |
| **Bitrate** | Medium–High for cutscenes | Low–Medium (bandwidth and storage are constrained) |

Consider creating two video presets: one for full-screen cutscenes (higher quality, larger sizes) and one for UI/tutorial clips (aggressive compression, smaller resolution). The size difference can be enormous — a 1080p Medium cutscene at 2 minutes can be 30–50 MB, while the same clip at 480p Low can be 3–5 MB.

---

## Fonts (TrueType / OpenType) ##
Font import settings control how Unity rasterizes TTF/OTF files into font textures. The main risks are oversized atlas textures from including unnecessary character sets, and blurry text from incorrect rendering modes. For projects using TextMeshPro, the font import preset covers the source font file — TMP Font Asset generation has its own settings configured through the TMP Font Asset Creator window.
Here are some generally recommended settings for fonts in /Fonts/:

| Setting                | Value | Notes |
|------------------------|--------|--------|
| **Font Size**          | 32–48 (project dependent) | The reference rasterization size. Larger values produce sharper glyphs but larger atlas textures. 32 is sufficient for body text; 48 for headers or if you display text at large screen sizes. |
| **Rendering Mode**     | Smooth | Anti-aliased glyph rendering. Use Hinted Smooth for small body text where hinting improves readability at low sizes. Use OS Default only if you need platform-native rendering behavior. |
| **Character**          | Dynamic | Rasterizes glyphs on demand at runtime. Avoids massive atlas textures from pre-baking every character. Use Unicode or ASCII only if you need guaranteed upfront availability of specific character sets (e.g., localization with known character ranges). |
| **Include Font Data**  | ON | Embeds the font file in the build. Required for Dynamic character mode to rasterize new glyphs at runtime. Only disable if using a fully pre-baked static character set. |
| **Font Names**         | (leave empty) | Fallback font names for platform font matching. Only relevant if you're referencing system fonts, which is uncommon for game projects. |

**TextMeshPro Font Asset Notes:**
TMP font assets are not created through the standard font importer — they're generated via **Window > TextMeshPro > Font Asset Creator**. Key settings to standardize:

| Setting                | Recommended Value | Notes |
|------------------------|--------|--------|
| **Sampling Point Size** | 42–64 | The SDF sampling size. Higher values produce sharper distance fields. 42 is TMP's default and works well for most cases. Use 64 for text displayed at very large sizes. |
| **Padding**            | 5–7 | Space around each glyph in the atlas for SDF gradient. 5 is minimum for clean rendering; 7 gives more room for effects (outlines, shadows). |
| **Atlas Resolution**   | 2048x2048 (or 1024x1024 for limited character sets) | Larger atlases fit more characters but consume more memory. 1024x1024 is usually sufficient for Latin character sets. CJK localization may need 4096x4096 or multi-atlas. |
| **Render Mode**        | SDFAA | Signed Distance Field with anti-aliasing. The standard mode for TMP. Use SDF for slightly smaller atlases if you don't need AA. Avoid Raster modes unless you specifically need pixel-perfect bitmap text. |
| **Character Set**      | ASCII or Custom Range | Only include characters you actually use. "Extended ASCII" or full Unicode sets generate massive atlases. For localization, generate separate font assets per language with targeted character ranges. |

**Platform Differences — Fonts:**
| Setting | PC / Console | Mobile |
|---------|-------------|--------|
| **TMP Atlas Resolution** | 2048x2048 (4096 for CJK) | 1024x1024 (2048 for CJK) — atlas memory adds up on mobile, especially with multiple fonts |
| **Font Size / Sampling** | 48–64 (higher res screens benefit from sharper glyphs) | 32–42 (mobile screens are smaller; diminishing returns above 42) |

---

## Important Cross-Category Notes ##

### Platform Overrides Matter
Unity allows per-platform texture overrides in the import settings. Use them. Each section above includes a platform differences table — this is a summary.

| Setting | PC / Console | Mobile (Android/iOS) |
|---------|-------------|----------------------|
| **Max Size** | 2048 default, 4096 for hero assets | 1024 default, 2048 for hero assets only |
| **Albedo Format** | BC7 | ASTC 4x4–6x6 (preferred), ETC2 fallback |
| **Normal Format** | BC5 (two-channel) | ASTC 5x5–6x6, ETC2 fallback |
| **Mask Format** | BC7 | ASTC 4x4–5x5 |
| **UI Format** | RGBA32 (uncompressed) | ASTC 4x4 (near-lossless, much smaller) |
| **Aniso Level** | 4–8 | 1–2 |
| **Filter Mode** | Bilinear/Trilinear | Bilinear only |
| **Video Codec** | H.264 (H.265 where supported) | H.264 only |
| **Video Max Res** | 1920x1080 cutscenes, 1280x720 UI | 1280x720 cutscenes, 854x480 UI |
| **TMP Atlas** | 2048x2048 | 1024x1024 |
| **Music Quality** | Vorbis 80–85% | Vorbis 70–80% |
| **Ambience Quality** | Vorbis 65–70% | Vorbis 60–65% |
| **SFX Quality** | Vorbis 85–90% | Vorbis 80–85% |
| **Audio Sample Rate** | Preserve Sample Rate | Optimize Sample Rate |

### Mipmaps Rule
The simplest rule to remember for mipmap settings across all texture categories:

| Category | Mipmaps |
|----------|---------|
| UI Textures | OFF |
| World Textures (Albedo) | ON |
| Normal Maps | ON |
| Mask Textures | ON |

If you remember only one rule from this document, remember this one. Incorrect mipmap settings are the most common cause of both wasted memory (UI) and visual artifacts (world textures).

### Don't Overcompress UI
UI compression artifacts are far more noticeable than in-world texture compression. A block compression artifact on a floor tile is invisible at gameplay distance, but the same artifact on a menu button is immediately obvious. Favor quality (RGBA32 or low compression) for UI, and use standard compression for everything else.

### Limit Max Size Globally
The biggest production mistake is accidental 8K textures making it into builds. A single uncontrolled texture can consume more memory than dozens of properly sized ones. Always cap Max Size in presets — it's easier to selectively increase size for hero assets than to hunt down oversized textures later.
