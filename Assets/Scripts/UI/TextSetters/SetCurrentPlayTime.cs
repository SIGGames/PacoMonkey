using System;
using Managers;
using TMPro;
using UnityEngine;

namespace UI.TextSetters {
    public class SetCurrentPlayTime : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI textComponent;

        private void Awake() {
            if (textComponent == null) {
                textComponent = GetComponent<TextMeshProUGUI>();
            }

            if (PlayTimeManager.Instance == null) {
                enabled = false;
            }

            if (textComponent == null) {
                Debug.LogError("Text component not found: " + name);
            }
        }

        private void Update() {
            UpdatePlayTimeText();
        }

        private void UpdatePlayTimeText() {
            float playTime = PlayTimeManager.Instance.PlayTime;
            TimeSpan timeSpan = TimeSpan.FromSeconds(playTime);
            textComponent.text = $"{timeSpan.Hours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
        }
    }
}