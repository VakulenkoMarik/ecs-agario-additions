using Unity.Entities;
using UnityEngine;

namespace Features.Split
{
    public struct Splittable : IComponentData
    {
        public bool tryToSplit;
        public float cooldown;
        public Entity instancePrefab;
        public float cooldownTimestamp;
    }
    
    
    public class SplittableAuthoring : MonoBehaviour
    {
        public float cooldown;
        public GameObject instancePrefab;
    }

    public class SplittableBaker : Baker<SplittableAuthoring>
    {
        public override void Bake(SplittableAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Splittable
            {
                tryToSplit = false,
                cooldown = authoring.cooldown,
                instancePrefab = GetEntity(authoring.instancePrefab, TransformUsageFlags.Dynamic),
                cooldownTimestamp = 0.0f,
            });
        }
    }
}