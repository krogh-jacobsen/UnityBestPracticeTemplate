using System.Collections;
using System.Globalization;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Pool;

namespace UnityBestPractices.Samples.AudioManager
{
    /// <summary>
    /// Game-wide audio service singleton.
    /// <para>
    /// Wraps <see cref="ObjectPool{AudioSource}"/> for allocation-free SFX playback, routes all
    /// audio through an <see cref="AudioMixer"/>, and persists per-channel volume settings via
    /// <see cref="PlayerPrefs"/>.
    /// </para>
    /// <para>
    /// Place this component on a root <see cref="GameObject"/> in a persistent Bootstrap scene,
    /// or let it survive scene loads by calling <see cref="Object.DontDestroyOnLoad"/> in Awake
    /// (already handled).
    /// </para>
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        #region Singleton

        public static AudioManager Instance { get; private set; }

        #endregion

        #region Fields

        [SerializeField] private AudioManagerConfig m_Config;

        // Pooled SFX sources
        private ObjectPool<AudioSource> m_SFXPool;

        // Dedicated non-pooled sources for continuous streams
        private AudioSource m_MusicSourceA;
        private AudioSource m_MusicSourceB;
        private AudioSource m_AmbienceSource;
        private bool m_MusicOnA = true;

        // Active crossfade coroutine — stop before starting a new one
        private Coroutine m_CrossfadeRoutine;

        #endregion

        #region Properties

        /// <summary>Currently active music source (the one audible to the player).</summary>
        public AudioSource ActiveMusicSource => m_MusicOnA ? m_MusicSourceA : m_MusicSourceB;

        #endregion

        #region MonoBehaviour Methods

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (m_Config == null)
            {
                Debug.LogError("[AudioManager] AudioManagerConfig is not assigned. " +
                               "Create one via Assets → Create → Best Practices → Audio Manager Config " +
                               "and assign it in the Inspector.");
                return;
            }

            InitialiseSFXPool();
            InitialiseMusicSources();
            InitialiseAmbienceSource();
            LoadSavedVolumes();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                m_SFXPool?.Dispose();
                Instance = null;
            }
        }

        #endregion

        #region Public API — Playback

        /// <summary>
        /// Plays a one-shot SFX at <paramref name="worldPosition"/> using a pooled
        /// <see cref="AudioSource"/>. The source is returned to the pool automatically
        /// when the clip finishes.
        /// </summary>
        public void PlaySFX(AudioClip clip, Vector3 worldPosition, float volume = 1f, float pitch = 1f)
        {
            if (clip == null) return;
            if (m_Config.SFXGroup == null)
            {
                Debug.LogWarning("[AudioManager] SFXGroup is not assigned in AudioManagerConfig.");
                return;
            }

            AudioSource source = m_SFXPool.Get();
            source.transform.position = worldPosition;
            source.clip = clip;
            source.volume = volume;
            source.pitch = pitch;
            source.spatialBlend = 1f;   // Full 3D
            source.Play();
            StartCoroutine(ReturnToPool(source, clip.length / Mathf.Max(pitch, 0.01f)));
        }

        /// <summary>
        /// Plays a one-shot 2D UI sound directly from the pool (no spatial blend).
        /// </summary>
        public void PlayUI(AudioClip clip, float volume = 1f)
        {
            if (clip == null) return;

            AudioSource source = m_SFXPool.Get();
            // Override mixer group to UI for this shot
            if (m_Config.UIGroup != null)
                source.outputAudioMixerGroup = m_Config.UIGroup;

            source.transform.position = Vector3.zero;
            source.clip = clip;
            source.volume = volume;
            source.pitch = 1f;
            source.spatialBlend = 0f;  // 2D
            source.Play();
            StartCoroutine(ReturnToPool(source, clip.length,
                restoreGroup: m_Config.SFXGroup));
        }

        /// <summary>
        /// Starts playing background music immediately.
        /// Use <see cref="CrossfadeMusic"/> for a smooth transition.
        /// </summary>
        public void PlayMusic(AudioClip clip, bool loop = true)
        {
            if (clip == null) return;
            AudioSource active = ActiveMusicSource;
            active.clip = clip;
            active.loop = loop;
            active.volume = 1f;
            active.Play();
        }

        /// <summary>
        /// Crossfades from the currently playing music to <paramref name="newClip"/>
        /// over <see cref="AudioManagerConfig.CrossfadeDuration"/> seconds.
        /// </summary>
        public void CrossfadeMusic(AudioClip newClip, bool loop = true)
        {
            if (newClip == null) return;

            if (m_CrossfadeRoutine != null)
                StopCoroutine(m_CrossfadeRoutine);

            m_CrossfadeRoutine = StartCoroutine(CrossfadeRoutine(newClip, loop, m_Config.CrossfadeDuration));
        }

        /// <summary>Stops music playback on both sources.</summary>
        public void StopMusic(float fadeOutDuration = 0f)
        {
            if (fadeOutDuration <= 0f)
            {
                m_MusicSourceA.Stop();
                m_MusicSourceB.Stop();
            }
            else
            {
                StartCoroutine(FadeOutRoutine(ActiveMusicSource, fadeOutDuration));
            }
        }

        /// <summary>Plays a looping ambience clip on the dedicated ambience source.</summary>
        public void PlayAmbience(AudioClip clip, float volume = 1f)
        {
            if (clip == null) return;
            m_AmbienceSource.clip = clip;
            m_AmbienceSource.volume = volume;
            m_AmbienceSource.loop = true;
            m_AmbienceSource.Play();
        }

        /// <summary>Stops the ambience source.</summary>
        public void StopAmbience() => m_AmbienceSource.Stop();

        #endregion

        #region Public API — Volume

        /// <summary>
        /// Sets the volume for <paramref name="channel"/> from a linear 0–1 slider value,
        /// converts it to decibels, applies it to the <see cref="AudioMixer"/> via the
        /// corresponding exposed parameter, and persists the value in <see cref="PlayerPrefs"/>.
        /// </summary>
        public void SetVolume(AudioChannel channel, float linearValue)
        {
            float db = LinearToDecibels(linearValue);
            string param = GetExposedParam(channel);

            if (string.IsNullOrEmpty(param))
            {
                Debug.LogWarning($"[AudioManager] No exposed parameter name configured for channel {channel}.");
                return;
            }

            // All mixer groups share the same AudioMixer root — grab it from whichever group is available
            AudioMixer mixer = GetMixerForChannel(channel);
            if (mixer == null)
            {
                Debug.LogWarning($"[AudioManager] No AudioMixerGroup assigned for channel {channel}.");
                return;
            }

            mixer.SetFloat(param, db);
            PlayerPrefs.SetFloat(param, linearValue);
        }

        /// <summary>Returns the persisted linear volume for <paramref name="channel"/> (0–1).</summary>
        public float GetVolume(AudioChannel channel)
        {
            string param = GetExposedParam(channel);
            return PlayerPrefs.GetFloat(param, 1f);
        }

        #endregion

        #region Initialisation

        private void InitialiseSFXPool()
        {
            m_SFXPool = new ObjectPool<AudioSource>(
                createFunc: CreateSFXSource,
                actionOnGet: source =>
                {
                    source.outputAudioMixerGroup = m_Config.SFXGroup;
                    source.gameObject.SetActive(true);
                },
                actionOnRelease: source =>
                {
                    source.Stop();
                    source.clip = null;
                    source.gameObject.SetActive(false);
                },
                actionOnDestroy: source => Destroy(source.gameObject),
                collectionCheck: false,
                defaultCapacity: m_Config.PoolDefaultCapacity,
                maxSize: m_Config.PoolMaxSize);
        }

        private void InitialiseMusicSources()
        {
            m_MusicSourceA = CreateDedicatedSource("MusicSource_A", m_Config.MusicGroup, loop: true);
            m_MusicSourceB = CreateDedicatedSource("MusicSource_B", m_Config.MusicGroup, loop: true);
            m_MusicSourceB.volume = 0f; // B starts silent — used as the crossfade target
        }

        private void InitialiseAmbienceSource()
        {
            m_AmbienceSource = CreateDedicatedSource("AmbienceSource", m_Config.AmbienceGroup, loop: true);
        }

        private AudioSource CreateSFXSource()
        {
            var go = new GameObject("PooledSFX");
            go.transform.SetParent(transform);
            var source = go.AddComponent<AudioSource>();
            source.outputAudioMixerGroup = m_Config.SFXGroup;
            source.playOnAwake = false;
            source.gameObject.SetActive(false);
            return source;
        }

        private AudioSource CreateDedicatedSource(string goName, AudioMixerGroup group, bool loop)
        {
            var go = new GameObject(goName);
            go.transform.SetParent(transform);
            var source = go.AddComponent<AudioSource>();
            source.outputAudioMixerGroup = group;
            source.playOnAwake = false;
            source.loop = loop;
            return source;
        }

        private void LoadSavedVolumes()
        {
            foreach (AudioChannel channel in System.Enum.GetValues(typeof(AudioChannel)))
            {
                string param = GetExposedParam(channel);
                if (string.IsNullOrEmpty(param)) continue;

                float saved = PlayerPrefs.GetFloat(param, 1f);
                AudioMixer mx = GetMixerForChannel(channel);
                mx?.SetFloat(param, LinearToDecibels(saved));
            }

            // Also restore master volume
            AudioMixer masterMixer = GetAnyMixer();
            if (masterMixer != null)
            {
                float savedMaster = PlayerPrefs.GetFloat(m_Config.MasterVolumeParam, 1f);
                masterMixer.SetFloat(m_Config.MasterVolumeParam, LinearToDecibels(savedMaster));
            }
        }

        #endregion

        #region Coroutines

        private IEnumerator ReturnToPool(AudioSource source, float delay,
            AudioMixerGroup restoreGroup = null)
        {
            yield return new WaitForSeconds(delay);
            if (restoreGroup != null)
                source.outputAudioMixerGroup = restoreGroup;
            if (source != null && source.gameObject.activeSelf)
                m_SFXPool.Release(source);
        }

        private IEnumerator CrossfadeRoutine(AudioClip newClip, bool loop, float duration)
        {
            AudioSource outgoing = ActiveMusicSource;
            m_MusicOnA = !m_MusicOnA;
            AudioSource incoming = ActiveMusicSource;

            incoming.clip = newClip;
            incoming.loop = loop;
            incoming.volume = 0f;
            incoming.Play();

            float elapsed = 0f;
            float startVolume = outgoing.volume;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                incoming.volume = Mathf.Lerp(0f, startVolume, t);
                outgoing.volume = Mathf.Lerp(startVolume, 0f, t);
                yield return null;
            }

            outgoing.Stop();
            outgoing.clip = null;
            outgoing.volume = 1f;
            incoming.volume = startVolume;
            m_CrossfadeRoutine = null;
        }

        private IEnumerator FadeOutRoutine(AudioSource source, float duration)
        {
            float startVolume = source.volume;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                source.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
                yield return null;
            }

            source.Stop();
            source.volume = startVolume;
        }

        #endregion

        #region Helpers

        private string GetExposedParam(AudioChannel channel) => channel switch
        {
            AudioChannel.SFX => m_Config.SFXVolumeParam,
            AudioChannel.Music => m_Config.MusicVolumeParam,
            AudioChannel.UI => m_Config.UIVolumeParam,
            AudioChannel.Ambience => m_Config.AmbienceVolumeParam,
            _ => string.Empty
        };

        private AudioMixer GetMixerForChannel(AudioChannel channel)
        {
            AudioMixerGroup group = channel switch
            {
                AudioChannel.SFX => m_Config.SFXGroup,
                AudioChannel.Music => m_Config.MusicGroup,
                AudioChannel.UI => m_Config.UIGroup,
                AudioChannel.Ambience => m_Config.AmbienceGroup,
                _ => null
            };
            return group?.audioMixer;
        }

        private AudioMixer GetAnyMixer()
        {
            return (m_Config.SFXGroup ?? m_Config.MusicGroup ?? m_Config.UIGroup ?? m_Config.AmbienceGroup)
                ?.audioMixer;
        }

        private static float LinearToDecibels(float linear)
        {
            // Clamp to avoid log10(0) = -Infinity
            return Mathf.Log10(Mathf.Max(linear, 0.0001f)) * 20f;
        }

        #endregion
    }
}
