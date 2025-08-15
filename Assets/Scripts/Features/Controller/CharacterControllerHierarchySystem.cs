using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Features.Controller
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [BurstCompile]
    public partial struct CharacterControllerHierarchySystem : ISystem
    {
        private ComponentLookup<CharacterInstance> _characterInstanceLookup;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndInitializationEntityCommandBufferSystem.Singleton>();
            _characterInstanceLookup = state.GetComponentLookup<CharacterInstance>();
        }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<EndInitializationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
            _characterInstanceLookup.Update(ref state);
            
            state.Dependency = new ParentValidationJob
            {
                ecb = ecb,
                characterInstanceLookup = _characterInstanceLookup,
            }.ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        public partial struct ParentValidationJob : IJobEntity
        {
            public EntityCommandBuffer.ParallelWriter ecb;
            [NativeDisableParallelForRestriction]
            public ComponentLookup<CharacterInstance> characterInstanceLookup;
            
            [BurstCompile]
            public void Execute([ChunkIndexInQuery] int sortKey, Entity entity, ref DynamicBuffer<ChildInstance> children)
            {
                for (int i = children.Length - 1; i >= 0; i--)
                {
                    var childEntity = children[i].entity;
                    if (!characterInstanceLookup.TryGetComponent(childEntity, out var roCharacterInstance))
                    {
                        children.RemoveAt(i);
                        continue;
                    }

                    if (roCharacterInstance.parent == entity)
                    {
                        continue;
                    }
                    
                    roCharacterInstance.parent = entity;
                    ecb.SetComponent(sortKey, childEntity, roCharacterInstance);
                }
            }
        }
}
}