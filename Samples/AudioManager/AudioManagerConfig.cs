using UnityEngine;
using UnityEngine.Audio;

namespace UnityBestPractices.Samples.AudioManager
{
    /// <summary>
    /// Project-wide audio configuration asset.
    /// Assign mixer groups, exposed parameter names, and pool sizes here.
    /// Create via <b>Assets → Create → Best Practices → Audio Manager Config</b>.
    /// </summary>
    [CreateAssetMenu(
        fileName = "AudioManagerConfig",
        menuName = "Best Practices/Audio Manager Config",
        order = 20)]
    public class AudioManagerConfig : ScriptableObject
    {
        [Header("Mixer Groups — drag from your AudioMixer asset")]
        public AudioMixerGroup SFXGroup;
        public AudioMixerGroup MusicGroup;
        public AudioMixerGroup UIGroup;
        public AudioMixerGroup AmbienceGroup;

        [Header("Exposed Parameter Names — must match the AudioMixer exactly (case-sensitive)")]
        public string MasterVolumeParam   = "MasterVolume";
        public string MusicVolumeParam    = "MusicVolume";
        public string SFXVolumeParam      = "SFXVolume";
        public string UIVolumeParam       = "UIVolume";
        public string AmbienceVolumeParam = "AmbienceVolume";

        [Header("SFX Pool")]
        [Min(1)] public int PoolDefaultCapacity = 20;
        [Min(1)] public int PoolMaxSize         = 40;

        [Header("Music Crossfade")]
        [Min(0f)] public float CrossfadeDuration = 1.5f;
    }
}
