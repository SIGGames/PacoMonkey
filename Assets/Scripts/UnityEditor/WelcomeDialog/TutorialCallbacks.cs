using UnityEngine;

namespace UnityEditor.WelcomeDialog {
    public class TutorialCallbacks : ScriptableObject {
        public GameObject TokenToSelect;

        public void SelectGameObject(GameObject gameObject) {
            if (!gameObject) return;
            Selection.activeObject = gameObject;
        }

        public void SelectToken() {
            if (!TokenToSelect) {
                TokenToSelect = GameObject.FindGameObjectWithTag("TutorialRequirement");
                if (!TokenToSelect) {
                    Debug.LogError("A GameObject with the tag 'TutorialRequirement' is required.");
                    return;
                }
            }

            SelectGameObject(TokenToSelect);
        }

        public void SelectMoveTool() {
            Tools.current = Tool.Move;
        }

        public void SelectRotateTool() {
            Tools.current = Tool.Rotate;
        }
    }
}