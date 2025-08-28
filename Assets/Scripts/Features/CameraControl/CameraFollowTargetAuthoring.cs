using Unity.Entities;
using UnityEngine;

namespace Features.CameraControl
{
    public struct CameraFollowTarget : IEnableableComponent, IComponentData
    {
        public bool activateOnStart;
        public bool beenActivatedObStart;
    }
    
    public class CameraFollowTargetAuthoring : MonoBehaviour
    {
        [SerializeField] private bool activateOnStart;
        
        public class Baker : Baker<CameraFollowTargetAuthoring>
        {
            public override void Bake(CameraFollowTargetAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
                
                AddComponent(entity, new CameraFollowTarget
                {
                    activateOnStart = authoring.activateOnStart
                });
                
                SetComponentEnabled<CameraFollowTarget>(entity, false);
            }
        }
    }
}
