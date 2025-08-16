using Features.Input;
using Unity.Entities;
using UnityEngine;

namespace Features.Controller
{
    public struct CharacterController : IComponentData
    {
        public int uid;
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
        public override void Bake(CharacterControllerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent<CharacterController>(entity);
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