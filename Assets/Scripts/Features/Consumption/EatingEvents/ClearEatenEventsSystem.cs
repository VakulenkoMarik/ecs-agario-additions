using Unity.Entities;

namespace Features.Consumption.EatingEvents
{
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
    public partial struct ClearEatenEventsSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EatenEventTag>();
        }
        
        public void OnUpdate(ref SystemState state)
        {
            var bufferEntity = SystemAPI.GetSingletonEntity<EatenEventTag>();
            var buffer = SystemAPI.GetBuffer<EatenEvent>(bufferEntity);

            if (buffer.IsEmpty)
                return;
            
            var ecbSingleton = state.World.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>();
            var ecb = ecbSingleton.CreateCommandBuffer();
            
            foreach (var e in buffer)
            {
                ecb.DestroyEntity(e.victim);
            }
            
            ecb.SetBuffer<EatenEvent>(bufferEntity).Clear();
        }
    }
}