namespace UnityBestPractices.Samples
{
    /// <summary>
    /// Contract for all states in the state machine.
    /// </summary>
    public interface IState
    {
        void OnEnter();
        void OnUpdate();
        void OnExit();
    }
}
