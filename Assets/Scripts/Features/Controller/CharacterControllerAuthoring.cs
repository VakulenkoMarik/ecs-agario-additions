using Features.Input;
using Unity.Entities;
using UnityEngine;

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
        public override void Bake(CharacterControllerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new CharacterController{uid = uint.MaxValue});
            AddComponent<GameCommands>(entity);
            switch (authoring.type)
            {
                case ControlType.Player:
                    AddComponent<PlayerControlTag>(entity);
                    break;
                case ControlType.AI:
                    AddComponent<SimpleAIControl>(entity);
                    break;
            }

            AddBuffer<ChildInstance>(entity);
        }
    }
}