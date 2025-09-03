using Data;
using HSM;
using Unity.Entities;
using UnityEngine;

namespace Core
{
    public enum StateType
    {
        MainMenu,
        Game,
    }

    public class Boot : MonoBehaviour
    {
        [SerializeField] private GlobalConfigs globalConfigs;
        [SerializeField] private StateType initStateType;

        private bool _isInitialized = false;

        private void Awake()
        {
            if (!_isInitialized)
            {
                Application.targetFrameRate = 60;
                globalConfigs.Bake(World.DefaultGameObjectInjectionWorld);

                ISubState<AppHsm> initState = GetSubStateByType(initStateType);
                HsmTools.InitHsm(initState);

                _isInitialized = true;
            }
        }

        private ISubState<AppHsm> GetSubStateByType(StateType type) {
            return type switch
            {
                StateType.MainMenu => new MenuState(),
                StateType.Game => new GameState(),
                _ => new MenuState(),
            };
        }
    }
}