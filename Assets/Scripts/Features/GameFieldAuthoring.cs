using Unity.Entities;
using UnityEngine;

namespace Features
{
    public struct GameFieldTag : IComponentData
    {
    }
    
    public class GameFieldAuthoring : MonoBehaviour
    {
        
    }

    public class GameFieldBaker : Baker<GameFieldAuthoring>
    {
        public override void Bake(GameFieldAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent<GameFieldTag>(entity);
        }
    }
    
    
}