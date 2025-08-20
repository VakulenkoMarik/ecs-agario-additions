using System;
using Unity.Entities;
using UnityEngine;

namespace Features.Controller
{
    public struct CharacterInstance : IComponentData
    {
        public Entity parent;
    }
    
    public struct CharacterInstanceCleanup : ICleanupComponentData
    {
        public Entity parent;
    }
    
    public class CharacterInstanceAuthoring : MonoBehaviour
    {
        public CharacterControllerAuthoring parent;
    }

    public class CharacterInstanceBaker : Baker<CharacterInstanceAuthoring>
    {
        public override void Bake(CharacterInstanceAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            var parentEntity = GetEntity(authoring.parent, TransformUsageFlags.None);
            AddComponent(entity, new CharacterInstance{parent = parentEntity});
        }
    }
    
}