using System;
using TMPro;
using UnityEngine;
using static Utils.PlayerPrefsKeys;

namespace UI.TextSetters {
    public class SetCurrentPlayTime : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI textComponent;
        [SerializeField] private bool getSessionTime;

        private float _playTime;
        private float _sessionTime;

        private void Start() {
            if (!getSessionTime) {
                _playTime = PlayerPrefs.GetFloat(CurrentPlayTimeKey, 0);
            } else {
                _sessionTime = 0;
            }

            UpdatePlayTimeText();
        }

        private void Update() {
            float deltaTime = Time.unscaledDeltaTime;

            if (getSessionTime) {
                _sessionTime += deltaTime;
            } else {
                _playTime += deltaTime;
                PlayerPrefs.SetFloat(CurrentPlayTimeKey, _playTime);
                PlayerPrefs.Save();
            }

            UpdatePlayTimeText();
        }

        private void UpdatePlayTimeText() {
            TimeSpan timeSpan = TimeSpan.FromSeconds(getSessionTime ? _sessionTime : _playTime);
            textComponent.text = $"{timeSpan.Hours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
        }
    }
}