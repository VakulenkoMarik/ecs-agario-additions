using Unity.Burst;
using Unity.Entities;
using Unity.Physics.Systems;

namespace ProjectTools.Ecs
{
    [UpdateInGroup(typeof(PhysicsSimulationGroup), OrderFirst = true)]
    [BurstCompile]
    public partial class DynamicCollisionCooldownSystem : SystemBase
    {
        [BurstCompile]
        protected override void OnUpdate()
        {
            Dependency = new CooldownJob
            {
                deltaTime = SystemAPI.Time.fixedDeltaTime,
            }.ScheduleParallel(Dependency);
        }
        
        [BurstCompile]
        [WithNone(typeof(Prefab))]
        public partial struct CooldownJob : IJobEntity
        {
            public double deltaTime;

            [BurstCompile]
            public void Execute(ref DynamicBuffer<DynamicForcedCollision> dynamicCollisions)
            {
                for (int i = dynamicCollisions.Length - 1; i >= 0; i--)
                {
                    var dynamicCollision = dynamicCollisions[i];
                    if (dynamicCollision.cooldown <= 0)
                    {
                        continue;
                    }
                    
                    dynamicCollision.cooldown -= deltaTime;
                    if (dynamicCollision.cooldown <= 0)
                    {
                        dynamicCollisions.RemoveAtSwapBack(i);
                    }
                    else
                    {
                        dynamicCollisions[i] = dynamicCollision;
                    }
                }
            }
        }
    }
}