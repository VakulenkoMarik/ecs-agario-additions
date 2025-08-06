using Data;
using Features.Consumption;
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
        private ComponentLookup<EatableComponent> _eatableLookup;
        private ComponentLookup<PhysicsVelocity> _physicsLookup;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameplayConfig>();
            _transformLookup = state.GetComponentLookup<LocalTransform>(true);
            _eatableLookup = state.GetComponentLookup<EatableComponent>(true);
            _physicsLookup = state.GetComponentLookup<PhysicsVelocity>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            _transformLookup.Update(ref state);
            _eatableLookup.Update(ref state);
            _physicsLookup.Update(ref state);
            var gameplayConfig = SystemAPI.GetSingleton<GameplayConfig>();
            
            foreach (var (rwFeed, rwEatable, roLocalTransform) in 
                     SystemAPI.Query<RefRW<FeedComponent>, RefRW<EatableComponent>, RefRO<LocalTransform>>())
            {
                if (!rwFeed.ValueRO.tryToFeed || rwFeed.ValueRW.cooldownTimestamp > SystemAPI.Time.ElapsedTime)
                {
                    continue;
                }
                
                var foodPrefab = rwFeed.ValueRO.foodPrefab;
                if (!_eatableLookup.TryGetComponent(foodPrefab, out var roPrefabEatable) ||
                    !_physicsLookup.TryGetComponent(foodPrefab, out var rwPhysicsVelocity) ||
                    rwEatable.ValueRO.mass - gameplayConfig.minMass < roPrefabEatable.mass)
                {
                    continue;
                }
                 
                rwEatable.ValueRW.mass -= roPrefabEatable.mass;
                var newEntity = state.EntityManager.Instantiate(foodPrefab);
                var newEntityLocalTransform = SystemAPI.GetComponent<LocalTransform>(newEntity);
                float distanceBetweenCenters = MassScalerSystem.MassToRadius(rwEatable.ValueRO.mass, gameplayConfig.massToScaleConversion) 
                               + MassScalerSystem.MassToRadius(roPrefabEatable.mass, gameplayConfig.massToScaleConversion);
                float3 localPosition = math.normalize(new float3(1f, 1f, 0f)) * distanceBetweenCenters;
                newEntityLocalTransform.Position = roLocalTransform.ValueRO.Position + localPosition;
                SystemAPI.SetComponent(newEntity, newEntityLocalTransform);
                rwPhysicsVelocity.Linear = math.normalize(new float3(1f, 1f, 0f)) * 2;
                SystemAPI.SetComponent(newEntity, rwPhysicsVelocity);
                rwFeed.ValueRW.cooldownTimestamp = (float)SystemAPI.Time.ElapsedTime + rwFeed.ValueRO.cooldown;
                break;
            }
        }
    }
}