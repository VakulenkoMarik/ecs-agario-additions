using System;
using Unity.Entities;

namespace Data
{
    [Serializable]
    public struct GameplayConfig : IComponentData
    {
        public float massToScaleConversion;
        public float massToZAxisConversion;
        public float baseZAxis;
    }
}