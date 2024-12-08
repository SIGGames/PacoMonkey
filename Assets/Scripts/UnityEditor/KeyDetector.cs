using UnityEngine;

namespace UnityEditor {
    public class KeyDetector : MonoBehaviour {
        void Update() {
            foreach (KeyCode keyCode in System.Enum.GetValues(typeof(KeyCode))) {
                if (Input.GetKeyDown(keyCode)) {
                    Debug.Log("Key pressed: " + keyCode);
                }
            }
        }
    }
}