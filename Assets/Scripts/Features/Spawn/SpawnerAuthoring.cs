using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Features.Spawn
{
    public struct Spawner : IComponentData
    {
        public Entity prefab;
        public float3 spawnAnchorPointOffset;
        public float3 spawnZone;
        public float nextSpawnTime;
        public float spawnRate;
    }

    public class SpawnerAuthoring : MonoBehaviour
    {
        public GameObject prefab;
        public float3 spawnAnchorPointOffset;
        public float3 spawnZone;
        public float spawnRate;
    }

    public class SpawnerBaker : Baker<SpawnerAuthoring>
    {
        public override void Bake(SpawnerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Spawner
            {
                prefab = GetEntity(authoring.prefab, TransformUsageFlags.Dynamic),
                spawnAnchorPointOffset = authoring.spawnAnchorPointOffset,
                spawnZone = authoring.spawnZone,
                nextSpawnTime = 0.0f,
                spawnRate = authoring.spawnRate,
            });
        }
    }
}