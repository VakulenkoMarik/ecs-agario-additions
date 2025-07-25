using Data;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Features.Consumption
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(EatingSystem))]
    [BurstCompile]
    public partial struct MassScalerSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameplayConfig>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var gameplayConfig = SystemAPI.GetSingleton<GameplayConfig>();
            
            foreach (var (rwTransform, roEatable) in SystemAPI
                         .Query<RefRW<LocalTransform>, RefRO<EatableComponent>>()
                         .WithAll<AutoScaleWithMassComponent>()
                         .WithChangeFilter<EatableComponent>())
            {
                rwTransform.ValueRW.Scale = MassToRadius(roEatable.ValueRO.mass, gameplayConfig.massToScaleConversion) * 2f;
                rwTransform.ValueRW.Position.z = gameplayConfig.baseZAxis - roEatable.ValueRO.mass * gameplayConfig.massToZAxisConversion;
            }
        }
        
        public static float MassToRadius(float mass, float massToScaleRatio)
        {
            return massToScaleRatio * math.sqrt(mass/math.PI);
        }
    }
}