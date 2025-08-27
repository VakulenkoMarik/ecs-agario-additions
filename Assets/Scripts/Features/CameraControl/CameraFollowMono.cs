using Unity.Mathematics;
using UnityEngine;

namespace Features.CameraControl
{
    public class CameraFollowMono : MonoBehaviour
    {
        public static CameraFollowMono Instance { get; private set; }

        private Vector3 _currentTargetPos;

        private void Awake()
        {
            Instance = this;
        }

        public void SetTargetPosition(float3 pos)
        {
            _currentTargetPos = pos;
        }

        private void LateUpdate()
        {
            transform.position = Vector3.Lerp(
                transform.position,
                _currentTargetPos + new Vector3(0, 0, -10),
                Time.deltaTime * 5f
            );
        }
    }
}