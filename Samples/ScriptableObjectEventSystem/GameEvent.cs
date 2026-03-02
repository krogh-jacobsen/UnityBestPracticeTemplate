using System;
using UnityEngine;

namespace UnityBestPractices.Samples
{
    /// <summary>
    /// A ScriptableObject-based event channel. Raise it from anywhere;
    /// GameEventListeners subscribe and respond without direct references.
    /// </summary>
    [CreateAssetMenu(fileName = "NewGameEvent", menuName = "Best Practices/Game Event")]
    public class GameEvent : ScriptableObject
    {
        #region Events

        public static event Action<GameEvent> OnAnyEventRaised;
        private event Action m_OnRaised;

        #endregion

        #region Public Methods

        public void Subscribe(Action listener)
        {
            m_OnRaised += listener;
        }

        public void Unsubscribe(Action listener)
        {
            m_OnRaised -= listener;
        }

        public void Raise()
        {
            m_OnRaised?.Invoke();
            OnAnyEventRaised?.Invoke(this);
        }

        #endregion
    }
}
