using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityBestPractices.Samples
{
    /// <summary>
    /// Owns the UIDocument. Queries elements, exposes events for the presenter,
    /// and exposes methods for the presenter to call. No business logic here.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class SampleView : MonoBehaviour
    {
        #region Fields

        private UIDocument m_Document;
        private Label m_CountLabel;
        private Button m_IncrementButton;

        private SamplePresenter m_Presenter;

        #endregion

        #region Events

        public event Action OnIncrementClicked;

        #endregion

        #region MonoBehaviour Methods

        private void Awake()
        {
            m_Document = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            var root = m_Document.rootVisualElement;
            m_CountLabel = root.Q<Label>("count-label");
            m_IncrementButton = root.Q<Button>("increment-button");

            m_IncrementButton.clicked += HandleIncrementClicked;

            m_Presenter = new SamplePresenter(this);
        }

        private void OnDisable()
        {
            m_IncrementButton.clicked -= HandleIncrementClicked;
            m_Presenter?.Dispose();
            m_Presenter = null;
        }

        #endregion

        #region Public Methods

        public void SetCount(int count)
        {
            if (m_CountLabel != null)
            {
                m_CountLabel.text = count.ToString();
            }
        }

        #endregion

        #region Private Methods

        private void HandleIncrementClicked()
        {
            OnIncrementClicked?.Invoke();
        }

        #endregion
    }
}
