using Features.Feed;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Features.Input;
using Features.Movement;

namespace Features.Controller
{
    [UpdateInGroup(typeof(GameplayGroup)), UpdateAfter(typeof(InputSystem))]
    [BurstCompile]
    public partial struct PlayerControlSystem : ISystem
    {
        private ComponentLookup<DirectionComponent> _directionLookup;
        private ComponentLookup<FeedComponent> _feedLookup;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<InputActionsComponent>();
            state.RequireForUpdate<PlayerInputComponent>();
            _directionLookup = state.GetComponentLookup<DirectionComponent>();
            _feedLookup = state.GetComponentLookup<FeedComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var inputEntity = SystemAPI.GetSingletonEntity<InputActionsComponent>();
            if (!state.EntityManager.GetComponentDataIfEnabled(inputEntity, out PlayerInputComponent playerInput))
            {
                return;
            }
            
            _directionLookup.Update(ref state);
            _feedLookup.Update(ref state);
            
            float3 direction = new float3(playerInput.moveValue.x, playerInput.moveValue.y, 0);
            bool isFeedPressed = playerInput.IsFeedPressed;
            foreach (var (_, entity) in SystemAPI.Query<RefRO<PlayerControlComponent>>().WithEntityAccess())
            {
                if (_directionLookup.TryGetComponent(entity, out var rwDirection))
                {
                    rwDirection.weightedDirection = direction;
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