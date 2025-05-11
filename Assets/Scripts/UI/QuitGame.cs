using UnityEngine;

namespace UI {
    public class QuitGame : MonoBehaviour {
        public void Quit() {
            #if !UNITY_WEBGL
            Application.Quit();
            #endif

            #if UNITY_WEBGL && !UNITY_EDITOR
            Application.ExternalEval("location.reload();");
            #endif

            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #endif
        }
    }
}