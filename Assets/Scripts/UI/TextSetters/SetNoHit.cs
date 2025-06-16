using Managers;
using TMPro;
using UnityEngine;

namespace UI.TextSetters {
    public class SetNoHit : MonoBehaviour {
        private TextMeshProUGUI _textComponent;

        private void Awake() {
            _textComponent = GetComponent<TextMeshProUGUI>();
        }

        private void Update() {
            SetText();
        }

        private void SetText() {
            if (_textComponent != null) {
                if (CharacterManager.Instance?.currentPlayerController?.lives.isNoHit == true) {
                    _textComponent.text = "No Hit";
                    enabled = true;
                } else {
                    enabled = false;
                }
            }
        }
    }
}