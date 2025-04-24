using Managers;
using Mechanics.Movement;
using UnityEngine;
using static Utils.TagUtils;

namespace Zones {
    public class NoHoldZone : MonoBehaviour {
        private void OnTriggerEnter2D(Collider2D col) {
            if (!col.CompareTag(Player)) {
                return;
            }

            Hold playerHold = GetHoldComponent();

            if (playerHold != null) {
                playerHold.enabled = false;
            }
        }

        private void OnTriggerExit2D(Collider2D col) {
            if (!col.CompareTag(Player)) {
                return;
            }

            Hold playerHold = GetHoldComponent();

            if (playerHold != null) {
                playerHold.enabled = true;
            }
        }

        private static Hold GetHoldComponent() {
            return CharacterManager.Instance.currentPlayerController.gameObject.GetComponent<Hold>();
        }
    }
}