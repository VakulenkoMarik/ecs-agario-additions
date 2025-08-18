using System;
using Unity.Entities;
using UnityEngine;

namespace Data
{
    [Serializable]
    public struct GameplayConfig : IComponentData
    {
        public float massToScaleConversion;
        public float massToZAxisConversion;
        public float baseZAxis;
        
        public float minMass;
        [Range(-0.5f, 0.5f)]
        public float massToSpeedExpModifier;
        
        public double selfEatingCooldown;
    }
}