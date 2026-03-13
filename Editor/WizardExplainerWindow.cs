using System;
using UnityEditor;
using UnityEngine;

namespace Unity.BestPractices.Editor
{
    /// <summary>
    /// A small floating window that displays contextual help for a wizard step.
    /// Open via <see cref="Show"/> — pass a heading and a delegate that draws the body content.
    /// </summary>
    /// <remarks>
    /// Designed for reuse across all <see cref="NewProjectWizard"/> steps.
    /// Each step provides its own <c>drawContent</c> action so the window can render anything:
    /// plain text, folder trees, setting tables, etc.
    /// </remarks>
    public class WizardExplainerWindow : EditorWindow
    {
        private string m_heading;
        private Action m_drawContent;
        private Vector2 m_scrollPosition;

        /// <summary>Opens a floating explainer window with the given heading and body renderer.</summary>
        /// <param name="heading">Bold title shown at the top of the window.</param>
        /// <param name="drawContent">IMGUI delegate that draws the window body content.</param>
        public static void Show(string heading, Action drawContent)
        {
            var window = CreateInstance<WizardExplainerWindow>();
            window.titleContent = new GUIContent(heading);
            window.m_heading = heading;
            window.m_drawContent = drawContent;
            window.minSize = new Vector2(300, 360);
            window.maxSize = new Vector2(480, 700);
            window.ShowUtility();
        }

        private void OnGUI()
        {
            // Header
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label(m_heading, EditorStyles.boldLabel);
            EditorGUILayout.EndVertical();

            GUILayout.Space(4);

            // Scrollable body
            m_scrollPosition = EditorGUILayout.BeginScrollView(m_scrollPosition);
            EditorGUILayout.BeginVertical();
            m_drawContent?.Invoke();
            GUILayout.Space(8);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();

            GUILayout.Space(4);

            // Close
            if (GUILayout.Button("Close", GUILayout.Height(26)))
                Close();
        }
    }
}
