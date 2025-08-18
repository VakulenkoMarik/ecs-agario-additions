using Features.Input;
using Unity.Entities;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace Features.Controller
{
    public struct CharacterController : IComponentData
    {
        public uint uid;
    }
    
    public struct ChildInstance : IBufferElementData
    {
        public Entity entity;
    }
    
    public class CharacterControllerAuthoring : MonoBehaviour
    {
        public ControlType type = ControlType.Player;
    }

    public class CharacterControllerGroupBaker : Baker<CharacterControllerAuthoring>
    {
        private static uint _uidCount = 0;
        
        public override void Bake(CharacterControllerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new CharacterController{uid = _uidCount++});
            AddComponent<GameCommands>(entity);
            switch (authoring.type)
            {
                case ControlType.Player:
                    AddComponent<PlayerControlTag>(entity);
                    break;
                case ControlType.AI:
                    AddComponent<SimpleAIControlTag>(entity);
                    break;
            }

            AddBuffer<ChildInstance>(entity);
        }
    }
}