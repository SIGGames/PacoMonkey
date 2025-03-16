using TMPro;
using UnityEngine;

namespace Localization {
    public class LocalizedTextUpdater : MonoBehaviour {
        [SerializeField] private string textKey;
        private TextMeshProUGUI _textComponent;

        private void Awake() {
            _textComponent = GetComponent<TextMeshProUGUI>();
        }

        private void Start() {
            UpdateText();
        }

        private void OnEnable() {
            if (LocalizationManager.Instance != null) {
                LocalizationManager.Instance.OnLanguageChanged += UpdateText;
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