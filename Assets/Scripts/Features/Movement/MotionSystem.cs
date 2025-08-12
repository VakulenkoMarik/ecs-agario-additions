using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;

namespace Features.Movement
{
    [UpdateInGroup(typeof(GameplayGroup)), BurstCompile]
    public partial struct MotionSystem : ISystem
    {
        private const float ComparisonEpsilon = 0.0001f;
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (roDirection, roMotion, rwVelocity) in 
                     SystemAPI.Query<RefRO<DirectionComponent>, RefRO<MotionComponent>, RefRW<PhysicsVelocity>>())
            {
                float3 target = roDirection.ValueRO.direction * roMotion.ValueRO.alteredMaxSpeed;
                if (math.all(math.abs(rwVelocity.ValueRO.Linear - target) < ComparisonEpsilon) || roMotion.ValueRO.accelerationTime <= 0)
                {
                    rwVelocity.ValueRW.Linear = target;
                    continue;
                }

                rwVelocity.ValueRW.Linear = math.lerp(rwVelocity.ValueRW.Linear, target, SystemAPI.Time.DeltaTime / roMotion.ValueRO.accelerationTime);
            }
        }
    }
}