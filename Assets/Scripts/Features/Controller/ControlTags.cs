using Unity.Entities;

namespace Features.Controller
{
    public enum ControlType
    {
        Player,
        AI,
    }
    
    public struct PlayerControlTag : IComponentData
    {
        
    }
    
    public struct SimpleAIControlTag : IComponentData
    {
        
    }
}