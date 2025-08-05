using Unity.Entities;
using UnityEngine;

namespace Features.Feed
{
    public struct FeedComponent : IComponentData
    {
        public bool tryToFeed;
        public float cooldown;
        public Entity foodPrefab;
        public float cooldownTimestamp;
    }
    
    public class FeedAuthoring : MonoBehaviour
    {
        public GameObject foodPrefab;
        public float cooldown = 0.5f;
    }
    
    public class FeedBaker : Baker<FeedAuthoring>
    {
        public override void Bake(FeedAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new FeedComponent
            {
                cooldown = authoring.cooldown,
                foodPrefab = GetEntity(authoring.foodPrefab, TransformUsageFlags.Dynamic),
            });
        }
    }
}