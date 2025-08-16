using Data;
using Features.Consumption;
using Features.Movement;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using Random = Unity.Mathematics.Random;

namespace Features.Feed
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [BurstCompile]
    public partial struct FeedSpawnerSystem : ISystem
    {
        private Random _random;
        private ComponentLookup<LocalTransform> _transformLookup;
        private ComponentLookup<Eatable> _eatableLookup;
        private ComponentLookup<PhysicsVelocity> _physicsLookup;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameplayConfig>();
            _transformLookup = state.GetComponentLookup<LocalTransform>(true);
            _eatableLookup = state.GetComponentLookup<Eatable>(true);
            _physicsLookup = state.GetComponentLookup<PhysicsVelocity>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            _transformLookup.Update(ref state);
            _eatableLookup.Update(ref state);
            _physicsLookup.Update(ref state);
            var gameplayConfig = SystemAPI.GetSingleton<GameplayConfig>();
            
            foreach (var (rwFeed, rwEatable, roLocalTransform, roDirection) in 
                     SystemAPI.Query<RefRW<Feed>, RefRW<Eatable>, RefRO<LocalTransform>, RefRO<Direction>>())
            {
                if (math.all(roDirection.ValueRO.vector == float3.zero) || !rwFeed.ValueRO.tryToFeed || 
                    rwFeed.ValueRW.cooldownTimestamp > SystemAPI.Time.ElapsedTime)
                {
                    continue;
                }
                
                var foodPrefab = rwFeed.ValueRO.foodPrefab;
                if (!_eatableLookup.TryGetComponent(foodPrefab, out var prefabEatable) ||
                    !_physicsLookup.TryGetComponent(foodPrefab, out var physicsVelocity) ||
                    rwEatable.ValueRO.mass - gameplayConfig.minMass < prefabEatable.mass)
                {
                    continue;
                }
                 
                rwEatable.ValueRW.mass -= prefabEatable.mass;
                var normalizedDirection = math.normalize(roDirection.ValueRO.vector);
                var newEntity = state.EntityManager.Instantiate(foodPrefab);
                var newEntityLocalTransform = SystemAPI.GetComponent<LocalTransform>(newEntity);
                float distanceBetweenCenters = MassScalerSystem.MassToRadius(rwEatable.ValueRO.mass, gameplayConfig.massToScaleConversion) 
                               + MassScalerSystem.MassToRadius(prefabEatable.mass, gameplayConfig.massToScaleConversion);
                float3 localPosition = normalizedDirection * distanceBetweenCenters;
                newEntityLocalTransform.Position = roLocalTransform.ValueRO.Position + localPosition;
                SystemAPI.SetComponent(newEntity, newEntityLocalTransform);
                physicsVelocity.Linear = normalizedDirection * 2;
                SystemAPI.SetComponent(newEntity, physicsVelocity);
                rwFeed.ValueRW.cooldownTimestamp = (float)SystemAPI.Time.ElapsedTime + rwFeed.ValueRO.cooldown;
            }
        }
    }
}