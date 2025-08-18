using Features.Abilities.Types;
using Unity.Entities;

namespace Features.Abilities
{
    [UpdateInGroup(typeof(InitializationSystemGroup)), CreateAfter(typeof(AbilityHandlerSystem))]
    public abstract partial class BaseAbilityRequestSystem : SystemBase
    {
        public abstract AbilityType Type { get; }
        public abstract TryApplyAbilityRequest StaticApplyRequest { get; }

        protected sealed override void OnCreate()
        {
            Create();
            World.GetOrCreateSystemManaged<AbilityHandlerSystem>().AddAbility(this);
        }

        protected virtual void Create()
        {
            
        }
    }
}