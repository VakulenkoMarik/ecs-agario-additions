using Data;
using Features.Consumption;
using Features.Controller;
using Features.Input;
using Features.Movement;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace Features.Abilities.Types
{
    [BurstCompile]
    public sealed partial class SplitRequestSystem : BaseAbilityRequestSystem
    {

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        #region BaseAbilityRequestSystem implementation

        public override AbilityType Type => AbilityType.Split;
        public override TryApplyAbilityRequest StaticApplyRequest => TryApplyRequest;
        
        [BurstCompile]
        public static bool TryApplyRequest(int sortKey, ref EntityCommandBuffer.ParallelWriter ecb, in Ability ability,
            in GameCommands gameCommands, in DynamicBuffer<ChildInstance> children)
        {
            if (!gameCommands.IsJumpPressed)
            {
                return false;
            }

            foreach (var instance in children)
            {
                ecb.SetComponent(sortKey, instance.entity, new SplitRequest {prefab = ability.prefab});
                ecb.SetComponentEnabled<SplitRequest>(sortKey, instance.entity, true);
            }

            return true;
        }

        #endregion
        
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        protected override void Create()
        {
            RequireForUpdate<EndInitializationEntityCommandBufferSystem.Singleton>();
            RequireForUpdate<GameplayConfig>();
        }
        
        protected override void OnUpdate()
        {
            var ecbSingleton = World.GetExistingSystemManaged<EndSimulationEntityCommandBufferSystem>();
            var ecb = ecbSingleton.CreateCommandBuffer().AsParallelWriter();

            Dependency = new RequestJob
            {
                ecb = ecb,
                gameplayConfig = SystemAPI.GetSingleton<GameplayConfig>(),
            }.ScheduleParallel(Dependency);
            
            ecbSingleton.AddJobHandleForProducer(Dependency);
        }
        
        
        [BurstCompile]
        [WithNone(typeof(Prefab))]
        private partial struct RequestJob : IJobEntity
        {
            public EntityCommandBuffer.ParallelWriter ecb;
            public GameplayConfig gameplayConfig;
        
            [BurstCompile]
            public void Execute([ChunkIndexInQuery] int sortKey, Entity entity, ref SplitRequest request, ref Eatable eatable, 
                in LocalTransform localTransform, in Direction direction, in CharacterInstance instance)
            {
                ecb.SetComponentEnabled<SplitRequest>(sortKey, entity, false);
            
                if (math.all(direction.vector == float3.zero))
                {
                    return;
                }
            
                var prefab = request.prefab;
                if (eatable.mass * 0.5f < gameplayConfig.minMass)
                {
                    return;
                }
            
                var newEntity = ecb.Instantiate(sortKey, prefab);
            
                eatable.mass *= 0.5f;
                ecb.SetComponent(sortKey, newEntity, new Eatable{mass = eatable.mass});
            
                var normalizedDirection = math.normalize(direction.vector);
                float distanceBetweenCenters = 2 * MassScalerSystem.MassToRadius(eatable.mass, gameplayConfig.massToScaleConversion);
            
                var newLocalTransform = localTransform;
                newLocalTransform.Position = localTransform.Position + normalizedDirection * distanceBetweenCenters;
                ecb.SetComponent(sortKey, newEntity, newLocalTransform);
            
                ecb.SetComponent(sortKey, newEntity, new PhysicsVelocity{Linear = normalizedDirection * 2});;
            
                ecb.SetComponent(sortKey, newEntity, new CharacterInstance{parent = instance.parent});
            }
        }
    }
    
    
}