using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

namespace Features.CameraControl
{
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct CameraFollowSystem : ISystem
    {
        private Entity _currentTarget;
        private ComponentLookup<LocalTransform> _transformLookup;
        private ComponentLookup<CameraFollowTarget> _followLookup;

        public void OnCreate(ref SystemState state)
        {
            _transformLookup = state.GetComponentLookup<LocalTransform>(true);
            _followLookup = state.GetComponentLookup<CameraFollowTarget>(false);
            _currentTarget = Entity.Null;
        }

        public void OnUpdate(ref SystemState state)
        {
            _transformLookup.Update(ref state);
            _followLookup.Update(ref state);
            
            foreach (var (target, entity) in 
                     SystemAPI.Query<EnabledRefRW<CameraFollowTarget>>()
                         .WithEntityAccess())
            {
                if (entity == _currentTarget)
                    continue;
                
                if (target.ValueRO)
                {
                    if (_currentTarget != Entity.Null && 
                        _followLookup.HasComponent(_currentTarget))
                    {
                        _followLookup.SetComponentEnabled(_currentTarget, false);
                    }

                    _currentTarget = entity;
                }
                else
                {
                    if (_currentTarget == entity)
                        _currentTarget = Entity.Null;
                }
            }
            
            if (_currentTarget != Entity.Null && 
                _transformLookup.HasComponent(_currentTarget))
            {
                var targetPos = _transformLookup[_currentTarget].Position;
                CameraFollowMono.Instance.SetTargetPosition(targetPos);
            }
        }
    }
}