using System;
using Controllers;
using Enums;
using Localization;
using TMPro;
using UnityEngine;

namespace UI.TextSetters {
    public class SetZoneText : MonoBehaviour {
        private TextMeshProUGUI _textComponent;

        private MapZone _previousMapZone = MapZone.Unknown;

        private void Awake() {
            _textComponent = GetComponent<TextMeshProUGUI>();
        }

        private void OnEnable() {
            SetCurrentMapZone();
            UpdateZoneText();
        }

        private void Update() {
            if (GameController.Instance.currentMapZone == _previousMapZone) {
                return;
            }

            SetCurrentMapZone();
            UpdateZoneText();
        }

        private void SetCurrentMapZone() {
            MapZone currentMapZone = GameController.Instance.currentMapZone;
            _previousMapZone = currentMapZone;
        }

        private void UpdateZoneText() {
            // Retrieve the localized text for the current map zone
            string zoneKey = "mz-" + _previousMapZone.ToString().ToLower();
            _textComponent.text = LocalizationManager.Instance.GetLocalizedText(zoneKey);
        }
    }
}