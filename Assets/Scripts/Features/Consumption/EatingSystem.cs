using Data;
using Features.Consumption.EatingEvents;
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
    public partial class EatingSystem : SystemBase
    {
        private ComponentLookup<EaterTag> _eaterLookup;
        private ComponentLookup<Eatable> _eatableLookup;
        private ComponentLookup<LocalTransform> _transformLookup;
        
        protected override void OnCreate()
        {
            RequireForUpdate<EndFixedStepSimulationEntityCommandBufferSystem.Singleton>();
            RequireForUpdate<SimulationSingleton>();
            RequireForUpdate<EatenEventTag>();
            RequireForUpdate<GameplayConfig>();
            
            _eaterLookup = GetComponentLookup<EaterTag>(true);
            _transformLookup = GetComponentLookup<LocalTransform>(true);
            _eatableLookup = GetComponentLookup<Eatable>(false);
        }
        
        protected override void OnUpdate()
        {
            _eaterLookup.Update(ref CheckedStateRef);
            _transformLookup.Update(ref CheckedStateRef);
            _eatableLookup.Update(ref CheckedStateRef);

            var ecbSingleton = World.GetExistingSystemManaged<EndFixedStepSimulationEntityCommandBufferSystem>();
            var ecb = ecbSingleton.CreateCommandBuffer().AsParallelWriter();
            var bufferEntity = SystemAPI.GetSingletonEntity<EatenEventTag>();

            var gameplayConfig = SystemAPI.GetSingleton<GameplayConfig>();

            Dependency = new EatingTriggerJob
            {
                gameplayConfig = gameplayConfig,
                eaterLookup = _eaterLookup,
                transformLookup = _transformLookup,
                eatableLookup = _eatableLookup,
                ecb = ecb,
                bufferEntity = bufferEntity
            }.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), Dependency);

            ecbSingleton.AddJobHandleForProducer(Dependency);
        }
    }
    
    [BurstCompile]
    public struct EatingTriggerJob : ITriggerEventsJob
    {
        [ReadOnly] public GameplayConfig gameplayConfig;
        [ReadOnly] public ComponentLookup<EaterTag> eaterLookup;
        [ReadOnly] public ComponentLookup<LocalTransform> transformLookup;
        [NativeDisableParallelForRestriction] public ComponentLookup<Eatable> eatableLookup;
        
        public EntityCommandBuffer.ParallelWriter ecb;
        
        [ReadOnly] public Entity bufferEntity;
    
        [BurstCompile]
        public void Execute(TriggerEvent triggerEvent)
        {
            var entityA = triggerEvent.EntityA;
            var entityB = triggerEvent.EntityB;
        
            if (eaterLookup.HasComponent(entityA) && TryEat(entityA, entityB, triggerEvent.BodyIndexB))
            {
                return;
            }
        
            if (eaterLookup.HasComponent(entityB))
            {
                TryEat(entityB, entityA, triggerEvent.BodyIndexB);
            }
        }
        
        private bool TryEat(Entity eater, Entity target, int sortKey)
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

            ecb.AppendToBuffer(sortKey, bufferEntity, new EatenEvent { eater = eater, victim = target });
            
            return true;
        }
    }
}