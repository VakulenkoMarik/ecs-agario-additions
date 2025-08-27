using Unity.Entities;
using UnityEngine;

namespace Features.CameraControl
{
    public struct CameraFollowTarget : IEnableableComponent, IComponentData {
        
    }
    
    public class CameraFollowTargetAuthoring : MonoBehaviour
    {
        public class Baker : Baker<CameraFollowTargetAuthoring>
        {
            public override void Bake(CameraFollowTargetAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
                
                AddComponent<CameraFollowTarget>(entity);
                SetComponentEnabled<CameraFollowTarget>(entity, false);
            }
        }
    }
}
