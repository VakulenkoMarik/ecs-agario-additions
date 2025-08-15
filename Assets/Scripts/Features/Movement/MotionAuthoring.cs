using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Features.Movement
{
    public struct Motion : IComponentData
    {
        public float maxSpeed;
        public float alteredMaxSpeed;
        public float accelerationTime;
    }
    
    public struct Direction : IComponentData
    {
        public float3 vector;
    }
    
    public class MotionAuthoring : MonoBehaviour
    {
        public float maxSpeed;
        [Range(0.001f, 10.0f)]
        public float accelerationTime;
    }

    public class MotionBacker : Baker<MotionAuthoring>
    {
        public override void Bake(MotionAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
    
            AddComponent(entity, new Motion
            {
                maxSpeed = authoring.maxSpeed,
                alteredMaxSpeed = authoring.maxSpeed,
                accelerationTime = authoring.accelerationTime,
            });
            
            AddComponent(entity, new Direction
            {
                vector = float3.zero,
            });
        }
    }
}