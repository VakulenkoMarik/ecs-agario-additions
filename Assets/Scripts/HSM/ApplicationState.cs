using UnityEngine;

namespace HSM
{
    public static class ApplicationState
    {
        public static bool IsQuitting { get; private set; }
    
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            Application.quitting += OnApplicationQuitting;
        }
    
        private static void OnApplicationQuitting()
        {
            IsQuitting = true;
        }
    }

}