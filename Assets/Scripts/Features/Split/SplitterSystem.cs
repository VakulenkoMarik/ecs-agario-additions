using Data;
using Features.Consumption;
using Features.Controller;
using Features.Movement;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace Features.Split
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [BurstCompile]
    public partial struct SplitterSystem : ISystem
    {
        private const int SpawnAttemptsCount = 5;
        private ComponentLookup<LocalTransform> _transformLookup;
        private BufferLookup<Child> _childBufferLookup;
        private ComponentLookup<Eatable> _eatableLookup;
        private ComponentLookup<PhysicsVelocity> _physicsLookup;
        private ComponentLookup<CharacterInstance> _characterInstanceLookup;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameplayConfig>();
            _transformLookup = state.GetComponentLookup<LocalTransform>(true);
            _eatableLookup = state.GetComponentLookup<Eatable>();
            _physicsLookup = state.GetComponentLookup<PhysicsVelocity>();
            _characterInstanceLookup = state.GetComponentLookup<CharacterInstance>();
        }

        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            _transformLookup.Update(ref state);
            _eatableLookup.Update(ref state);
            _physicsLookup.Update(ref state);
            _characterInstanceLookup.Update(ref state);
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            var gameplayConfig = SystemAPI.GetSingleton<GameplayConfig>();
            
            foreach (var (rwSplittable, rwEatable, roCharacterInstance, roLocalTransform, roDirection) in 
                     SystemAPI.Query<RefRW<Splittable>, RefRW<Eatable>, RefRO<CharacterInstance>, 
                         RefRO<LocalTransform>, RefRO<Direction>>())
            {
                if (math.all(roDirection.ValueRO.vector == float3.zero) || !rwSplittable.ValueRO.tryToSplit || 
                    rwSplittable.ValueRW.cooldownTimestamp > SystemAPI.Time.ElapsedTime)
                {
                    continue;
                }

                var instancePrefab = rwSplittable.ValueRO.instancePrefab;
                if (!_eatableLookup.TryGetComponent(instancePrefab, out var prefabEatable) ||
                    !_transformLookup.TryGetComponent(instancePrefab, out var prefabLocalTransform) ||
                    !_physicsLookup.TryGetComponent(instancePrefab, out var prefabPhysics) ||
                    !_characterInstanceLookup.TryGetComponent(instancePrefab, out var prefabCharacterInstance) ||
                    rwEatable.ValueRO.mass * 0.5f < gameplayConfig.minMass)
                {
                    continue;
                }


                var newEntity = ecb.Instantiate(instancePrefab);
                
                rwEatable.ValueRW.mass *= 0.5f;
                prefabEatable.mass = rwEatable.ValueRO.mass;
                ecb.SetComponent(newEntity, prefabEatable);
                
                var normalizedDirection = math.normalize(roDirection.ValueRO.vector);
                float distanceBetweenCenters = 2 * MassScalerSystem.MassToRadius(rwEatable.ValueRO.mass, gameplayConfig.massToScaleConversion);
                
                float3 localPosition = normalizedDirection * distanceBetweenCenters;
                prefabLocalTransform.Position = roLocalTransform.ValueRO.Position + localPosition;
                ecb.SetComponent(newEntity, prefabLocalTransform);
                
                prefabPhysics.Linear = normalizedDirection * 2;
                ecb.SetComponent(newEntity, prefabPhysics);
                
                prefabCharacterInstance.parent = roCharacterInstance.ValueRO.parent;
                ecb.SetComponent(newEntity, prefabCharacterInstance);
                
                rwSplittable.ValueRW.cooldownTimestamp = (float)SystemAPI.Time.ElapsedTime + rwSplittable.ValueRO.cooldown;
                break;
            }
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}