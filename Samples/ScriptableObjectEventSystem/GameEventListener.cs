using UnityEngine;
using UnityEngine.Events;

namespace UnityBestPractices.Samples
{
    /// <summary>
    /// Listens to a GameEvent ScriptableObject and invokes a UnityEvent response.
    /// Attach to any GameObject. Assign the GameEvent asset and the Response in the Inspector.
    /// </summary>
    public class GameEventListener : MonoBehaviour
    {
        #region Fields

        [SerializeField] private GameEvent m_Event;
        [SerializeField] private UnityEvent m_Response;

        #endregion

        #region MonoBehaviour Methods

        private void OnEnable()
        {
            if (m_Event != null)
            {
                m_Event.Subscribe(HandleEventRaised);
            }
        }

        private void OnDisable()
        {
            if (m_Event != null)
            {
                m_Event.Unsubscribe(HandleEventRaised);
            }
        }

        #endregion

        #region Private Methods

        private void HandleEventRaised()
        {
            m_Response?.Invoke();
        }

        #endregion
    }
}
