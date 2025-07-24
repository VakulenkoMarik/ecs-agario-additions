using Unity.Entities;
using UnityEngine;

namespace Features.Controller
{
    public struct PlayerControlComponent : IComponentData
    {
        
    }
    
    public class PlayerControlAuthoring : MonoBehaviour
    {
        
    }

    public class PlayerControlBaker : Baker<PlayerControlAuthoring>
    {
        public override void Bake(PlayerControlAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent<PlayerControlComponent>(entity);
        }
    }
    
}