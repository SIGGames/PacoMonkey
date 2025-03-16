using Controllers;
using Enums;
using Localization;
using TMPro;
using UnityEngine;

namespace UI.TextSetters {
    public class SetZoneText : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI textComponent;

        private MapZone _previousMapZone = MapZone.Unknown;

        private void Update() {
            if (GameController.Instance.currentMapZone == _previousMapZone) {
                return;
            }

            MapZone currentMapZone = GameController.Instance.currentMapZone;
            _previousMapZone = currentMapZone;

            // Retrieve the localized text for the current map zone
            string zoneKey = "mz-" + currentMapZone.ToString().ToLower();
            textComponent.text = LocalizationManager.Instance.GetLocalizedText(zoneKey);
        }
    }
}