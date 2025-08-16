using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Features.Input
{
    [UpdateInGroup(typeof(GameplayGroup), OrderFirst = true)]
    public partial class InputSystem : SystemBase, InputActions.IPlayerActions
    {
        private InputActions _inputActions;
        private Tracked<InputBridge> _trackedInputBridge;
        private Tracked<GameCommands> _trackedPlayerInput;

        protected override void OnCreate()
        {
            _inputActions = new InputActions();
            _inputActions.Disable();
            
            _trackedInputBridge.data.isFocussed = Application.isFocused;
            var entity = EntityManager.CreateSingleton(_trackedInputBridge.data);
            
            EntityManager.AddComponentData(entity, _trackedPlayerInput.data);
            
            //EntityManager.SetComponentEnabled<PlayerInputComponent>(entity, false); // Disable input map

            
            _inputActions.Player.SetCallbacks(this);
        }
        
        protected override void OnStartRunning()
        {
            Application.focusChanged += OnFocusChanged;
        }

        protected override void OnUpdate()
        {
            var entity = SystemAPI.GetSingletonEntity<InputBridge>();
            RefreshInputActions(in entity, _inputActions.Player, ref _trackedPlayerInput);

            
            
            if (_trackedPlayerInput.PopIsChanged)
            {
                EntityManager.SetComponentData(entity, _trackedPlayerInput.data);
            }
        }

        protected override void OnStopRunning()
        {
            Application.focusChanged -= OnFocusChanged;
            _inputActions.Disable();
            var entity = SystemAPI.GetSingletonEntity<InputBridge>();
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
        
        private void OnFocusChanged(bool isFocussed)
        {
            _trackedPlayerInput.data.isTargetValid = isFocussed;
            _trackedPlayerInput.isChanged = true;
            _trackedInputBridge.data.isFocussed = isFocussed;
            _trackedInputBridge.isChanged = true;
        }

        public void OnTarget(InputAction.CallbackContext context)
        {
            var screenPosition = context.action.ReadValue<Vector2>();
            
            var camera = Camera.main;
            if (camera == null)
            {
                _trackedPlayerInput.data.targetValue = float2.zero;
                _trackedPlayerInput.data.isTargetValid = false;
            }
            else
            {
                var worldPosition = camera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 0));
                _trackedPlayerInput.data.targetValue = new float2(worldPosition.x, worldPosition.y);
                _trackedPlayerInput.data.isTargetValid = _trackedInputBridge.data.isFocussed;
            }
            
            _trackedPlayerInput.isChanged = true;
        }

        void InputActions.IPlayerActions.OnMove(InputAction.CallbackContext context)
        {
            _trackedPlayerInput.data.moveValue = context.action.ReadValue<Vector2>();
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