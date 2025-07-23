using UnityEngine;

namespace Core
{
    public class Boot : MonoBehaviour
    {
        private void Awake()
        {
            Application.targetFrameRate = 60;
        }
        
    }
}