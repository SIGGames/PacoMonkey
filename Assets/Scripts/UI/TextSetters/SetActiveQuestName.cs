using TMPro;
using UnityEngine;

namespace UI.TextSetters {
    public class SetActiveQuestName : MonoBehaviour {
        private TextMeshProUGUI _textComponent;

        private void Awake() {
            _textComponent = GetComponent<TextMeshProUGUI>();
        }

        public void UpdateTextName(string questName) {
            _textComponent.text = questName;
        }
    }
}