using Unity.Entities;
using UnityEngine;

namespace Features.Controller
{
    public struct CharacterInstance : IComponentData
    {
        public Entity parent;
    }
    
    public class CharacterInstanceAuthoring : MonoBehaviour
    {
        
    }

    public class CharacterInstanceBaker : Baker<CharacterInstanceAuthoring>
    {
        public override void Bake(CharacterInstanceAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent<CharacterInstance>(entity);
        }
    }
    
}