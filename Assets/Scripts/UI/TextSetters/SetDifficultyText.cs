using Localization;
using Managers;
using TMPro;
using UnityEngine;

namespace UI.TextSetters {
    public class SetDifficultyText : MonoBehaviour {
        private TextMeshProUGUI _textComponent;

        private void Awake() {
            _textComponent = GetComponent<TextMeshProUGUI>();

            if (_textComponent == null) {
                Debug.LogError("TextMeshProUGUI component not found on " + name);
            }
        }

        private void OnEnable() {
            if (DifficultyManager.Instance != null) {
                string difficultyKey = DifficultyManager.Instance.currentDifficulty.ToString().ToLower();
                _textComponent.text = LocalizationManager.Instance.GetLocalizedText(difficultyKey);
            }
        }
    }
}