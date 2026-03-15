using System.Collections.Generic;
using UnityEngine;

namespace Unity.BestPractices.Editor
{
    /// <summary>
    /// Defines which custom Tags and Layers to add to the project.
    /// Create via <b>Assets → Create → Best Practices → Tags And Layers Config</b>, then apply
    /// via <b>Window → Best Practices → Setup Tags and Layers</b>.
    /// </summary>
    [CreateAssetMenu(fileName = "TagsAndLayersConfig",
        menuName = "Project Configurator/Tags And Layers Config",
        order = 10)]
    public class TagsAndLayersConfig : ScriptableObject
    {
        [Header("Tags to add (built-in tags are skipped automatically)")]
        public List<string> Tags = new List<string>
        {
            "Player", "Enemy", "Interactable", "Pickup", "Projectile"
        };

        [Header("Layers to add (user layers 8–31; built-in layers 0–7 are never overwritten)")]
        public List<string> Layers = new List<string>
        {
            "Player", "Enemy", "Ground", "Projectile", "Interactable", "UI_World"
        };
    }
}
