using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Features.Input;
using Features.Movement;

namespace Features.Controller
{
    [UpdateInGroup(typeof(GameplayGroup)), UpdateAfter(typeof(InputSystem))]
    [BurstCompile]
    public partial struct PlayerControlSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<InputActionsComponent>();
            state.RequireForUpdate<PlayerControlComponent>();
            state.RequireForUpdate<DirectionComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var inputEntity = SystemAPI.GetSingletonEntity<InputActionsComponent>();
            if (!state.EntityManager.GetComponentDataIfEnabled(inputEntity, out PlayerInputComponent playerInput))
            {
                return;
            }
            
            float3 direction = new float3(playerInput.moveValue.x, playerInput.moveValue.y, 0);
            foreach (var (directionRef, _) in SystemAPI.Query<RefRW<DirectionComponent>, RefRO<PlayerControlComponent>>())
            {
                directionRef.ValueRW.weightedDirection = direction;
            }
        }
    }
}