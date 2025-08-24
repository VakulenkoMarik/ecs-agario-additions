using Data;
using Features.Consumption;
using Features.Input;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace Features.Controller
{
    [UpdateInGroup(typeof(GameplayGroup)), UpdateBefore(typeof(CharacterControllerSystem))]
    [BurstCompile]
    public partial struct SimpleAIControlSystem : ISystem
    {
        private const float RandomTargetUpdateInterval = 2f;
        public NativeList<DistanceHit> _distanceHitsBuffer;
        public Random _random;
        private ComponentLookup<LocalToWorld> _localToWorldLookup;
        private ComponentLookup<Eatable> _eatableLookup;
        private ComponentLookup<CharacterController> _characterControllerControllerLookup;
        private ComponentLookup<CharacterInstance> _characterInstanceLookup;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<AIBehaviourConfig>();
            state.RequireForUpdate<PhysicsWorldSingleton>();
            _random = new Random((uint)System.DateTime.Now.Ticks);
            _distanceHitsBuffer = new NativeList<DistanceHit>(100, Allocator.Persistent);
            _localToWorldLookup = state.GetComponentLookup<LocalToWorld>(true);
            _eatableLookup = state.GetComponentLookup<Eatable>(true);
            _characterControllerControllerLookup = state.GetComponentLookup<CharacterController>(true);
            _characterInstanceLookup = state.GetComponentLookup<CharacterInstance>(true);
        }

        public void OnDestroy(ref SystemState state)
        {
            _distanceHitsBuffer.Dispose();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld;
            var aiBehaviourConfig = SystemAPI.GetSingleton<AIBehaviourConfig>();
            var boxHalfExtents = new float3(aiBehaviourConfig.viewRadius);
            
            state.EntityManager.CompleteDependencyBeforeRO<LocalToWorld>();
            state.EntityManager.CompleteDependencyBeforeRO<ChildInstance>();

            _localToWorldLookup.Update(ref state);
            _eatableLookup.Update(ref state);
            _characterControllerControllerLookup.Update(ref state);
            _characterInstanceLookup.Update(ref state);
            
            foreach (var (rwGameCommands, rwControl, childBuffer, roController) in SystemAPI
                         .Query<RefRW<GameCommands>, RefRW<SimpleAIControl>, DynamicBuffer<ChildInstance>, RefRO<CharacterController>>())
            {
                rwGameCommands.ValueRW.isTargetValid = true;
                rwControl.ValueRW.randomCooldown = math.max(0, rwControl.ValueRO.randomCooldown - SystemAPI.Time.DeltaTime);
                
                var center = float3.zero;
                float averageMass = 0f;  
                for (int i = 0; i < childBuffer.Length; ++i)
                {
                    if (_localToWorldLookup.HasComponent(childBuffer[i].entity))
                    {
                        center += _localToWorldLookup.GetRefRO(childBuffer[i].entity).ValueRO.Position;
                        averageMass += _eatableLookup.GetRefRO(childBuffer[i].entity).ValueRO.mass;
                    }
                }
                
                center /= childBuffer.Length;
                averageMass /= childBuffer.Length;
                var newTargetValue = center.xy;
                
                _distanceHitsBuffer.Clear();
                
                if (physicsWorld.OverlapBox(center, quaternion.identity, boxHalfExtents,
                        ref _distanceHitsBuffer, CollisionFilter.Default))
                {
                    newTargetValue = GetBestTargetPoint(roController.ValueRO.uid, center, averageMass, aiBehaviourConfig).xy;
                }

                if (math.all(newTargetValue == center.xy))
                {
                    if (rwControl.ValueRO.randomCooldown > 0)
                    {
                        continue;
                    }
                    
                    rwControl.ValueRW.randomCooldown += RandomTargetUpdateInterval;
                    float angleRad = _random.NextFloat(0, 360) * math.TORADIANS;
                    newTargetValue = center.xy + new float2(
                        aiBehaviourConfig.viewRadius * math.cos(angleRad), 
                        aiBehaviourConfig.viewRadius * math.sin(angleRad));
                }
                
                rwGameCommands.ValueRW.targetValue = newTargetValue;
            }
        }

        private float3 GetBestTargetPoint(uint uid, float3 center, float averageMass, in AIBehaviourConfig aiBehaviourConfig)
        {
            float3 enemyDirection = float3.zero;
            float3 desiredDirection = float3.zero;
            
            foreach (var hit in _distanceHitsBuffer)
            {
                var entity = hit.Entity;
                if (!_eatableLookup.TryGetComponent(entity, out var eatable))
                {
                    continue;
                }

                if (_characterInstanceLookup.TryGetComponent(entity, out var instance) &&
                    _characterControllerControllerLookup.TryGetComponent(instance.parent, out var controller) &&
                    controller.uid == uid)
                {
                    continue;
                }

                float3 hitPos = _localToWorldLookup.HasComponent(entity) 
                    ? _localToWorldLookup.GetRefRO(entity).ValueRO.Position 
                    : hit.Position;

                float3 dir = hitPos - center;
                float dist = math.length(dir);
                if (dist <= 1e-5f)
                {
                    continue;
                }

                float3 dirNorm = dir / dist;

                if (eatable.mass >= averageMass)
                {
                    float3 nextDir = dirNorm * math.exp(dist * aiBehaviourConfig.enemyProximityExpModifier);
                    enemyDirection += nextDir;
                    Debug.DrawLine(center, center + nextDir, Color.red);
                }
                else
                {
                    float3 nextDir = dirNorm * math.exp(dist * aiBehaviourConfig.foodProximityExpModifier);
                    desiredDirection += nextDir;
                    Debug.DrawLine(center, center + nextDir, Color.blue);
                }
            }

            desiredDirection = desiredDirection * aiBehaviourConfig.foodDirectionWeight -
                               enemyDirection * aiBehaviourConfig.enemyDirectionWeight;
            
            if (math.lengthsq(desiredDirection) > 1e-6f)
            {
                return center + math.normalize(desiredDirection) * aiBehaviourConfig.viewRadius;
            }

            return center;
        }
    }
}