using HSM;
using Unity.Entities;
using UnityEngine;

namespace Core
{
    public class MenuState : BaseSubState<GameState, AppHsm>
    {
        public override void OnEnter(SystemBase system)
        {
            Debug.Log("Enter MenuState");
        }
        
        public override void OnExit(SystemBase system)
        {
            Debug.Log("Exit MenuState");
        }
    }
}