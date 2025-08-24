using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Features.Controller
{
    [UpdateInGroup(typeof(InitializationSystemGroup)), UpdateBefore(typeof(EndInitializationEntityCommandBufferSystem))]
    [BurstCompile]
    public partial class CharacterControllerHierarchyInitSystem : SystemBase
    {
        public static uint uidCounter = 0;
        private BufferLookup<ChildInstance> _childInstanceLookup;

        protected override void OnCreate()
        {
            RequireForUpdate<EndInitializationEntityCommandBufferSystem.Singleton>();
            _childInstanceLookup = GetBufferLookup<ChildInstance>();
        }
        
        protected override void OnUpdate()
        {
            var ecbSingleton = World.GetExistingSystemManaged<EndInitializationEntityCommandBufferSystem>();
            var ecb = ecbSingleton.CreateCommandBuffer().AsParallelWriter(); 

            EntityManager.CompleteDependencyBeforeRW<ChildInstance>();
            _childInstanceLookup.Update(ref CheckedStateRef);
            
            Dependency = Entities
                .WithoutBurst()
                .WithChangeFilter<CharacterController>()
                .ForEach((ref CharacterController controller) =>
                {
                    if (controller.uid == uint.MaxValue)
                    {
                        controller.uid = uidCounter++;
                    }
                })
                .Schedule(Dependency);
            
            Dependency = new ParentValidationJob
            {
                ecb = ecb,
                childInstanceLookup = _childInstanceLookup,
            }.ScheduleParallel(Dependency);
            ecbSingleton.AddJobHandleForProducer(Dependency);
        }

        [BurstCompile]
        [WithChangeFilter(typeof(CharacterInstance))]
        public partial struct ParentValidationJob : IJobEntity
        {
            public EntityCommandBuffer.ParallelWriter ecb;
            [NativeDisableParallelForRestriction]
            public BufferLookup<ChildInstance> childInstanceLookup;
            
            [BurstCompile]
            public void Execute([ChunkIndexInQuery] int sortKey, Entity entity, in CharacterInstance instance)
            {
                if (!childInstanceLookup.TryGetBuffer(instance.parent, out var children))
                {
                    Debug.LogError($"Entity {entity} has no valid parent");
                    ecb.DestroyEntity(sortKey, entity);
                    return;
                }

                ecb.AddComponent(sortKey, entity, new CharacterInstanceCleanup{parent = instance.parent});
                children.Add(new ChildInstance {entity = entity});
            }
        }
    }
}