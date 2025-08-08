using Unity.Burst;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;

namespace ProjectTools.Ecs
{
    [UpdateInGroup(typeof(PhysicsSystemGroup), OrderLast = true)]
    [BurstCompile]
    public partial struct PhysicsVelocityConstraintsSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PhysicsConstraints>();
            state.RequireForUpdate<PhysicsVelocity>();
        }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var positionJobHandle = new PositionConstraintsJob().ScheduleParallel(state.Dependency);
            state.Dependency = positionJobHandle;
        }
    }
    
    [BurstCompile]
    public partial struct PositionConstraintsJob : IJobEntity
    {
        [BurstCompile]
        public void Execute(ref PhysicsVelocity velocity, in PhysicsConstraints constraints)
        {
            var freezePosition = constraints.freezePosition;
            if (freezePosition.x) velocity.Linear.x = 0;
            if (freezePosition.y) velocity.Linear.y = 0;
            if (freezePosition.z) velocity.Linear.z = 0;
            
            var freezeRotation = constraints.freezeRotation;
            if (freezeRotation.x) velocity.Angular.x = 0;
            if (freezeRotation.y) velocity.Angular.y = 0;
            if (freezeRotation.z) velocity.Angular.z = 0;
        }
    }
}