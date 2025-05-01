using Managers;
using Mechanics.Movement;
using UnityEngine;
using static Utils.TagUtils;

namespace Zones {
    public class NoHoldV2Zone : MonoBehaviour {
        private void OnTriggerEnter2D(Collider2D col) {
            if (!col.CompareTag(Player)) {
                return;
            }

            HoldV2 playerHold = GetHoldComponent();

            if (playerHold != null) {
                playerHold.enabled = false;
            }
        }

        private void OnTriggerExit2D(Collider2D col) {
            if (!col.CompareTag(Player)) {
                return;
            }

            HoldV2 playerHold = GetHoldComponent();

            if (playerHold != null) {
                playerHold.enabled = true;
            }
        }

        private static HoldV2 GetHoldComponent() {
            return CharacterManager.Instance.currentPlayerController.gameObject.GetComponent<HoldV2>();
        }
    }
}