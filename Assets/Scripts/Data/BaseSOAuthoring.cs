using System;
using Unity.Entities;
using UnityEngine;

namespace Data
{
    public interface ISOAuthoring
    {
        public void Bake(World world);
    }
    
    [Serializable]
    public abstract class BaseSOAuthoring : ScriptableObject, ISOAuthoring
    {
        public abstract void Bake(World world);
    }
}