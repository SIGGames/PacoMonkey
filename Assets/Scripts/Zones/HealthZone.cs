using Health;
using UnityEngine;

namespace Zones {
    public class HealthZone : MonoBehaviour {
        void OnTriggerEnter2D(Collider2D collider) {
            var playerLives = collider.gameObject.GetComponent<Lives>();
            if (playerLives != null && playerLives.CurrentLives < playerLives.MaxLives) {
                playerLives.IncrementLivesToMax();
            }
        }
    }
}