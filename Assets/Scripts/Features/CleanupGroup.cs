using Unity.Entities;

namespace Features
{
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true),
    UpdateAfter(typeof(EndSimulationEntityCommandBufferSystem))]
    public partial class CleanupGroup : ComponentSystemGroup
    {
        
    }
}