using Unity.Entities;
using UnityEngine;

namespace Features.Abilities.Types
{
    public enum AbilityType : int
    {
        Feed,
        Split,
    }
    
    public struct FeedRequest : IComponentData, IEnableableComponent
    {
        public Entity prefab;
    }
    
    public struct SplitRequest : IComponentData, IEnableableComponent
    {
        public Entity prefab;
    }
    
    public class AbilitiesRequestsAuthoring : MonoBehaviour
    {
        public bool isFeedAllowed;
        public bool isSplitAllowed;
    }
    
    public class AbilitiesRequestsBaker : Baker<AbilitiesRequestsAuthoring>
    {
        public override void Bake(AbilitiesRequestsAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            if (authoring.isFeedAllowed)
            {
                AddComponent<FeedRequest>(entity);
                SetComponentEnabled<FeedRequest>(entity, false);
            }
            
            if (authoring.isSplitAllowed)
            {
                AddComponent<SplitRequest>(entity);
                SetComponentEnabled<SplitRequest>(entity, false);
            }
        }
    }
}