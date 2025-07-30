using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

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

    public static void ParentTo(this EntityManager manager, Entity child, Entity parent)
    {
        if (!manager.HasComponent<LocalToWorld>(child))
        {
            manager.AddComponent<LocalToWorld>(child);
        }

        if (manager.HasComponent<Parent>(child))
        {
            manager.SetComponentData(child, new Parent { Value = parent });
        }
        else
        {
            manager.AddComponentData(child, new Parent { Value = parent });   
        }
        
        if (manager.HasComponent<SceneTag>(parent))
        {
            var sceneTag = manager.GetSharedComponent<SceneTag>(parent);
            manager.AddSharedComponent(child, sceneTag);
        }
    }
    
    public static Entity CreateChildEntity(this EntityManager manager, Entity parent)
    {
        var child = manager.CreateEntity(typeof(LocalTransform), typeof(LocalToWorld), typeof(Parent));
        manager.ParentTo(child, parent);

        return child;
    }
}