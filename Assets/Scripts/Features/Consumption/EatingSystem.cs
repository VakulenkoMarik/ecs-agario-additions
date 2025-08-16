using Data;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;

namespace Features.Consumption
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(PhysicsSystemGroup))]
    [BurstCompile]
    public partial struct EatingSystem : ISystem
    {
        private ComponentLookup<EaterTag> _eaterLookup;
        private ComponentLookup<Eatable> _eatableLookup;
        private ComponentLookup<LocalTransform> _transformLookup;
        
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndFixedStepSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<SimulationSingleton>();
            state.RequireForUpdate<GameplayConfig>();
            
            _eaterLookup = state.GetComponentLookup<EaterTag>(true);
            _transformLookup = state.GetComponentLookup<LocalTransform>(true);
            _eatableLookup = state.GetComponentLookup<Eatable>(false);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<EndFixedStepSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);
            
            _eaterLookup.Update(ref state);
            _transformLookup.Update(ref state);
            _eatableLookup.Update(ref state);

            state.Dependency = new EatingTriggerJob
            {
                gameplayConfig = SystemAPI.GetSingleton<GameplayConfig>(),
                eaterLookup = _eaterLookup,
                transformLookup = _transformLookup,
                eatableLookup = _eatableLookup,
                ecb = ecb,
            }.Schedule(
                SystemAPI.GetSingleton<SimulationSingleton>(),
                state.Dependency
            );
        }
    }
    
    [BurstCompile]
    public struct EatingTriggerJob : ITriggerEventsJob
    {
        [ReadOnly] public GameplayConfig gameplayConfig;
        [ReadOnly] public ComponentLookup<EaterTag> eaterLookup;
        [ReadOnly] public ComponentLookup<LocalTransform> transformLookup;
        [NativeDisableParallelForRestriction]
        public ComponentLookup<Eatable> eatableLookup;
        public EntityCommandBuffer ecb;
    
        [BurstCompile]
        public void Execute(TriggerEvent triggerEvent)
        {
            var entityA = triggerEvent.EntityA;
            var entityB = triggerEvent.EntityB;
        
            if (eaterLookup.HasComponent(entityA) && TryEat(entityA, entityB))
            {
                return;
            }
        
            if (eaterLookup.HasComponent(entityB))
            {
                TryEat(entityB, entityA);
            }
        }
        
        private bool TryEat(Entity eater, Entity target)
        {
            if (!eatableLookup.TryGetComponent(eater, out var eaterEatable) ||
                !eatableLookup.TryGetComponent(target, out var targetEatable) ||
                !transformLookup.TryGetComponent(eater, out var eaterTransform) ||
                !transformLookup.TryGetComponent(target, out var targetTransform))
            {
                return false;
            }
            
            float eaterRadius = MassScalerSystem.MassToRadius(eaterEatable.mass, gameplayConfig.massToScaleConversion);
            float distance = math.distance(eaterTransform.Position.xy, targetTransform.Position.xy);
            if (distance > eaterRadius || eaterEatable.mass <= targetEatable.mass)
            {
                return false;
            }
            
            eaterEatable.mass += targetEatable.mass;
            eatableLookup[eater] = eaterEatable;
            ecb.DestroyEntity(target);
            
            return true;
        }
    }

}