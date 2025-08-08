using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ProjectTools.Ecs
{
    public struct PhysicsConstraints : IComponentData
    {
        public bool3 freezePosition;
        public bool3 freezeRotation;
    }
    
    public class RigidbodyBaker : Baker<Rigidbody>
    {
        public override void Bake(Rigidbody authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            var constraints = authoring.constraints;
            AddComponent(entity, new PhysicsConstraints
            {
                freezePosition = constraints.HasFlag(RigidbodyConstraints.FreezePosition) ? new bool3(true, true, true) :
                    new bool3(
                        constraints.HasFlag(RigidbodyConstraints.FreezePositionX),
                        constraints.HasFlag(RigidbodyConstraints.FreezePositionY),
                        constraints.HasFlag(RigidbodyConstraints.FreezePositionZ)
                        ),
                freezeRotation = constraints.HasFlag(RigidbodyConstraints.FreezeRotation) ? new bool3(true, true, true) :
                    new bool3(
                        constraints.HasFlag(RigidbodyConstraints.FreezeRotationX),
                        constraints.HasFlag(RigidbodyConstraints.FreezeRotationY),
                        constraints.HasFlag(RigidbodyConstraints.FreezeRotationZ)
                        ),
            });
        }
    }
}