using Unity.Mathematics;
using UnityEngine;

namespace Features.CameraControl
{
    [RequireComponent(typeof(Camera))]
    public class CameraFollowMono : MonoBehaviour
    {
        public static CameraFollowMono Instance { get; private set; }

        private Vector3 _currentTargetPos;
        private float _currentTargetCameraSize = 2;
        private Camera _thisCamera;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            _thisCamera = GetComponent<Camera>();
        }

        private void LateUpdate()
        {
            if (transform.position != _currentTargetPos)
            {
                transform.position = Vector3.Lerp(
                    transform.position,
                    _currentTargetPos + new Vector3(0, 0, -10),
                    Time.deltaTime * 5f
                );
            } 
            
            if (_thisCamera.orthographicSize - _currentTargetCameraSize > .001f)
            {
                _thisCamera.orthographicSize = Mathf.Lerp(
                    _thisCamera.orthographicSize,
                    _currentTargetCameraSize,
                    Time.deltaTime * 5f
                );
            }
        }

        public void SetCameraSizeInstantly(float targetCameraSize)
        {
            _thisCamera.orthographicSize = targetCameraSize;
            _currentTargetCameraSize = targetCameraSize;
        }
        
        public void SetCameraSize(float targetCameraSize)
        {
            _currentTargetCameraSize = targetCameraSize;
        }
        
        public void SetTargetPositionInstantly(float3 pos)
        {
            transform.position = pos;
            _currentTargetPos = pos;
        }

        public void SetTargetPosition(float3 pos)
        {
            _currentTargetPos = pos;
        }
    }
}