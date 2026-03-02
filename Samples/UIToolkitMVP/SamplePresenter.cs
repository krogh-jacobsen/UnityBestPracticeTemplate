using System;

namespace UnityBestPractices.Samples
{
    /// <summary>
    /// Owns model data and drives the view. Has no Unity dependencies.
    /// </summary>
    public class SamplePresenter
    {
        #region Fields

        private int m_Count;
        private readonly SampleView m_View;

        #endregion

        #region Events

        public event Action<int> CountChanged;

        #endregion

        #region Properties

        public int Count => m_Count;

        #endregion

        #region Public Methods

        public SamplePresenter(SampleView view)
        {
            m_View = view;
            m_View.OnIncrementClicked += HandleIncrementClicked;
        }

        public void Dispose()
        {
            m_View.OnIncrementClicked -= HandleIncrementClicked;
        }

        #endregion

        #region Private Methods

        private void HandleIncrementClicked()
        {
            m_Count++;
            m_View.SetCount(m_Count);
            CountChanged?.Invoke(m_Count);
        }

        #endregion
    }
}
