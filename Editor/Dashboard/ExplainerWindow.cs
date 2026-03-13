using UnityEditor;
using UnityEngine;

namespace UnityBestPractices.Editor.Dashboard
{
    /// <summary>
    /// A generic floating explainer popup that describes what a dashboard feature does
    /// and optionally lets the user run its action directly from the window.
    /// </summary>
    /// <remarks>
    /// Open via <see cref="Show"/>. Pass a title, any number of plain-text paragraphs,
    /// an optional pre-formatted "preview" block (monospaced), and an optional run action.
    /// </remarks>
    public class ExplainerWindow : EditorWindow
    {
        private string m_title;
        private string[] m_paragraphs;
        private string m_preview;
        private string m_runLabel;
        private System.Action m_runAction;
        private Vector2 m_scroll;

        /// <summary>
        /// Opens an explainer popup window.
        /// </summary>
        /// <param name="title">Window title and heading.</param>
        /// <param name="paragraphs">One or more explanation paragraphs shown in sequence.</param>
        /// <param name="preview">Optional pre-formatted text block (e.g. a folder tree) rendered in a monospaced style.</param>
        /// <param name="runLabel">Label for the action button. Leave null to hide the button.</param>
        /// <param name="runAction">Action invoked when the run button is clicked.</param>
        public static void Show(string title, string[] paragraphs, string preview = null,
            string runLabel = null, System.Action runAction = null)
        {
            var win = CreateInstance<ExplainerWindow>();
            win.titleContent = new GUIContent(title);
            win.m_title = title;
            win.m_paragraphs = paragraphs;
            win.m_preview = preview;
            win.m_runLabel = runLabel;
            win.m_runAction = runAction;
            win.minSize = new Vector2(420, 300);
            win.maxSize = new Vector2(560, 700);
            win.ShowUtility();
        }

        private void OnGUI()
        {
            m_scroll = EditorGUILayout.BeginScrollView(m_scroll);

            GUILayout.Space(6);
            GUILayout.Label(m_title, EditorStyles.boldLabel);
            GUILayout.Space(4);

            if (m_paragraphs != null)
            {
                foreach (var para in m_paragraphs)
                {
                    GUILayout.Label(para, EditorStyles.wordWrappedLabel);
                    GUILayout.Space(4);
                }
            }

            if (!string.IsNullOrEmpty(m_preview))
            {
                GUILayout.Space(4);
                GUILayout.Label("Folder structure", EditorStyles.miniLabel);
                var style = new GUIStyle(EditorStyles.helpBox)
                {
                    font = Font.CreateDynamicFontFromOSFont(new[] { "Courier New", "Courier", "monospace" }, 11),
                    wordWrap = false,
                    richText = false,
                    padding = new RectOffset(8, 8, 6, 6)
                };
                GUILayout.Label(m_preview, style);
            }

            EditorGUILayout.EndScrollView();

            if (!string.IsNullOrEmpty(m_runLabel) && m_runAction != null)
            {
                GUILayout.Space(4);
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(m_runLabel, GUILayout.Width(120), GUILayout.Height(26)))
                {
                    m_runAction.Invoke();
                    Close();
                }
                GUILayout.Space(6);
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(6);
            }
        }
    }
}
