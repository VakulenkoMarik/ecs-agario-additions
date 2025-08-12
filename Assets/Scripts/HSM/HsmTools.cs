using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace HSM
{
    public static class HsmTools
    {
        public static bool IsAppQuitting { get; private set; }

        public static void InitHsm(this World world, ISubState<AppHsm> initState, List<Type> newRequiredSystems = null)
        {
            world.EntityManager.CreateSingleton(new AppHsm(initState, newRequiredSystems));
        }
        
        public static void InitHsm(ISubState<AppHsm> initState, List<Type> newRequiredSystems = null)
        {
            World.DefaultGameObjectInjectionWorld.EntityManager.CreateSingleton(new AppHsm(initState, newRequiredSystems));
        }
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            Application.quitting += OnApplicationQuitting;
        }
    
        private static void OnApplicationQuitting()
        {
            IsAppQuitting = true;
        }
    }

}