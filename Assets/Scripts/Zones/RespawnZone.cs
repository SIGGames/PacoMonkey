using Controllers;
using Managers;
using UnityEngine;

namespace Zones {
    public class RespawnZone : MonoBehaviour {
        private void OnTriggerEnter2D(Collider2D col) {
            if (col.gameObject.GetComponent<PlayerController>() == null) {
                return;
            }

            PlayerController pc = CharacterManager.Instance.currentPlayerController;
            pc.Respawn();
        }
    }
}