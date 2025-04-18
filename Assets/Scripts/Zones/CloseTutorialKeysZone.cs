using UnityEngine;
using static Utils.TagUtils;

namespace Zones {
    public class CloseTutorialKeysZone : MonoBehaviour {
        [SerializeField] private ShowTutorialKeysZone showTutorialKeysZone;

        private void OnTriggerEnter2D(Collider2D col) {
            if (!col.CompareTag(Player) || showTutorialKeysZone == null) {
                return;
            }

            showTutorialKeysZone.ClosePopUp(true);
        }
    }
}