using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Features.Controller
{
    [UpdateInGroup(typeof(InitializationSystemGroup), OrderLast = true)]
    [BurstCompile]
    public partial struct CharacterControllerHierarchyInitSystem : ISystem
    {
        private BufferLookup<ChildInstance> _childInstanceLookup;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndInitializationEntityCommandBufferSystem.Singleton>();
            _childInstanceLookup = state.GetBufferLookup<ChildInstance>();
        }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<EndInitializationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
            _childInstanceLookup.Update(ref state);
            
            state.Dependency = new ParentValidationJob
            {
                ecb = ecb,
                childInstanceLookup = _childInstanceLookup,
            }.ScheduleParallel(state.Dependency);
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
                    ecb.RemoveComponent<CharacterInstance>(sortKey, entity);
                    ecb.DestroyEntity(sortKey, entity);
                    return;
                }

                ecb.AddComponent(sortKey, entity, new CharacterInstanceCleanup{parent = instance.parent});
                children.Add(new ChildInstance {entity = entity});
            }
        }
    }
}