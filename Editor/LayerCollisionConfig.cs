using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.BestPractices.Editor
{
    /// <summary>
    /// Represents a single layer-pair collision rule.
    /// </summary>
    [Serializable]
    public struct LayerPairRule
    {
        [Tooltip("Index of the first layer (0–31).")]
        public int LayerA;

        [Tooltip("Index of the second layer (0–31).")]
        public int LayerB;

        [Tooltip("When true the two layers will NOT collide with each other.")]
        public bool IgnoreCollision;
    }

    /// <summary>
    /// Data-driven configuration for Physics layer collision rules.
    /// Create via <b>Assets → Create → Best Practices → Layer Collision Config</b>, then apply
    /// via <b>Window → Best Practices → Layer Collision Matrix</b>.
    /// </summary>
    [CreateAssetMenu(fileName = "LayerCollisionConfig",
        menuName = "Project Configurator/Layer Collision Config",
        order = 11)]
    public class LayerCollisionConfig : ScriptableObject
    {
        [Tooltip("Each entry defines whether two layers should ignore each other's collisions.")]
        public List<LayerPairRule> Rules = new List<LayerPairRule>();
    }
}
