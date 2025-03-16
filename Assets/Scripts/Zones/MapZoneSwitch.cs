using Controllers;
using Enums;
using UnityEngine;
using static Utils.TagUtils;

namespace Zones {
    [RequireComponent(typeof(Collider2D))]
    public class MapZoneSwitch : MonoBehaviour {
        [SerializeField]
        private MapZone mapZone = MapZone.Unknown;

        private void OnTriggerEnter2D(Collider2D col) {
            if (col.gameObject.CompareTag(Player)) {
                GameController.Instance.currentMapZone = mapZone;
            }
        }
    }
}