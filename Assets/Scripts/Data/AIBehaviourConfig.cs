using System;
using Unity.Entities;
using UnityEngine;

namespace Data
{
    [Serializable]
    public struct AIBehaviourConfig : IComponentData
    {
        public float viewRadius;
        public float enemyDirectionWeight;
        [Range(-0.5f, 0.5f)]
        public float enemyProximityExpModifier;
        public float foodDirectionWeight;
        [Range(-0.5f, 0.5f)]
        public float foodProximityExpModifier;
    }
}