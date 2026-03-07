using UnityEngine;

namespace Unity.BestPractices
{
    /// <summary>
    /// ScriptableObject configuration that defines a standard set of tags, sorting layers,
    /// and physics layers to register in the Unity Tag Manager.
    /// </summary>
    /// <remarks>
    /// Create an instance via <b>Assets → Create → Best Practices → Project Tags and Layers</b>.
    /// Then run <b>Window → Best Practices → Setup Tags and Layers</b> to apply the config.
    /// </remarks>
    [CreateAssetMenu(
        fileName = "ProjectTagsAndLayers",
        menuName = "Best Practices/Project Tags and Layers",
        order = 100
    )]
    public class ProjectTagsAndLayers : ScriptableObject
    {
        [Header("Tags")]
        [Tooltip("Custom tags to register. Built-in tags (Untagged, Respawn, Finish, etc.) are always present.")]
        public string[] tags = new string[]
        {
            "Player",
            "Enemy",
            "NPC",
            "Projectile",
            "Pickup",
            "Interactable",
            "Checkpoint",
            "SpawnPoint",
            "Trigger",
            "MainCamera"
        };

        [Header("Sorting Layers")]
        [Tooltip("Sorting layers in render order (bottom to top). 'Default' is always present.")]
        public string[] sortingLayers = new string[]
        {
            "Background",
            "Environment",
            "Props",
            "Characters",
            "Foreground",
            "UI",
            "Overlay"
        };

        [Header("Physics Layers")]
        [Tooltip("Physics layers to register in slots 6–31. Layers 0–5 are reserved by Unity.")]
        public PhysicsLayerEntry[] physicsLayers = new PhysicsLayerEntry[]
        {
            new PhysicsLayerEntry { layerIndex = 6,  layerName = "Player" },
            new PhysicsLayerEntry { layerIndex = 7,  layerName = "Enemy" },
            new PhysicsLayerEntry { layerIndex = 8,  layerName = "NPC" },
            new PhysicsLayerEntry { layerIndex = 9,  layerName = "Projectile" },
            new PhysicsLayerEntry { layerIndex = 10, layerName = "Pickup" },
            new PhysicsLayerEntry { layerIndex = 11, layerName = "Interactable" },
            new PhysicsLayerEntry { layerIndex = 12, layerName = "Ground" },
            new PhysicsLayerEntry { layerIndex = 13, layerName = "Environment" },
            new PhysicsLayerEntry { layerIndex = 14, layerName = "Trigger" },
            new PhysicsLayerEntry { layerIndex = 15, layerName = "Ragdoll" },
        };

        /// <summary>
        /// Represents a single physics layer assignment.
        /// </summary>
        [System.Serializable]
        public struct PhysicsLayerEntry
        {
            [Tooltip("Layer index (6–31). Layers 0–5 are reserved by Unity.")]
            [Range(6, 31)]
            public int layerIndex;

            [Tooltip("Name for this layer.")]
            public string layerName;
        }
    }
}

