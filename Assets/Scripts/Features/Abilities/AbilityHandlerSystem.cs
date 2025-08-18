using System;
using Features.Abilities.Types;
using Features.Controller;
using Features.Input;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace Features.Abilities
{
    
    public delegate bool TryApplyAbilityRequest(int sortKey, ref EntityCommandBuffer.ParallelWriter ecb, in Ability ability, 
        in GameCommands gameCommands, in DynamicBuffer<ChildInstance> children);
    
    [UpdateInGroup(typeof(GameplayGroup))]
    [BurstCompile]
    public partial class AbilityHandlerSystem : SystemBase
    {
        private NativeParallelHashMap<int, FunctionPointer<TryApplyAbilityRequest>> _applyTable;

        private ComponentLookup<GameCommands> _gameCommandsLookup;

        public void AddAbility(BaseAbilityRequestSystem system)
        {
            _applyTable.Add((int)system.Type, BurstCompiler.CompileFunctionPointer(system.StaticApplyRequest));
        }
        
        protected override void OnCreate()
        {
            RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            _gameCommandsLookup = GetComponentLookup<GameCommands>();
            
            int enumCount = Enum.GetValues(typeof(AbilityType)).Length;
            _applyTable = new NativeParallelHashMap<int, FunctionPointer<TryApplyAbilityRequest>>(enumCount, Allocator.Persistent);
        }

        [BurstCompile]
        protected override void OnUpdate()
        {
            var ecbSingleton = World.GetExistingSystemManaged<EndSimulationEntityCommandBufferSystem>();
            var ecb = ecbSingleton.CreateCommandBuffer().AsParallelWriter();

            _gameCommandsLookup.Update(ref CheckedStateRef);
            
            Dependency = new AbilityJob
            {
                ecb = ecb,
                applyTable = _applyTable.AsReadOnly(),
                elapsedTime = SystemAPI.Time.ElapsedTime,
            }.ScheduleParallel(Dependency);
            
            ecbSingleton.AddJobHandleForProducer(Dependency);
        }
        
        public void OnDestroy(ref SystemState state)
        {
            if (_applyTable.IsCreated)
            {
                _applyTable.Dispose();
            }
        }

    }
    
    [BurstCompile]
    [WithNone(typeof(Prefab))]
    public partial struct AbilityJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ecb;
        [ReadOnly]
        public NativeParallelHashMap<int, FunctionPointer<TryApplyAbilityRequest>>.ReadOnly applyTable;
        [ReadOnly] 
        public double elapsedTime;
        
        [BurstCompile]
        public void Execute([ChunkIndexInQuery] int sortKey, ref DynamicBuffer<Ability> abilities, 
            ref DynamicBuffer<ChildInstance> instances, in GameCommands gameCommands)
        {
            for (int i = 0; i < abilities.Length; i++)
            {
                var ability = abilities[i];
                if (ability.cooldownTimestamp > elapsedTime)
                {
                    continue;
                }
                
                bool isPerformed = applyTable[(int)ability.type].Invoke(sortKey, ref ecb, ability, gameCommands, instances);
                if (isPerformed)
                {
                    ability.cooldownTimestamp = elapsedTime + ability.cooldown;
                    abilities[i] = ability;
                }
            }
        }
    }
}