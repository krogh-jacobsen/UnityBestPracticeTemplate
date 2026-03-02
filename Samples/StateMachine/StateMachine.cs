using UnityEngine;

namespace UnityBestPractices.Samples
{
    /// <summary>
    /// A generic state machine. Call ChangeState() to transition;
    /// call Update() from the owner's Update() method.
    /// </summary>
    public class StateMachine
    {
        #region Fields

        private IState m_CurrentState;

        #endregion

        #region Properties

        public IState CurrentState => m_CurrentState;

        #endregion

        #region Public Methods

        public void ChangeState(IState newState)
        {
            m_CurrentState?.OnExit();
            m_CurrentState = newState;
            m_CurrentState?.OnEnter();
        }

        public void Update()
        {
            m_CurrentState?.OnUpdate();
        }

        #endregion
    }
}
