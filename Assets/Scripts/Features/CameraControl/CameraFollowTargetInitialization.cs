using Features.Controller;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;

namespace Features.CameraControl
{
    public partial struct CameraFollowTargetInitialization : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (cameraFollowTarget, targetCharacterEntity) 
                     in SystemAPI.Query<
                         RefRW<CameraFollowTarget>>()
                         .WithPresent<CameraFollowTarget>()
                         .WithAll<CharacterInstance>()
                         .WithEntityAccess())
            {
                
                if (cameraFollowTarget.ValueRO is { activateOnStart: true, beenActivatedObStart: false})
                {
                    cameraFollowTarget.ValueRW.beenActivatedObStart = true;
                    SystemAPI.SetComponentEnabled<CameraFollowTarget>(targetCharacterEntity, true);
                }
            }
        }
    }
}
