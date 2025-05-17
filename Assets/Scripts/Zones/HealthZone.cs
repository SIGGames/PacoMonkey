using Health;
using Managers;
using UnityEngine;
using static Utils.TagUtils;

namespace Zones {
    public class HealthZone : MonoBehaviour {
        private Lives _playerLives;

        [Range(0, 10)]
        [SerializeField] private float timeBetweenIncrement = 1f;

        [Range(0, 10)]
        [SerializeField] private float healthIncrement = 1f;

        private void OnTriggerEnter2D(Collider2D col) {
            if (!col.CompareTag(Player)) {
                return;
            }

            _playerLives = CharacterManager.Instance.currentPlayerController.lives;
            if (_playerLives.CurrentLives < _playerLives.MaxLives) {
                _playerLives.IncrementLivesToMaxSlow(timeBetweenIncrement, healthIncrement);
            }
        }

        private void OnTriggerExit2D(Collider2D col) {
            if (!col.CompareTag(Player)) {
                return;
            }

            _playerLives = CharacterManager.Instance.currentPlayerController.lives;
            _playerLives.StopIncrementingLives();
        }
    }
}