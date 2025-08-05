using Unity.Entities;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Features.Input
{
    [UpdateInGroup(typeof(GameplayGroup), OrderFirst = true)]
    public partial class InputSystem : SystemBase, InputActions.IPlayerActions
    {
        private InputActions _inputActions;
        private Tracked<PlayerInputComponent> _trackedPlayerInput;

        protected override void OnCreate()
        {
            _inputActions = new InputActions();
            _inputActions.Disable();
            
            var entity = EntityManager.CreateSingleton<InputActionsComponent>();
            EntityManager.AddComponentData(entity, _trackedPlayerInput.data);
            //EntityManager.SetComponentEnabled<PlayerInputComponent>(entity, false); // Disable input map

            _inputActions.Player.SetCallbacks(this);
        }

        protected override void OnUpdate()
        {
            var entity = SystemAPI.GetSingletonEntity<InputActionsComponent>();
            RefreshInputActions(in entity, _inputActions.Player, ref _trackedPlayerInput);

            if (_trackedPlayerInput.PopIsChanged)
            {
                EntityManager.SetComponentData(entity, _trackedPlayerInput.data);
            }
        }

        protected override void OnStopRunning()
        {
            _inputActions.Disable();
            var entity = SystemAPI.GetSingletonEntity<InputActionsComponent>();
            _trackedPlayerInput = default;
            EntityManager.SetComponentData(entity, _trackedPlayerInput.data);
        }
        
        protected override void OnDestroy()
        {
            _inputActions.Player.SetCallbacks(null);
            _inputActions.Dispose();
            _inputActions = null;
        }
        
        private void RefreshInputActions<T>(in Entity entity, InputActionMap map, ref Tracked<T> trackedData) 
            where T : unmanaged, IComponentData, IEnableableComponent
        {
            bool isEnabled = EntityManager.IsComponentEnabled<T>(entity);

            if (map.enabled == isEnabled)
            {
                return;
            }

            if (isEnabled)
            {
                map.Enable();
            }
            else
            {
                map.Disable();
                trackedData = default;
                EntityManager.SetComponentData(entity, trackedData.data);
            }
        }


        void InputActions.IPlayerActions.OnMove(InputAction.CallbackContext context)
        {
            _trackedPlayerInput.data.moveValue = context.action.ReadValue<Vector2>();
            _trackedPlayerInput.isChanged = true;
        }
            
        void InputActions.IPlayerActions.OnLook(InputAction.CallbackContext context) 
        {
            _trackedPlayerInput.data.lookValue = context.action.ReadValue<Vector2>();
            _trackedPlayerInput.isChanged = true;
        }

        void InputActions.IPlayerActions.OnFeed(InputAction.CallbackContext context)
        {
            _trackedPlayerInput.data.feedValue = context.action.ReadValue<float>();
            _trackedPlayerInput.isChanged = true;
        }

        void InputActions.IPlayerActions.OnJump(InputAction.CallbackContext context) 
        {
            _trackedPlayerInput.data.jumpValue = context.action.ReadValue<float>();
            _trackedPlayerInput.isChanged = true;
        } 
    }
}