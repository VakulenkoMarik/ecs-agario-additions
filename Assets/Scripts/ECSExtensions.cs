using Unity.Burst;
using Unity.Entities;

[BurstCompile]
public static class EcsExtensions
{
    public static bool GetComponentDataIfEnabled<T>(this EntityManager manager, Entity entity, out T component) 
        where T : unmanaged, IComponentData, IEnableableComponent
    {
        if (manager.IsComponentEnabled<T>(entity))
        { 
            component = manager.GetComponentData<T>(entity);
            return true;
        }

        component = default;
        return false;
    }
}