using Managers;
using UnityEngine;
using static Utils.TagUtils;

namespace Zones {
    public class CloseTutorialKeysZone : MonoBehaviour {
        private void OnTriggerEnter2D(Collider2D col) {
            if (!col.CompareTag(Player)) {
                return;
            }

            PopUpManager.Instance.ClosePopUp();
        }
    }
}