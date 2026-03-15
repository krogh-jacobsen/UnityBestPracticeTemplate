using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Unity.BestPractices.Editor
{
    /// <summary>
    /// Visualises and edits the Physics layer collision matrix as a triangular grid of checkboxes.
    /// Rules can be saved to a <see cref="LayerCollisionConfig"/> asset and applied on demand.
    /// <para>Menu: <b>Window → Best Practices → Layer Collision Matrix</b></para>
    /// </summary>
    public class LayerCollisionMatrixWindow : EditorWindow
    {
        #region Constants

        private const int k_MaxLayers = 32;
        private const int k_CellSize = 18;
        private const int k_LabelWidth = 130;

        #endregion

        #region Fields

        private bool[,] m_Matrix = new bool[k_MaxLayers, k_MaxLayers];
        private string[] m_LayerNames = new string[k_MaxLayers];
        private int[] m_ActiveLayerIndices = System.Array.Empty<int>();
        private Vector2 m_ScrollPosition;

        #endregion

        #region Editor Window Methods

        [MenuItem("Tools/Unity Project Configurator/Utilities/Layer Collision Matrix", false, 250)]
        public static void ShowWindow()
        {
            var window = GetWindow<LayerCollisionMatrixWindow>("Layer Collision Matrix");
            window.minSize = new Vector2(500, 420);
            window.Show();
        }

        private void OnEnable()
        {
            LoadFromPhysicsSettings();
        }

        private void OnGUI()
        {
            DrawToolbar();
            GUILayout.Space(6);
            DrawMatrix();
        }

        #endregion

        #region Private Methods — Toolbar

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (GUILayout.Button("Reload from Physics", EditorStyles.toolbarButton, GUILayout.Width(150)))
                LoadFromPhysicsSettings();

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Load Config Asset", EditorStyles.toolbarButton, GUILayout.Width(130)))
                LoadFromConfigAsset();

            if (GUILayout.Button("Save Config Asset", EditorStyles.toolbarButton, GUILayout.Width(130)))
                SaveToConfigAsset();

            if (GUILayout.Button("Apply to Physics", EditorStyles.toolbarButton, GUILayout.Width(130)))
                ApplyToPhysics();

            EditorGUILayout.EndHorizontal();
        }

        #endregion

        #region Private Methods — Matrix UI

        private void DrawMatrix()
        {
            if (m_ActiveLayerIndices == null || m_ActiveLayerIndices.Length == 0)
            {
                EditorGUILayout.HelpBox("No named layers found in this project.", MessageType.Info);
                return;
            }

            m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition);

            int count = m_ActiveLayerIndices.Length;

            // Column headers — rotated via fixed-height labels
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(k_LabelWidth + 4);
            for (int ci = 0; ci < count; ci++)
            {
                int idx = m_ActiveLayerIndices[ci];
                var content = new GUIContent(m_LayerNames[idx], $"Layer {idx}");
                GUILayout.Label(content, EditorStyles.miniLabel,
                    GUILayout.Width(k_CellSize),
                    GUILayout.Height(k_LabelWidth));
            }
            EditorGUILayout.EndHorizontal();

            // Rows
            for (int ri = 0; ri < count; ri++)
            {
                int rowLayer = m_ActiveLayerIndices[ri];
                EditorGUILayout.BeginHorizontal();

                GUILayout.Label(m_LayerNames[rowLayer], GUILayout.Width(k_LabelWidth));

                for (int ci = 0; ci < count; ci++)
                {
                    int colLayer = m_ActiveLayerIndices[ci];

                    if (ci > ri)
                    {
                        // Upper triangle — editable
                        bool current = m_Matrix[rowLayer, colLayer];
                        bool toggled = EditorGUILayout.Toggle(current, GUILayout.Width(k_CellSize));
                        if (toggled != current)
                        {
                            m_Matrix[rowLayer, colLayer] = toggled;
                            m_Matrix[colLayer, rowLayer] = toggled; // keep symmetric
                        }
                    }
                    else if (ci == ri)
                    {
                        // Diagonal — same layer always collides with itself
                        GUI.enabled = false;
                        EditorGUILayout.Toggle(true, GUILayout.Width(k_CellSize));
                        GUI.enabled = true;
                    }
                    else
                    {
                        // Lower triangle — mirror of upper; read-only
                        GUI.enabled = false;
                        EditorGUILayout.Toggle(m_Matrix[rowLayer, colLayer], GUILayout.Width(k_CellSize));
                        GUI.enabled = true;
                    }
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();

            GUILayout.Space(4);
            EditorGUILayout.HelpBox(
                "Checked  =  layers collide with each other.\n" +
                "Unchecked =  layers ignore each other.\n" +
                "Click 'Apply to Physics' to write your changes to the Physics Settings.",
                MessageType.Info);
        }

        #endregion

        #region Private Methods — Data Operations

        private void LoadFromPhysicsSettings()
        {
            RefreshLayerNames();

            for (int a = 0; a < k_MaxLayers; a++)
            {
                for (int b = a; b < k_MaxLayers; b++)
                {
                    bool collides = !Physics.GetIgnoreLayerCollision(a, b);
                    m_Matrix[a, b] = collides;
                    m_Matrix[b, a] = collides;
                }
            }

            Repaint();
        }

        private void ApplyToPhysics()
        {
            int changes = 0;

            for (int a = 0; a < k_MaxLayers; a++)
            {
                for (int b = a; b < k_MaxLayers; b++)
                {
                    bool shouldIgnore = !m_Matrix[a, b];
                    bool currentIgnore = Physics.GetIgnoreLayerCollision(a, b);

                    if (shouldIgnore != currentIgnore)
                    {
                        Physics.IgnoreLayerCollision(a, b, shouldIgnore);
                        changes++;
                    }
                }
            }

            string summary = $"Applied {changes} change(s) to Physics Settings.";
            Debug.Log($"[Best Practices] Layer Collision Matrix: {summary}");
            EditorUtility.DisplayDialog("Layer Collision Matrix", summary, "OK");
        }

        private void LoadFromConfigAsset()
        {
            string[] guids = AssetDatabase.FindAssets("t:LayerCollisionConfig");
            if (guids.Length == 0)
            {
                EditorUtility.DisplayDialog(
                    "Load Config Asset",
                    "No LayerCollisionConfig asset found in the project.\n\n" +
                    "Create one via Assets → Create → Best Practices → Layer Collision Config.",
                    "OK");
                return;
            }

            var config = AssetDatabase.LoadAssetAtPath<LayerCollisionConfig>(
                AssetDatabase.GUIDToAssetPath(guids[0]));

            RefreshLayerNames();

            // Start from all-collide, then apply stored rules
            for (int a = 0; a < k_MaxLayers; a++)
                for (int b = 0; b < k_MaxLayers; b++)
                    m_Matrix[a, b] = true;

            foreach (var rule in config.Rules)
            {
                m_Matrix[rule.LayerA, rule.LayerB] = !rule.IgnoreCollision;
                m_Matrix[rule.LayerB, rule.LayerA] = !rule.IgnoreCollision;
            }

            Repaint();
            Debug.Log($"[Best Practices] Layer Collision Matrix: loaded rules from '{config.name}'.");
        }

        private void SaveToConfigAsset()
        {
            string[] guids = AssetDatabase.FindAssets("t:LayerCollisionConfig");
            string assetPath = guids.Length > 0
                ? AssetDatabase.GUIDToAssetPath(guids[0])
                : "Assets/LayerCollisionConfig.asset";

            LayerCollisionConfig config = guids.Length > 0
                ? AssetDatabase.LoadAssetAtPath<LayerCollisionConfig>(assetPath)
                : ScriptableObject.CreateInstance<LayerCollisionConfig>();

            config.Rules.Clear();

            for (int a = 0; a < k_MaxLayers; a++)
            {
                if (string.IsNullOrEmpty(m_LayerNames[a])) continue;

                for (int b = a + 1; b < k_MaxLayers; b++)
                {
                    if (string.IsNullOrEmpty(m_LayerNames[b])) continue;

                    config.Rules.Add(new LayerPairRule
                    {
                        LayerA = a,
                        LayerB = b,
                        IgnoreCollision = !m_Matrix[a, b]
                    });
                }
            }

            if (guids.Length == 0)
                AssetDatabase.CreateAsset(config, assetPath);
            else
                EditorUtility.SetDirty(config);

            AssetDatabase.SaveAssets();
            Debug.Log($"[Best Practices] Layer Collision Matrix: config saved to '{assetPath}'.");
        }

        private void RefreshLayerNames()
        {
            var active = new List<int>();
            for (int i = 0; i < k_MaxLayers; i++)
            {
                m_LayerNames[i] = LayerMask.LayerToName(i);
                if (!string.IsNullOrEmpty(m_LayerNames[i]))
                    active.Add(i);
            }
            m_ActiveLayerIndices = active.ToArray();
        }

        #endregion
    }
}
