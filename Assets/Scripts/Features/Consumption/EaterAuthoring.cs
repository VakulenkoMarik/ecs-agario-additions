using Unity.Entities;
using UnityEngine;

namespace Features.Consumption
{
    public struct EaterTag : IComponentData
    {
    }
    
    public class EaterAuthoring : MonoBehaviour
    {
    }
    
    public class EaterBaker : Baker<EaterAuthoring>
    {
        public override void Bake(EaterAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent<EaterTag>(entity);
        }
    }
}