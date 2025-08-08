using Unity.Entities;
using UnityEngine;

namespace ProjectTools.Ecs
{
    public struct DynamicCollider : IComponentData
    {
        public uint ownLayer;
    }
    
    [InternalBufferCapacity(5)]
    public struct DynamicAllowedCollision : IBufferElementData
    {
        public uint withLayer;
    }
    
    public readonly partial struct DynamicCollisionAspect : IAspect
    {
        public readonly RefRW<DynamicCollider> dynamicLayer;
        
        public readonly DynamicBuffer<DynamicAllowedCollision> allowedCollisions;
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
            
            AddBuffer<DynamicAllowedCollision>(entity);
            if (authoring.allowedCollisions == null)
            {
                return;
            }
            
            foreach (uint allowedCollision in authoring.allowedCollisions)
            {
                AppendToBuffer(entity, new DynamicAllowedCollision{ withLayer = allowedCollision });
            }
        }
    }
}