using TMPro;
using Unity.VisualScripting;
using UnityEngine;

namespace Localization {
    public class LocalizedTextUpdater : MonoBehaviour {
        [SerializeField] private string textKey;
        private TextMeshProUGUI _textComponent;

        private void Awake() {
            if (_textComponent == null) {
                _textComponent = GetComponent<TextMeshProUGUI>();
            }

            if (_textComponent == null) {
                Debug.LogError("Text component not found: " + name);
            }
        }

        private void Start() {
            UpdateText();
        }

        private void OnEnable() {
            if (LocalizationManager.Instance != null) {
                LocalizationManager.Instance.OnLanguageChanged += UpdateText;
                // Force update since the text might have changed while the object was disabled
                UpdateText();
            }
        }

        private void OnDisable() {
            if (LocalizationManager.Instance != null) {
                LocalizationManager.Instance.OnLanguageChanged -= UpdateText;
            }
        }

        private void UpdateText() {
            if (LocalizationManager.Instance != null) {
                _textComponent.text = LocalizationManager.Instance.GetLocalizedText(textKey);
            }
        }
    }
}