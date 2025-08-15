using Features.Input;
using Unity.Burst;
using Unity.Entities;

namespace Features.Controller
{
    [UpdateInGroup(typeof(GameplayGroup)), UpdateBefore(typeof(CharacterControllerSystem))]
    [BurstCompile]
    public partial struct PlayerControlSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<InputBridge>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var inputBridgeEntity = SystemAPI.GetSingletonEntity<InputBridge>();
            var playerGameCommands = SystemAPI.GetComponent<GameCommands>(inputBridgeEntity);

            foreach (var rwGameCommands in SystemAPI.Query<RefRW<GameCommands>>().WithAll<PlayerControlTag>())
            {
                rwGameCommands.ValueRW = playerGameCommands;
            }
        }
    }
}