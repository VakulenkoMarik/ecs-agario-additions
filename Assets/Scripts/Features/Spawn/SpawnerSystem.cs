using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using Random = Unity.Mathematics.Random;

namespace Features.Spawn
{
    [UpdateInGroup(typeof(InitializationSystemGroup), OrderFirst = true)]
    [BurstCompile]
    public partial struct SpawnerSystem : ISystem
    {
        private const int SpawnAttemptsCount = 5;
        private Random _random;
        private ComponentLookup<LocalTransform> _transformLookup;
        private BufferLookup<Child> _childBufferLookup;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<PhysicsWorldSingleton>();
            _random = new Random((uint)System.DateTime.Now.Ticks);
            _transformLookup = state.GetComponentLookup<LocalTransform>(true);
            _childBufferLookup = state.GetBufferLookup<Child>(true);
        }

        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            InitializeSpawnerPrefabs(ref state);
            
            TrySpawn(ref state);
        }

        private void InitializeSpawnerPrefabs(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            foreach (var (roSpawner, parentEntity) in SystemAPI.Query<RefRO<Spawner>>()
                         .WithChangeFilter<Spawner>().WithEntityAccess())
            {
                ecb.AddComponent(roSpawner.ValueRO.prefab, new Parent {Value = parentEntity});
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
        
        private void TrySpawn(ref SystemState state)
        {
            _transformLookup.Update(ref state);
            _childBufferLookup.Update(ref state);
            var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld;
            
            foreach (var (rwSpawner, roLocalToWorld, entity) in SystemAPI.Query<RefRW<Spawner>, RefRO<LocalToWorld>>().WithEntityAccess())
            {
                if (rwSpawner.ValueRO.nextSpawnTime > SystemAPI.Time.ElapsedTime ||
                    (_childBufferLookup.TryGetBuffer(entity, out var childBuffer) && childBuffer.Length >= rwSpawner.ValueRO.maxCount))
                {
                    continue;
                }

                for (int i = 0; i < SpawnAttemptsCount; i++)
                {
                    if (!_transformLookup.TryGetComponent(rwSpawner.ValueRO.prefab, out var roTransform))
                    {
                        continue;
                    }
                    
                    float3 spawnerPosition = rwSpawner.ValueRO.spawnAnchorPointOffset + new float3(
                        _random.NextFloat(-rwSpawner.ValueRO.spawnZone.x, rwSpawner.ValueRO.spawnZone.x),
                        _random.NextFloat(-rwSpawner.ValueRO.spawnZone.y, rwSpawner.ValueRO.spawnZone.y),
                        _random.NextFloat(-rwSpawner.ValueRO.spawnZone.z, rwSpawner.ValueRO.spawnZone.z));
                    
                    var worldPosition = roLocalToWorld.ValueRO.Position + spawnerPosition;
                    float3 boxHalfExtents = math.max(math.abs(roTransform.Scale) * 1.05f, new float3(0.1f)) * 0.5f;
                    if (physicsWorld.CheckBox(worldPosition, quaternion.identity, boxHalfExtents, CollisionFilter.Default))
                    {
                        continue;
                    }
                    
                    var newEntity = state.EntityManager.Instantiate(rwSpawner.ValueRO.prefab);
                    var localTransform = SystemAPI.GetComponent<LocalTransform>(newEntity);
                    localTransform.Position = spawnerPosition;
                    SystemAPI.SetComponent(newEntity, localTransform);
                    rwSpawner.ValueRW.nextSpawnTime = (float)SystemAPI.Time.ElapsedTime + rwSpawner.ValueRO.spawnRate;
                    break;
                }
            }
        }
    }
}