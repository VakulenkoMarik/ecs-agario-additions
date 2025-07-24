using Data;
using Unity.Entities;
using UnityEngine;

namespace Core
{
    public class Boot : MonoBehaviour
    {
        [SerializeField] private GlobalConfigs globalConfigs; 
        
        private void Awake()
        {
            Application.targetFrameRate = 60;
            globalConfigs.Bake(World.DefaultGameObjectInjectionWorld);
        }
        
    }
}