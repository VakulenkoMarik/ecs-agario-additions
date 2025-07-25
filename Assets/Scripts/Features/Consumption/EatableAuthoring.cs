using Unity.Entities;
using UnityEngine;

namespace Features.Consumption
{
    public struct AutoScaleWithMassComponent : IComponentData
    {
        
    }
    
    public struct EatableComponent : IComponentData
    {
        public float mass;
    }
    
    public class EatableAuthoring : MonoBehaviour
    {
        public bool isAutoScale = true;
        public float mass = 1.0f;
    }
    
    public class EatableBaker : Baker<EatableAuthoring>
    {
        public override void Bake(EatableAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new EatableComponent
            {
                mass = authoring.mass,
            });
            
            if (authoring.isAutoScale)
            {
                AddComponent(entity, new AutoScaleWithMassComponent());
            }
        }
    }
}