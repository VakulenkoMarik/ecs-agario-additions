using System;
using Unity.Entities;
using UnityEngine;

namespace Data
{
    
    
    [CreateAssetMenu(fileName = "GlobalConfigs", menuName = "Data/GlobalConfigs")][Serializable]
    public class GlobalConfigs : BaseSOAuthoring
    {
        [SerializeField] private GameplayConfig gameplayConfig;
        
        public override void Bake(World world)
        {
            world.EntityManager.CreateSingleton(gameplayConfig);
        }
    }
}