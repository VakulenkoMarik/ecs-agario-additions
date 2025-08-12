using Unity.Entities;

namespace HSM
{
    [UpdateInGroup(typeof(InitializationSystemGroup), OrderFirst = true)]
    public partial class HsmPreUpdateSystem : SystemBase
    {
        private bool _isFirstUpdate = true;
        
        protected override void OnCreate()
        {
            RequireForUpdate<AppHsm>();
        }

        protected override void OnUpdate()
        {
            var hsm = SystemAPI.ManagedAPI.GetSingleton<AppHsm>();
            if (_isFirstUpdate)
            {
                hsm.OnEnterInternal(this);
                _isFirstUpdate = false;
            }
            
            hsm.OnUpdateInternal(this);
        }
    }
    
    [UpdateInGroup(typeof(PresentationSystemGroup), OrderFirst = true)]
    public partial class HsmLateUpdateSystem : SystemBase
    {
        protected override void OnCreate()
        {
            RequireForUpdate<AppHsm>();
        }

        protected override void OnUpdate()
        {
            var hsm = SystemAPI.ManagedAPI.GetSingleton<AppHsm>();
            hsm.OnLateUpdateInternal(this);
            hsm.TryChangeStateInternal(this);
        }

        protected override void OnDestroy()
        {
            var hsm = SystemAPI.ManagedAPI.GetSingleton<AppHsm>();
            hsm.OnExitInternal(this);
        }
    }
}