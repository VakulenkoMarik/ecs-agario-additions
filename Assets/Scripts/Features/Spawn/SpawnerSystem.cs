using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Features.Spawn
{
    [UpdateInGroup(typeof(GameplayGroup), OrderLast = true)]
    [BurstCompile]
    public partial struct SpawnerSystem : ISystem
    {
        private Random _random;
        
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            _random = new Random((uint)System.DateTime.Now.Ticks);
        }

        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);

            
            foreach (var (rwSpawner, entity) in SystemAPI.Query<RefRW<Spawner>>().WithEntityAccess())
            {
                if (rwSpawner.ValueRO.nextSpawnTime < SystemAPI.Time.ElapsedTime)
                {
                    var newEntity = ecb.Instantiate(rwSpawner.ValueRO.prefab);
                    ecb.ParentTo(state.EntityManager, newEntity, entity);
                    
                    float3 spawnerPosition = rwSpawner.ValueRO.spawnAnchorPointOffset;
                    if (rwSpawner.ValueRO.spawnZone.x != 0)
                    {
                        spawnerPosition.x += _random.NextFloat(-rwSpawner.ValueRO.spawnZone.x, rwSpawner.ValueRO.spawnZone.x);
                    }

                    if (rwSpawner.ValueRO.spawnZone.y != 0)
                    {
                        spawnerPosition.y += _random.NextFloat(-rwSpawner.ValueRO.spawnZone.y, rwSpawner.ValueRO.spawnZone.y);
                    }

                    if (rwSpawner.ValueRO.spawnZone.z != 0)
                    {
                        spawnerPosition.z += _random.NextFloat(-rwSpawner.ValueRO.spawnZone.z, rwSpawner.ValueRO.spawnZone.z);
                    }
                    
                    ecb.SetComponent(newEntity, LocalTransform.FromPosition(spawnerPosition));

                    rwSpawner.ValueRW.nextSpawnTime = (float)SystemAPI.Time.ElapsedTime + rwSpawner.ValueRO.spawnRate;
                }
            }
        }
    }
}