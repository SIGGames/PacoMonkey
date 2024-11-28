using Health;
using UnityEngine;

namespace Zones {
    public class HealthZone : MonoBehaviour {
        private Lives _playerLives;

        [Range(0, 10)]
        [SerializeField] private float timeBetweenIncrement = 1f;

        [Range(0, 10)]
        [SerializeField] private float healthIncrement = 1f;

        void OnTriggerEnter2D(Collider2D collider) {
            _playerLives = collider.gameObject.GetComponent<Lives>();
            if (_playerLives != null && _playerLives.CurrentLives < _playerLives.MaxLives) {
                _playerLives.IncrementLivesToMaxSlow(timeBetweenIncrement, healthIncrement);
            }
        }

        void OnTriggerExit2D(Collider2D collider) {
            if (_playerLives != null && collider.gameObject.GetComponent<Lives>() == _playerLives) {
                _playerLives.StopIncrementingLives();
                _playerLives = null;
            }
        }
    }
}