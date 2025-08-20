using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Features.Controller
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(FixedStepSimulationSystemGroup))]
    [BurstCompile]
    public partial struct CharacterControllerHierarchyCleanupSystem : ISystem
    {
        private BufferLookup<ChildInstance> _childInstanceLookup;

        public void OnCreate(ref SystemState state)
        {
            _childInstanceLookup = state.GetBufferLookup<ChildInstance>();
        }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            
            state.EntityManager.CompleteDependencyBeforeRW<ChildInstance>();
            _childInstanceLookup.Update(ref state);
            
            
            foreach (var (roInstance, entity) in SystemAPI.Query<RefRO<CharacterInstanceCleanup>>().WithNone<CharacterInstance>().WithEntityAccess())
            {
                if (_childInstanceLookup.TryGetBuffer(roInstance.ValueRO.parent, out var children))
                {
                    for (int i = children.Length - 1; i >= 0; --i)
                    {
                        if (children[i].entity != entity)
                        {
                            continue;
                        }

                        children.RemoveAtSwapBack(i);
                        break;
                    }

                    if (children.Length == 0)
                    {
                        ecb.DestroyEntity(roInstance.ValueRO.parent);
                        continue;
                    }
                }

                ecb.RemoveComponent<CharacterInstanceCleanup>(entity);
            }
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}