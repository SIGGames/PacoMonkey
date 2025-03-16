using UnityEngine;
using TMPro;
using System;
using static Utils.PlayerPrefsKeys;

namespace UI {
    public class SetCurrentPlayTime : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI textComponent;
        [SerializeField] private bool getSessionTime;

        private float _playTime;
        private float _sessionTime;

        private void Start() {
            _playTime = PlayerPrefs.GetFloat(CurrentPlayTimeKey, 0);
            _sessionTime = 0;
            UpdatePlayTimeText();
        }

        private void Update() {
            float deltaTime = Time.unscaledDeltaTime;
            _playTime += deltaTime;
            _sessionTime += deltaTime;

            PlayerPrefs.SetFloat(CurrentPlayTimeKey, _playTime);
            PlayerPrefs.Save();

            UpdatePlayTimeText();
        }

        private void UpdatePlayTimeText() {
            TimeSpan timeSpan = TimeSpan.FromSeconds(getSessionTime ? _sessionTime : _playTime);
            textComponent.text = $"{timeSpan.Hours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
        }
    }
}