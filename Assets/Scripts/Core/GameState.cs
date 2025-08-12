using System;
using System.Collections.Generic;
using Features.Controller;
using Features.Movement;
using HSM;
using Unity.Entities;
using UnityEngine;

namespace Core
{
    public class GameState : BaseSubState<GameState, AppHsm>
    {
        protected override List<Type> RequiredSystems => new List<Type>
        {
            typeof(MotionSystem),
            typeof(PlayerControlSystem),
        };

        public override void OnEnter(SystemBase system)
        {
            Debug.Log("Enter GameState");
        }
        
        public override void OnExit(SystemBase system)
        {
            Debug.Log("Exit GameState");
        }
    }
}