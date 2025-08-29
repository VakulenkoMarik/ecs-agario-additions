using Unity.Entities;
using UnityEngine;

namespace Features.Consumption.EatingEvents
{
    public struct EatenEvent : IBufferElementData
    {
        public Entity eater;
        public Entity victim;
    }

    public struct EatenEventTag : IComponentData
    {
        
    }
    
    public class EatenEventAuthoring : MonoBehaviour
    {
        public class Baker : Baker<EatenEventAuthoring>
        {
            public override void Bake(EatenEventAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent<EatenEventTag>(entity);
                AddBuffer<EatenEvent>(entity);
            }
        }
    }
}
