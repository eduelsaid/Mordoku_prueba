using UnityEngine;

namespace Murdoku
{
    /// <summary>
    /// Arranca el prototipo automáticamente si no hay un MurdokuGameController en escena.
    /// </summary>
    public static class MurdokuBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            if (Object.FindAnyObjectByType<MurdokuGameController>() != null)
                return;

            var go = new GameObject("MurdokuGame");
            go.AddComponent<MurdokuGameController>();
        }
    }
}
