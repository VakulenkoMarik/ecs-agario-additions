using System;
using System.Collections.Generic;
using Unity.Entities;

namespace HSM
{
    public class AppHsm : BaseState<AppHsm>, IComponentData
    {
#if UNITY_EDITOR && DEBUG
        public readonly List<string> statesStack = new ();

        public override void OnUpdate(SystemBase system)
        {
            statesStack.Clear();
            BaseState subState = SubState as BaseState;
            while (subState != null)
            {
                statesStack.Add(subState.GetType().Name);
                subState = subState.GetSubStateInternal();
            }
        }
#endif
        
        private readonly List<Type> _requiredSystems;
        protected override List<Type> RequiredSystems => _requiredSystems;
        
        public AppHsm(){}

        public AppHsm(ISubState<AppHsm> initState, List<Type> newRequiredSystems = null)
        {
            base.SetSubState(initState);
            _requiredSystems = newRequiredSystems ?? new List<Type>();
        }

        public new void SetSubState(ISubState<AppHsm> nextState)
        {
            base.SetSubState(nextState);
        }
    }
}