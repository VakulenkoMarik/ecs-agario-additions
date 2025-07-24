using System;
using Unity.Entities;

namespace Data
{
    [Serializable]
    public struct GameplayConfig : IComponentData
    {
        public float data;
    }
}