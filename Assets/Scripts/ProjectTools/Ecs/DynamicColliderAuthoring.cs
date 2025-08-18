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
        
    }
    
    public readonly partial struct DynamicCollisionAspect : IAspect
    {
        public readonly RefRW<DynamicCollider> dynamicLayer;
        
        public readonly DynamicBuffer<DynamicForcedCollision> allowedCollisions;
    }

    public class DynamicColliderAuthoring : MonoBehaviour
    {
        public uint ownLayer;
        
        public uint[] allowedCollisions;
    }
    
    public class DynamicColliderBaker : Baker<DynamicColliderAuthoring>
    {
        public override void Bake(DynamicColliderAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new DynamicCollider { ownLayer = authoring.ownLayer });
            
            AddBuffer<DynamicForcedCollision>(entity);
            if (authoring.allowedCollisions == null)
            {
                return;
            }
            
            foreach (uint allowedCollision in authoring.allowedCollisions)
            {
                AppendToBuffer(entity, new DynamicForcedCollision{ withLayer = allowedCollision });
            }
        }
    }
}