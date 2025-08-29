using Features.Consumption.EatingEvents;
using Features.Controller;
using Unity.Entities;

namespace Features.CameraControl
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct SetCameraTargetOnSelfConsumingSystem : ISystem
    {
        private ComponentLookup<CameraFollowTarget> _cameraTargets;
        private ComponentLookup<CharacterInstance> _characterInstances;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EatenEventTag>();
            _cameraTargets = state.GetComponentLookup<CameraFollowTarget>(true);
            _characterInstances = state.GetComponentLookup<CharacterInstance>(true);
        }
        
        public void OnUpdate(ref SystemState state)
        {
            var bufferEntity = SystemAPI.GetSingletonEntity<EatenEventTag>();
            var buffer = SystemAPI.GetBuffer<EatenEvent>(bufferEntity);

            if (buffer.IsEmpty)
                return;

            _cameraTargets.Update(ref state);
            _characterInstances.Update(ref state);

            foreach (var e in buffer)
            {
                if (_cameraTargets.HasComponent(e.victim) && _cameraTargets.IsComponentEnabled(e.victim))
                {
                    if (_characterInstances.TryGetComponent(e.victim, out var victimInstance) &&
                        _characterInstances.TryGetComponent(e.eater, out var eaterInstance) &&
                        victimInstance.parent == eaterInstance.parent)
                    {
                        if (_cameraTargets.HasComponent(e.eater))
                        {
                            SystemAPI.SetComponentEnabled<CameraFollowTarget>(e.eater, true);
                        }
                    }
                }
            }
        }
    }
}