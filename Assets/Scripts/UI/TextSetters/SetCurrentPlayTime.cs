using System;
using Managers;
using TMPro;
using UnityEngine;

namespace UI.TextSetters {
    public class SetCurrentPlayTime : MonoBehaviour {
        private TextMeshProUGUI _textComponent;

        private void Awake() {
            if (_textComponent == null) {
                _textComponent = GetComponent<TextMeshProUGUI>();
            }

            if (_textComponent == null) {
                Debug.LogError("Text component not found: " + name);
            }
        }

        private void Update() {
            if (PlayTimeManager.Instance == null) {
                return;
            }

            UpdatePlayTimeText();
        }

        private void UpdatePlayTimeText() {
            float playTime = PlayTimeManager.Instance.PlayTime;
            TimeSpan timeSpan = TimeSpan.FromSeconds(playTime);
            _textComponent.text = $"{timeSpan.Hours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
        }
    }
}