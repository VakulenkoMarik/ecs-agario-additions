using System;
using System.Collections.Generic;
using Unity.Entities;

namespace HSM
{
    public class AppHsm : BaseState<AppHsm>, IComponentData
    {
        public AppHsm(BaseSubState<AppHsm> initState, List<Type> newRequiredSystems = null)
        {
            base.SetSubState(initState);
            _requiredSystems = newRequiredSystems ?? new List<Type>();
        }

        public new void SetSubState(BaseSubState<AppHsm> nextState)
        {
            base.SetSubState(nextState);
        }
        
        private readonly List<Type> _requiredSystems;
        protected override List<Type> RequiredSystems => _requiredSystems;
    }
}