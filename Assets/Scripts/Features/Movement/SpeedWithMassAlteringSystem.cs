using Data;
using Features.Consumption;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace Features.Movement
{
    [UpdateInGroup(typeof(GameplayGroup)), UpdateBefore(typeof(MotionSystem)), BurstCompile]
    public partial struct SpeedWithMassAlteringSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameplayConfig>();
        }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var gameplayConfig = SystemAPI.GetSingleton<GameplayConfig>();
            
            foreach (var (rwMotion, roEatable) in SystemAPI.Query<RefRW<MotionComponent>, RefRO<EatableComponent>>())
            {
                if (roEatable.ValueRO.mass <= gameplayConfig.minMass)
                {
                    rwMotion.ValueRW.alteredMaxSpeed = rwMotion.ValueRO.maxSpeed;
                }
                else
                {
                    rwMotion.ValueRW.alteredMaxSpeed = rwMotion.ValueRO.maxSpeed * math.exp(
                        gameplayConfig.massToSpeedExpModifier * (roEatable.ValueRO.mass - gameplayConfig.minMass));
                }
            }
        }
    }
}