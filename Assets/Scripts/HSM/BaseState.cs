using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace HSM
{
    public abstract class BaseState
    {
        protected virtual List<Type> RequiredSystems => new();

        internal abstract void SetSubStateInternal(BaseState subState);
        
        internal abstract void OnEnterInternal(SystemBase system);
        internal abstract void OnUpdateInternal(SystemBase system);
        internal abstract void OnLateUpdateInternal(SystemBase system);

        internal abstract void OnExitInternal(SystemBase system);

        internal abstract void TryChangeStateInternal(SystemBase system);
    }
    
    public abstract class BaseSubState<TParent> : BaseState<BaseSubState<TParent>> where TParent : BaseState
    {
        public TParent Parent { get; private set; }

        internal void SetParent(TParent parent)
        {
            Parent = parent;
        }
        
        protected void SetState(BaseSubState<TParent> state)
        {
            Parent.SetSubStateInternal(state);
        }
    }
    
    public abstract class BaseState<TSelf> : BaseState where TSelf : BaseState
    {
        public BaseSubState<TSelf> SubState { get; internal set; }
        private bool _isSubStateChangeRequested = false;
        private BaseSubState<TSelf> _nextSubState = null;

        public virtual void OnEnter(SystemBase system){}
        public virtual void OnUpdate(SystemBase system){}
        public virtual void OnLateUpdate(SystemBase system){}
        public virtual void OnExit(SystemBase system){}
        
        protected void SetSubState(BaseSubState<TSelf> subState)
        {
            _isSubStateChangeRequested = true;
            _nextSubState = subState;
        }
        
        internal override void SetSubStateInternal(BaseState subState)
        {
            SetSubState(subState as BaseSubState<TSelf>);
        }

        internal override void OnEnterInternal(SystemBase system)
        {
            SetSystemsActive(system, true);
            OnEnter(system);
            TryChangeStateInternal(system);
        }
        
        internal override void OnUpdateInternal(SystemBase system)
        {
            OnUpdate(system);
            SubState?.OnUpdateInternal(system);
        }

        internal override void OnLateUpdateInternal(SystemBase system)
        {
            OnLateUpdate(system);
            SubState?.OnLateUpdateInternal(system);
        }
        
        internal override void TryChangeStateInternal(SystemBase system)
        {
            if (!_isSubStateChangeRequested)
            {
                SubState?.TryChangeStateInternal(system);
                return;
            }

            _isSubStateChangeRequested = false;
            
            SubState?.OnExitInternal(system);
            SubState = _nextSubState;
            _nextSubState = null;
            
            if (SubState == null)
            {
                return;
            }
            
            SubState.SetParent(this as TSelf);
            SubState.OnEnterInternal(system);
        }

        internal override void OnExitInternal(SystemBase system)
        {
            SubState?.OnExitInternal(system);
            SubState = null;
            OnExit(system);
            SetSystemsActive(system, false);
        }
        
        private void SetSystemsActive(SystemBase system, bool isActive)
        {
            foreach (var systemType in RequiredSystems)
            {
                if (systemType.IsClass)
                {
                    var systemRef = system.World.GetExistingSystemManaged(systemType);
                    if (systemRef == null)
                    {
                        Debug.LogError($"System {systemType} not found or not created yet.");
                        continue;
                    }
                    
                    systemRef.Enabled = isActive;
                }
                else
                {
                    var systemHandle = system.World.Unmanaged.GetExistingUnmanagedSystem(systemType);
                    if (systemHandle == SystemHandle.Null)
                    {
                        Debug.LogError($"System {systemType} not found or not created yet.");
                        continue;
                    }
                    
                    system.World.Unmanaged.ResolveSystemStateRef(systemHandle).Enabled = isActive;
                }
            }
        }
        
    }
}