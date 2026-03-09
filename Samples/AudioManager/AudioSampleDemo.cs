using UnityEngine;
using UnityEngine.UI;

namespace UnityBestPractices.Samples.AudioManager
{
    /// <summary>
    /// Wires the <see cref="AudioManager"/> singleton to a set of demo UI controls.
    /// Attach this to a canvas root alongside the slider and button references.
    /// </summary>
    public class AudioSampleDemo : MonoBehaviour
    {
        #region Fields

        [Header("Demo Clips — assign in the Inspector")]
        [SerializeField] private AudioClip m_SFXClip;
        [SerializeField] private AudioClip m_UISFXClip;
        [SerializeField] private AudioClip m_MusicClipA;
        [SerializeField] private AudioClip m_MusicClipB;
        [SerializeField] private AudioClip m_AmbienceClip;

        [Header("Volume Sliders")]
        [SerializeField] private Slider m_MasterSlider;
        [SerializeField] private Slider m_MusicSlider;
        [SerializeField] private Slider m_SFXSlider;
        [SerializeField] private Slider m_UISlider;
        [SerializeField] private Slider m_AmbienceSlider;

        #endregion

        #region MonoBehaviour Methods

        private void Start()
        {
            // Initialise slider positions from persisted preferences
            InitSlider(m_MasterSlider, AudioChannel.SFX, onChanged: null, masterParam: true);
            InitSlider(m_MusicSlider, AudioChannel.Music, OnMusicVolumeChanged);
            InitSlider(m_SFXSlider, AudioChannel.SFX, OnSFXVolumeChanged);
            InitSlider(m_UISlider, AudioChannel.UI, OnUIVolumeChanged);
            InitSlider(m_AmbienceSlider, AudioChannel.Ambience, OnAmbienceVolumeChanged);
        }

        #endregion

        #region Button Callbacks — wire these to UI Button onClick events

        public void OnPlaySFXButton()
        {
            if (AudioManager.Instance == null) { WarnNoManager(); return; }
            AudioManager.Instance.PlaySFX(m_SFXClip, Camera.main != null ? Camera.main.transform.position : Vector3.zero);
        }

        public void OnPlayUIButton()
        {
            if (AudioManager.Instance == null) { WarnNoManager(); return; }
            AudioManager.Instance.PlayUI(m_UISFXClip);
        }

        public void OnPlayMusicAButton()
        {
            if (AudioManager.Instance == null) { WarnNoManager(); return; }
            AudioManager.Instance.PlayMusic(m_MusicClipA);
        }

        public void OnCrossfadeToMusicBButton()
        {
            if (AudioManager.Instance == null) { WarnNoManager(); return; }
            AudioManager.Instance.CrossfadeMusic(m_MusicClipB);
        }

        public void OnStopMusicButton()
        {
            if (AudioManager.Instance == null) { WarnNoManager(); return; }
            AudioManager.Instance.StopMusic(fadeOutDuration: 1f);
        }

        public void OnPlayAmbienceButton()
        {
            if (AudioManager.Instance == null) { WarnNoManager(); return; }
            AudioManager.Instance.PlayAmbience(m_AmbienceClip);
        }

        public void OnStopAmbienceButton()
        {
            if (AudioManager.Instance == null) { WarnNoManager(); return; }
            AudioManager.Instance.StopAmbience();
        }

        #endregion

        #region Slider Callbacks

        private void OnMusicVolumeChanged(float value)
        {
            AudioManager.Instance?.SetVolume(AudioChannel.Music, value);
        }

        private void OnSFXVolumeChanged(float value)
        {
            AudioManager.Instance?.SetVolume(AudioChannel.SFX, value);
        }

        private void OnUIVolumeChanged(float value)
        {
            AudioManager.Instance?.SetVolume(AudioChannel.UI, value);
        }

        private void OnAmbienceVolumeChanged(float value)
        {
            AudioManager.Instance?.SetVolume(AudioChannel.Ambience, value);
        }

        #endregion

        #region Private Helpers

        /// <summary>
        /// Sets the slider's initial value from persisted preferences and registers the callback.
        /// </summary>
        private static void InitSlider(Slider slider, AudioChannel channel,
            UnityEngine.Events.UnityAction<float> onChanged,
            bool masterParam = false)
        {
            if (slider == null) return;

            float saved = AudioManager.Instance != null
                ? AudioManager.Instance.GetVolume(channel)
                : 1f;

            slider.SetValueWithoutNotify(saved);

            if (onChanged != null)
                slider.onValueChanged.AddListener(onChanged);
        }

        private static void WarnNoManager()
        {
            Debug.LogWarning("[AudioSampleDemo] No AudioManager instance found in the scene. " +
                             "Add a GameObject with the AudioManager component to a Bootstrap scene " +
                             "or mark it with DontDestroyOnLoad.");
        }

        #endregion
    }
}
