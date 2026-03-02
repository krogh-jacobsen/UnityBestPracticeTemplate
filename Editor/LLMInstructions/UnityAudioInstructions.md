
# GitHub Copilot Instructions: Unity Audio

Use this cheat sheet for LLM completions. See the readme.md for the general rationale behind these guidelines.

Table of contents:
- [Unity Version-Specific Instructions](#unity-version-specific-instructions)
- [AudioMixer and AudioMixerGroup](#audiomixer-and-audiomixergroup)
- [AudioMixerSnapshot Transitions](#audiomixersnapshot-transitions)
- [Volume Control via Exposed Parameters](#volume-control-via-exposed-parameters)
- [AudioSource Pooling](#audiosource-pooling)
- [Avoid PlayClipAtPoint for Frequent SFX](#avoid-playclipatpoint-for-frequent-sfx)
- [AudioClip Memory Management](#audioclip-memory-management)
- [Spatial Audio](#spatial-audio)
- [AudioListener](#audiolistener)
- [Pooled Audio Service Pattern](#pooled-audio-service-pattern)

# Unity Version-Specific Instructions

- ℹ️ This project uses Unity 6.3. The Audio system is unchanged from Unity 2022 LTS, but Unity 6 adds the **Unity Audio Random Container** for randomised SFX — prefer it over manual randomisation.
- ℹ️ Unity 6 deprecates the FMOD-based audio backend in favour of the native Unity Audio backend — ensure project settings use the native backend.
- ℹ️ Use `AudioSource.outputAudioMixerGroup` to route all audio through the mixer — direct volume control on `AudioSource.volume` bypasses the mixer hierarchy.
- ℹ️ `AudioMixer.SetFloat` parameter names must match **Exposed Parameters** names in the Audio Mixer window exactly (case-sensitive).

# AudioMixer and AudioMixerGroup

- ✅ Create a single `AudioMixer` asset for the project with a hierarchical group structure.
- ✅ Route every `AudioSource` to an `AudioMixerGroup` via `AudioSource.outputAudioMixerGroup`.
- ✅ Typical group hierarchy: `Master` > `Music`, `SFX`, `Voice`, `Ambience` — `SFX` > `Footsteps`, `Weapons`, `UI`.
- ❌ Never set `AudioSource.volume` directly for player-controlled volume — use mixer parameters.
- ❌ Never create multiple `AudioMixer` assets for a single game — one mixer with multiple groups is correct.

```csharp
public class AudioSourceSetup : MonoBehaviour
{
    [SerializeField] private AudioMixerGroup m_SFXGroup;

    private AudioSource m_Source;

    private void Awake()
    {
        m_Source = GetComponent<AudioSource>();

        // Route to the SFX group — not setting volume directly
        m_Source.outputAudioMixerGroup = m_SFXGroup;
        m_Source.playOnAwake = false;
    }
}
```

# AudioMixerSnapshot Transitions

- ✅ Use `AudioMixerSnapshot` to define different audio states (Combat, Exploration, UI, Pause) with different volume/effect settings.
- ✅ Transition between snapshots with `snapshot.TransitionTo(timeToReach)` for a smooth blend.
- ✅ Use `AudioMixer.TransitionToSnapshots(snapshots, weights, timeToReach)` to blend between multiple snapshots simultaneously.
- ❌ Do not manually lerp mixer parameters across frames to simulate snapshot behaviour — use `TransitionTo` instead.

```csharp
public class AudioStateManager : MonoBehaviour
{
    [SerializeField] private AudioMixerSnapshot m_ExplorationSnapshot;
    [SerializeField] private AudioMixerSnapshot m_CombatSnapshot;
    [SerializeField] private AudioMixerSnapshot m_PauseSnapshot;

    [SerializeField] private float m_TransitionTime = 0.5f;

    public void EnterCombat()
    {
        m_CombatSnapshot.TransitionTo(m_TransitionTime);
    }

    public void ExitCombat()
    {
        m_ExplorationSnapshot.TransitionTo(m_TransitionTime);
    }

    public void PauseGame()
    {
        // Instant pause — no transition time
        m_PauseSnapshot.TransitionTo(0f);
    }
}
```

# Volume Control via Exposed Parameters

- ✅ Expose mixer group volume parameters in the **Audio Mixer** window (**right-click the volume knob > Expose**).
- ✅ Name exposed parameters descriptively: `MasterVolume`, `MusicVolume`, `SFXVolume`.
- ✅ Convert linear volume (0–1 from a UI slider) to decibels before passing to `AudioMixer.SetFloat`.
- ❌ Never set volume as a linear value directly in `SetFloat` — the mixer expects decibels (dB).
- ❌ Never pass `0f` to the dB formula — it produces `-Infinity`; clamp the input to a minimum (e.g. `0.0001f`).

```csharp
public class VolumeController : MonoBehaviour
{
    [SerializeField] private AudioMixer m_Mixer;

    private const string k_MasterVolumeParam = "MasterVolume";
    private const string k_MusicVolumeParam = "MusicVolume";
    private const string k_SFXVolumeParam = "SFXVolume";

    // Call this from a UI Slider's onValueChanged event
    // sliderValue: 0.0 (silent) to 1.0 (full volume)
    public void SetMasterVolume(float sliderValue)
    {
        float db = LinearToDecibels(sliderValue);
        m_Mixer.SetFloat(k_MasterVolumeParam, db);
        PlayerPrefs.SetFloat(k_MasterVolumeParam, sliderValue);
    }

    public void SetMusicVolume(float sliderValue)
    {
        m_Mixer.SetFloat(k_MusicVolumeParam, LinearToDecibels(sliderValue));
        PlayerPrefs.SetFloat(k_MusicVolumeParam, sliderValue);
    }

    public void SetSFXVolume(float sliderValue)
    {
        m_Mixer.SetFloat(k_SFXVolumeParam, LinearToDecibels(sliderValue));
        PlayerPrefs.SetFloat(k_SFXVolumeParam, sliderValue);
    }

    private void Start()
    {
        // Restore saved volumes on startup
        SetMasterVolume(PlayerPrefs.GetFloat(k_MasterVolumeParam, 1f));
        SetMusicVolume(PlayerPrefs.GetFloat(k_MusicVolumeParam, 1f));
        SetSFXVolume(PlayerPrefs.GetFloat(k_SFXVolumeParam, 1f));
    }

    private static float LinearToDecibels(float linear)
    {
        // Clamp to avoid log(0) = -Infinity
        return Mathf.Log10(Mathf.Max(linear, 0.0001f)) * 20f;
    }
}
```

# AudioSource Pooling

- ✅ Pre-create a pool of `AudioSource` components on a persistent `GameObject` in the scene.
- ✅ Return `AudioSource` instances to the pool when `!audioSource.isPlaying`.
- ✅ Use `UnityEngine.Pool.ObjectPool<AudioSource>` to manage the pool lifecycle.
- ❌ Never create and destroy `AudioSource` GameObjects at runtime for frequent SFX — it causes GC allocations and audio glitches.
- ❌ Never use `AudioSource.PlayClipAtPoint` for sounds that play frequently (footsteps, bullets, impacts) — it allocates a new `GameObject` every call.

```csharp
public class AudioPool : MonoBehaviour
{
    [SerializeField] private int m_InitialPoolSize = 20;
    [SerializeField] private int m_MaxPoolSize = 40;
    [SerializeField] private AudioMixerGroup m_DefaultSFXGroup;

    private ObjectPool<AudioSource> m_Pool;

    private void Awake()
    {
        m_Pool = new ObjectPool<AudioSource>(
            createFunc: CreateAudioSource,
            actionOnGet: source => source.gameObject.SetActive(true),
            actionOnRelease: source => source.gameObject.SetActive(false),
            actionOnDestroy: source => Destroy(source.gameObject),
            collectionCheck: false,
            defaultCapacity: m_InitialPoolSize,
            maxSize: m_MaxPoolSize
        );
    }

    private AudioSource CreateAudioSource()
    {
        var go = new GameObject("PooledAudioSource");
        go.transform.SetParent(transform);
        var source = go.AddComponent<AudioSource>();
        source.outputAudioMixerGroup = m_DefaultSFXGroup;
        source.playOnAwake = false;
        return source;
    }

    public void PlaySFX(AudioClip clip, Vector3 position, float volume = 1f, float pitch = 1f)
    {
        AudioSource source = m_Pool.Get();
        source.transform.position = position;
        source.clip = clip;
        source.volume = volume;
        source.pitch = pitch;
        source.Play();
        StartCoroutine(ReturnWhenDone(source, clip.length / pitch));
    }

    private System.Collections.IEnumerator ReturnWhenDone(AudioSource source, float duration)
    {
        yield return new WaitForSeconds(duration);
        m_Pool.Release(source);
    }
}
```

# Avoid PlayClipAtPoint for Frequent SFX

```csharp
// AVOID for frequent SFX (footsteps, bullets, impacts)
AudioSource.PlayClipAtPoint(clip, transform.position);
// Allocates a new GameObject + AudioSource every call, then destroys it after playback

// PREFER — use a pooled audio service
[SerializeField] private AudioPool m_AudioPool;

private void OnFootstep()
{
    m_AudioPool.PlaySFX(m_FootstepClip, transform.position);
}
```

- ✅ `AudioSource.PlayClipAtPoint` is acceptable for rare events (explosion, level complete) where allocation cost is negligible.
- ❌ Never use `PlayClipAtPoint` inside `Update`, on collision events with many simultaneous colliders, or for any sound that plays more than a few times per second.

# AudioClip Memory Management

- ✅ Set `AudioClip.loadType` in the Inspector:
  - `Decompress On Load` — for short SFX clips (< 200 KB compressed). Fast access, higher memory.
  - `Compressed In Memory` — for medium-length clips (200 KB–1 MB). Balanced.
  - `Streaming` — for long music tracks (> 1 MB). Low memory, slight latency.
- ✅ Enable `Load In Background` on large clips to avoid a hitch when the clip first loads.
- ✅ Call `audioClip.UnloadAudioData()` on clips that are no longer needed to free memory.
- ✅ Call `audioClip.LoadAudioData()` before playing a clip that was previously unloaded.
- ❌ Never stream short SFX — streaming adds latency that causes perceptible delays.
- ❌ Never decompress-on-load long music tracks — they consume hundreds of MB of RAM uncompressed.

```csharp
public class AudioMemoryManager : MonoBehaviour
{
    [SerializeField] private AudioClip m_BossMusic;

    public void PrepareForBossFight()
    {
        // Ensure the clip is loaded before the fight starts
        if (!m_BossMusic.loadState.Equals(AudioDataLoadState.Loaded))
            m_BossMusic.LoadAudioData();
    }

    public void ExitBossFight()
    {
        // Unload boss music from memory when no longer needed
        m_BossMusic.UnloadAudioData();
    }
}
```

# Spatial Audio

- ✅ Set `AudioSource.spatialBlend = 1f` for fully 3D sounds (footsteps, weapon fire, ambient world sounds).
- ✅ Set `AudioSource.spatialBlend = 0f` for fully 2D sounds (music, UI feedback, voice-over).
- ✅ Configure `AudioSource.minDistance` and `maxDistance` to match the world scale of your scene.
- ✅ Use `AudioSource.rolloffMode = AudioRolloffMode.Custom` with a hand-tuned curve for realistic attenuation.
- ❌ Never leave `spatialBlend` at 0 for world-space SFX — sounds will be heard at equal volume everywhere.

```csharp
public class WorldSFXSource : MonoBehaviour
{
    private AudioSource m_Source;

    [SerializeField] private float m_MinDistance = 2f;
    [SerializeField] private float m_MaxDistance = 25f;

    private void Awake()
    {
        m_Source = GetComponent<AudioSource>();
        m_Source.spatialBlend = 1f;         // Fully 3D
        m_Source.minDistance = m_MinDistance;
        m_Source.maxDistance = m_MaxDistance;
        m_Source.rolloffMode = AudioRolloffMode.Logarithmic;
    }
}
```

# AudioListener

- ✅ Ensure exactly one `AudioListener` is active in the scene at all times — Unity warns about 0 or 2+ listeners.
- ✅ Place the `AudioListener` on the main camera or on the player character's head bone for first-person games.
- ✅ For additive multi-scene setups, disable extra `AudioListener` components when loading additive scenes that contain a camera with a listener.
- ❌ Never have two active `AudioListener` components simultaneously — the audio output will be undefined.

# Pooled Audio Service Pattern

The complete minimal pattern for a game-wide audio service:

```csharp
public class AudioService : MonoBehaviour
{
    public static AudioService Instance { get; private set; }

    [Header("Mixer Groups")]
    [SerializeField] private AudioMixerGroup m_SFXGroup;
    [SerializeField] private AudioMixerGroup m_MusicGroup;

    [Header("Pool Settings")]
    [SerializeField] private int m_SFXPoolSize = 20;

    private ObjectPool<AudioSource> m_SFXPool;
    private AudioSource m_MusicSource;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitialisePools();
    }

    private void InitialisePools()
    {
        m_SFXPool = new ObjectPool<AudioSource>(
            createFunc: CreateSFXSource,
            actionOnGet: s => s.gameObject.SetActive(true),
            actionOnRelease: s => { s.Stop(); s.gameObject.SetActive(false); },
            actionOnDestroy: s => Destroy(s.gameObject),
            defaultCapacity: m_SFXPoolSize,
            maxSize: m_SFXPoolSize * 2
        );

        var musicGo = new GameObject("MusicSource");
        musicGo.transform.SetParent(transform);
        m_MusicSource = musicGo.AddComponent<AudioSource>();
        m_MusicSource.outputAudioMixerGroup = m_MusicGroup;
        m_MusicSource.loop = true;
    }

    private AudioSource CreateSFXSource()
    {
        var go = new GameObject("SFX");
        go.transform.SetParent(transform);
        var src = go.AddComponent<AudioSource>();
        src.outputAudioMixerGroup = m_SFXGroup;
        src.playOnAwake = false;
        return src;
    }

    public void PlaySFX(AudioClip clip, Vector3 worldPosition)
    {
        if (clip == null) return;
        AudioSource src = m_SFXPool.Get();
        src.transform.position = worldPosition;
        src.spatialBlend = 1f;
        src.clip = clip;
        src.Play();
        StartCoroutine(ReturnToPool(src, clip.length));
    }

    public void PlayMusic(AudioClip musicClip)
    {
        m_MusicSource.clip = musicClip;
        m_MusicSource.Play();
    }

    private System.Collections.IEnumerator ReturnToPool(AudioSource src, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (src != null && src.gameObject.activeSelf)
            m_SFXPool.Release(src);
    }
}
```
