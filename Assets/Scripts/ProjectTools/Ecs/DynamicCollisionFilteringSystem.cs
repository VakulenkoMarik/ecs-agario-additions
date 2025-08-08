using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;

namespace ProjectTools.Ecs
{
    [UpdateInGroup(typeof(PhysicsCreateContactsGroup), OrderLast = true)]
    [BurstCompile]
    public partial struct DynamicCollisionFilteringSystem : ISystem
    {
        private ComponentLookup<DynamicCollider> _dynamicColliderLookup;
        private BufferLookup<DynamicAllowedCollision> _dynamicCollisionLookup;
        private ComponentLookup<PhysicsCollider> _physicsColliderLookup;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SimulationSingleton>();
            state.RequireForUpdate<PhysicsWorldSingleton>();

            _dynamicColliderLookup = state.GetComponentLookup<DynamicCollider>();
            _dynamicCollisionLookup = state.GetBufferLookup<DynamicAllowedCollision>();
            _physicsColliderLookup = state.GetComponentLookup<PhysicsCollider>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld;

            _dynamicColliderLookup.Update(ref state);
            _dynamicCollisionLookup.Update(ref state);
            _physicsColliderLookup.Update(ref state);

            state.Dependency = new CollisionOverrideJob
            {
                dynamicColliderLookup = _dynamicColliderLookup,
                dynamicCollisionLookup = _dynamicCollisionLookup,
                physicsColliderLookup = _physicsColliderLookup,
            }.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), ref physicsWorld, state.Dependency);
        }
    }
    
    
    [BurstCompile]
    public struct CollisionOverrideJob : IContactsJob
    {
        [ReadOnly] public ComponentLookup<DynamicCollider> dynamicColliderLookup;
        [ReadOnly] public BufferLookup<DynamicAllowedCollision> dynamicCollisionLookup;
        [ReadOnly] public ComponentLookup<PhysicsCollider> physicsColliderLookup;

        [BurstCompile]
        public void Execute(ref ModifiableContactHeader header, ref ModifiableContactPoint contact)
        {
            var entityA = header.EntityA;
            var entityB = header.EntityB;

            if ((header.JacobianFlags &= JacobianFlags.IsTrigger) == 0)
            {
                return;
            }

            if (IsColliding(entityA, entityB) || IsColliding(entityB, entityA))
            {
                header.JacobianFlags &= ~JacobianFlags.IsTrigger;
                header.JacobianFlags |= JacobianFlags.EnableCollisionEvents;
            }
        }
        
        private bool IsColliding(in Entity entityA, in Entity entityB)
        {
            if (!dynamicColliderLookup.TryGetComponent(entityA, out var dynamicColliderA))
            {
                return false;
            }
            
            var collisionResponse = physicsColliderLookup[entityB].Value.Value.GetCollisionResponse();
            if (collisionResponse is CollisionResponsePolicy.Collide or CollisionResponsePolicy.CollideRaiseCollisionEvents)
            {
                return true;
            }

            if (!dynamicCollisionLookup.TryGetBuffer(entityB, out var dynamicCollisionBufferB))
            {
                return false;
            }

            uint targetLayer = dynamicColliderA.ownLayer;
            foreach (var allowedCollision in dynamicCollisionBufferB)
            {
                if (targetLayer == allowedCollision.withLayer)
                {
                    return true;
                }
            }

            return false;
        }
    }

}