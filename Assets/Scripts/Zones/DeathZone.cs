using Controllers;
using Managers;
using UnityEngine;

namespace Zones {
    public class DeathZone : MonoBehaviour {
        private void OnTriggerEnter2D(Collider2D col) {
            if (col.gameObject.GetComponent<PlayerController>() == null) {
                return;
            }

            PlayerController pc = CharacterManager.Instance.currentPlayerController;
            pc.lives.Die();

            // We need to simulate the player is grounded in order to trigger the respawn animation
            pc.IsGrounded = true;
        }
    }
}