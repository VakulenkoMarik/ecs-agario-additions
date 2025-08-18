using System;
using Unity.Entities;
using UnityEngine;

namespace ProjectTools.Ecs
{
    public struct DynamicCollider : IComponentData
    {
        public uint ownLayer;
    }
    
    [InternalBufferCapacity(5)]
    public struct DynamicForcedCollision : IBufferElementData
    {
        public uint withLayer;
        public double cooldown;
    }
    
    public readonly partial struct DynamicCollisionAspect : IAspect
    {
        public readonly RefRW<DynamicCollider> dynamicLayer;
        
        public readonly DynamicBuffer<DynamicForcedCollision> allowedCollisions;
    }

    public class DynamicColliderAuthoring : MonoBehaviour
    {
        [Serializable]
        public class ForcedCollision
        {
            public uint withLayer;
            public double cooldown = -1;
        }
        
        public uint ownLayer;
        
        public ForcedCollision[] forcedCollisions;
    }
    
    public class DynamicColliderBaker : Baker<DynamicColliderAuthoring>
    {
        public override void Bake(DynamicColliderAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new DynamicCollider { ownLayer = authoring.ownLayer });
            
            AddBuffer<DynamicForcedCollision>(entity);
            if (authoring.forcedCollisions == null)
            {
                return;
            }
            
            foreach (DynamicColliderAuthoring.ForcedCollision forcedCollision in authoring.forcedCollisions)
            {
                AppendToBuffer(entity, new DynamicForcedCollision
                {
                    withLayer = forcedCollision.withLayer, 
                    cooldown = forcedCollision.cooldown,
                });
            }
        }
    }
}