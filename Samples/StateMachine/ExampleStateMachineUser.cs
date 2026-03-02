using UnityEngine;

namespace UnityBestPractices.Samples
{
    /// <summary>
    /// Example MonoBehaviour demonstrating how to wire two states into the StateMachine.
    /// Press Space to toggle between Idle and Moving states.
    /// </summary>
    public class ExampleStateMachineUser : MonoBehaviour
    {
        #region Fields

        private StateMachine m_StateMachine;
        private IdleState m_IdleState;
        private MovingState m_MovingState;

        #endregion

        #region MonoBehaviour Methods

        private void Awake()
        {
            m_StateMachine = new StateMachine();
            m_IdleState = new IdleState(this);
            m_MovingState = new MovingState(this);
        }

        private void Start()
        {
            m_StateMachine.ChangeState(m_IdleState);
        }

        private void Update()
        {
            m_StateMachine.Update();

            if (Input.GetKeyDown(KeyCode.Space))
            {
                ToggleState();
            }
        }

        #endregion

        #region Private Methods

        private void ToggleState()
        {
            if (m_StateMachine.CurrentState == m_IdleState)
            {
                m_StateMachine.ChangeState(m_MovingState);
            }
            else
            {
                m_StateMachine.ChangeState(m_IdleState);
            }
        }

        #endregion

        #region Nested State Classes

        private class IdleState : IState
        {
            private readonly ExampleStateMachineUser m_Owner;

            public IdleState(ExampleStateMachineUser owner) => m_Owner = owner;

            public void OnEnter() => Debug.Log("Entered Idle");
            public void OnUpdate() { }
            public void OnExit() => Debug.Log("Exited Idle");
        }

        private class MovingState : IState
        {
            private readonly ExampleStateMachineUser m_Owner;

            public MovingState(ExampleStateMachineUser owner) => m_Owner = owner;

            public void OnEnter() => Debug.Log("Entered Moving");

            public void OnUpdate()
            {
                m_Owner.transform.Translate(Vector3.forward * Time.deltaTime);
            }

            public void OnExit() => Debug.Log("Exited Moving");
        }

        #endregion
    }
}
