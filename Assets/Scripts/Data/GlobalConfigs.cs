using System;
using Unity.Entities;
using UnityEngine;

namespace Data
{
    
    
    [CreateAssetMenu(fileName = "GlobalConfigs", menuName = "Data/GlobalConfigs")][Serializable]
    public class GlobalConfigs : BaseSOAuthoring
    {
        [SerializeField] private GameplayConfig gameplayConfig;
        [SerializeField] private AIBehaviourConfig aiBehaviourConfig;
        
        public override void Bake(World world)
        {
            world.EntityManager.CreateSingleton(gameplayConfig);
            world.EntityManager.CreateSingleton(aiBehaviourConfig);
        }
    }
}