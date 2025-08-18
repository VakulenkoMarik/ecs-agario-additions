using Data;
using Features.Consumption;
using Features.Controller;
using Features.Input;
using Features.Movement;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace Features.Abilities.Types
{
    [BurstCompile]
    public sealed partial class FeedRequestSystem : BaseAbilityRequestSystem
    {
        private ComponentLookup<LocalTransform> _transformLookup;
        private ComponentLookup<Eatable> _eatableLookup;
        
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        #region BaseAbilityRequestSystem implementation
        
        public override AbilityType Type => AbilityType.Feed;
        public override TryApplyAbilityRequest StaticApplyRequest => TryApplyRequest;
        
        [BurstCompile]
        public static bool TryApplyRequest(int sortKey, ref EntityCommandBuffer.ParallelWriter ecb, in Ability ability,
            in GameCommands gameCommands, in DynamicBuffer<ChildInstance> children)
        {
            if (!gameCommands.IsFeedPressed)
            {
                return false;
            }

            foreach (var instance in children)
            {
                ecb.SetComponent(sortKey, instance.entity, new FeedRequest{prefab = ability.prefab});
                ecb.SetComponentEnabled<FeedRequest>(sortKey, instance.entity, true);
            }

            return true;
        }
        
        #endregion
        
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        protected override void Create()
        {
            RequireForUpdate<EndInitializationEntityCommandBufferSystem.Singleton>();
            RequireForUpdate<GameplayConfig>();
            
            _transformLookup = GetComponentLookup<LocalTransform>(true);
            _eatableLookup = GetComponentLookup<Eatable>(true);
        }
        
        protected override void OnUpdate()
        {
            var ecbSingleton = World.GetExistingSystemManaged<EndSimulationEntityCommandBufferSystem>();
            var ecb = ecbSingleton.CreateCommandBuffer().AsParallelWriter();
            
            _transformLookup.Update(ref CheckedStateRef);
            _eatableLookup.Update(ref CheckedStateRef);

            Dependency = new RequestJob
            {
                ecb = ecb,
                gameplayConfig = SystemAPI.GetSingleton<GameplayConfig>(),
                transformLookup = _transformLookup,
                eatableLookup = _eatableLookup,
            }.ScheduleParallel(Dependency);
            
            ecbSingleton.AddJobHandleForProducer(Dependency);
        }
        
        
        [BurstCompile]
        [WithNone(typeof(Prefab))]
        private partial struct RequestJob : IJobEntity
        {
            public EntityCommandBuffer.ParallelWriter ecb;
            public GameplayConfig gameplayConfig;
            [ReadOnly]
            public ComponentLookup<LocalTransform> transformLookup;
            [ReadOnly, NativeDisableContainerSafetyRestriction]
            public ComponentLookup<Eatable> eatableLookup;
            
            [BurstCompile]
            public void Execute([ChunkIndexInQuery] int sortKey, Entity entity, ref FeedRequest request, ref Eatable eatable, 
                in LocalTransform localTransform, in Direction direction)
            {
                ecb.SetComponentEnabled<FeedRequest>(sortKey, entity, false);
                
                if (math.all(direction.vector == float3.zero))
                {
                    return;
                }
                
                var prefab = request.prefab;
                if (!eatableLookup.TryGetComponent(prefab, out var prefabEatable) ||
                    !transformLookup.TryGetComponent(prefab, out var prefabLocalTransform) ||
                    eatable.mass - gameplayConfig.minMass < prefabEatable.mass)
                {
                    return;
                }
                
                var newEntity = ecb.Instantiate(sortKey, prefab);
            
                eatable.mass -= prefabEatable.mass;
                var normalizedDirection = math.normalize(direction.vector);
                float distanceBetweenCenters = MassScalerSystem.MassToRadius(eatable.mass, gameplayConfig.massToScaleConversion) 
                                               + MassScalerSystem.MassToRadius(prefabEatable.mass, gameplayConfig.massToScaleConversion);
                
                prefabLocalTransform.Position = localTransform.Position + normalizedDirection * distanceBetweenCenters;
                ecb.SetComponent(sortKey, newEntity, prefabLocalTransform);
                
                ecb.SetComponent(sortKey, newEntity, new PhysicsVelocity{Linear = normalizedDirection * 2});
            }
        }
    }
}