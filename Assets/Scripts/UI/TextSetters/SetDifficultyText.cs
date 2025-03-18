using Enums;
using Localization;
using Managers;
using TMPro;
using UnityEngine;

namespace UI.TextSetters {
    public class SetDifficultyText : MonoBehaviour {
        private TextMeshProUGUI _textComponent;
        private Difficulty _previousDifficulty;

        private void Awake() {
            _textComponent = GetComponent<TextMeshProUGUI>();

            if (_textComponent == null) {
                Debug.LogError("TextMeshProUGUI component not found on " + name);
            }
        }

        private void Update() {
            Difficulty currentDifficulty = DifficultyManager.Instance.currentDifficulty;

            if (_previousDifficulty == currentDifficulty) {
                return;
            }

            _previousDifficulty = currentDifficulty;

            string difficultyKey = currentDifficulty.ToString().ToLower();
            _textComponent.text = LocalizationManager.Instance.GetLocalizedText(difficultyKey);
        }
    }
}