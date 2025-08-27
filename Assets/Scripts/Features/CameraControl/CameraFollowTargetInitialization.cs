using Features.Controller;
using Unity.Burst;
using Unity.Entities;

namespace Features.CameraControl
{
    public partial struct CameraFollowTargetInitialization : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (characterInstance, targetCharacterEntity) 
                     in SystemAPI.Query< 
                         RefRO<CharacterInstance>>()
                         .WithPresent<CameraFollowTarget>()
                         .WithEntityAccess())
            {
                
                if (SystemAPI.HasComponent<PlayerControlTag>(characterInstance.ValueRO.parent))
                {
                    SystemAPI.SetComponentEnabled<CameraFollowTarget>(targetCharacterEntity, true);
                    state.Enabled = false;
                }
                
            }
        }
    }
}
