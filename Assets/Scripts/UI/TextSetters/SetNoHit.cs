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
                _textComponent.text = CharacterManager.Instance?.currentPlayerController?.lives.isNoHit == true ? "No Hit" : string.Empty;
            }
        }
    }
}