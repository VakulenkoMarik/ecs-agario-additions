using Features.Feed;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Features.Input;
using Features.Movement;
using ProjectTools.Ecs;
using Unity.Transforms;

namespace Features.Controller
{
    [UpdateInGroup(typeof(GameplayGroup)), UpdateAfter(typeof(InputSystem))]
    [BurstCompile]
    public partial struct PlayerControlSystem : ISystem
    {
        private ComponentLookup<DirectionComponent> _directionLookup;
        private ComponentLookup<FeedComponent> _feedLookup;
        private ComponentLookup<LocalTransform> _transformLookup;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<InputActionsComponent>();
            state.RequireForUpdate<PlayerInputComponent>();
            _directionLookup = state.GetComponentLookup<DirectionComponent>();
            _feedLookup = state.GetComponentLookup<FeedComponent>();
            _transformLookup = state.GetComponentLookup<LocalTransform>(true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var inputActions = SystemAPI.GetSingleton<InputActionsComponent>();
            var inputEntity = SystemAPI.GetSingletonEntity<InputActionsComponent>();
            if (!state.EntityManager.GetComponentDataIfEnabled(inputEntity, out PlayerInputComponent playerInput))
            {
                return;
            }
            
            _directionLookup.Update(ref state);
            _feedLookup.Update(ref state);
            _transformLookup.Update(ref state);

            float3 targetPosition = new float3 { xy = playerInput.targetValue};
            bool isFeedPressed = playerInput.IsFeedPressed;
            foreach (var (_, entity) in SystemAPI.Query<RefRO<PlayerControlComponent>>().WithEntityAccess())
            {
                if (_directionLookup.TryGetComponent(entity, out var rwDirection) && 
                    _transformLookup.TryGetComponent(entity, out var localTransform))
                {
                    if (inputActions.isFocussed)
                    {
                        targetPosition.z = localTransform.Position.z;
                        var direction = targetPosition - localTransform.Position;
                        rwDirection.direction = math.length(direction) > 1f ? math.normalize(direction) : direction;
                    }
                    else if (math.any(rwDirection.direction != float3.zero))
                    {
                        rwDirection.direction = float3.zero;
                    }
                    
                    SystemAPI.SetComponent(entity, rwDirection);
                }
                
                if (_feedLookup.TryGetComponent(entity, out var rwFeed))
                {
                    rwFeed.tryToFeed = isFeedPressed;
                    SystemAPI.SetComponent(entity, rwFeed);
                }
            }
        }
    }
}