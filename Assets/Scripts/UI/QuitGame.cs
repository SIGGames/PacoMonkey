using UnityEngine;

namespace UI {
    public class QuitGame : MonoBehaviour {
        public void Quit() {
            #if !UNITY_WEBGL
            // Well, not sure if this is the right approach but at least it does not freezes the game on WebGL
            Application.Quit();
            #endif

            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #endif
        }
    }
}