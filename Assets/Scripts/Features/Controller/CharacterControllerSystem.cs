using Unity.Burst;
using Unity.Entities;
using Features.Input;
using Features.Movement;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Transforms;

namespace Features.Controller
{
    [UpdateInGroup(typeof(GameplayGroup))]
    [BurstCompile]
    public partial struct CharacterControllerSystem : ISystem
    {
        private ComponentLookup<GameCommands> _gameCommandsLookup;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            _gameCommandsLookup = state.GetComponentLookup<GameCommands>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            _gameCommandsLookup.Update(ref state);
            
            var directionJob = new CharacterControlDirectionJob
            {
                gameCommandsLookup = _gameCommandsLookup,
            }.ScheduleParallel(state.Dependency);
            
            state.Dependency = directionJob;
        }
    }

    [BurstCompile]
    public partial struct CharacterControlDirectionJob : IJobEntity
    {
        [ReadOnly] 
        public ComponentLookup<GameCommands> gameCommandsLookup;
        
        [BurstCompile]
        public void Execute(in CharacterInstance instance, in LocalTransform localTransform, ref Direction direction)
        {
            if (!gameCommandsLookup.TryGetComponent(instance.parent, out var gameCommands))
            {
                return;
            }
            
            if (gameCommands.isTargetValid)
            {
                var targetPosition = new float3
                {
                    xy = gameCommands.targetValue,
                    z = localTransform.Position.z,
                };
                var directionVector = targetPosition - localTransform.Position;
                direction.vector = math.length(directionVector) > 1f ? math.normalize(directionVector) : directionVector;
            }
            else if (math.any(direction.vector != float3.zero))
            {
                direction.vector = float3.zero;
            }
        }
    }
}