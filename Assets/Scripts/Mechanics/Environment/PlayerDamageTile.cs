using Controllers;
using UnityEditor;
using UnityEngine;

namespace Mechanics.Environment {
    public class PlayerDamageTile : MonoBehaviour {
        [TagSelector]
        [SerializeField] private string damageTagTile;

        private PlayerController _player;

        private void Awake() {
            _player = GetComponent<PlayerController>();
        }

        private void OnTriggerEnter2D(Collider2D other) {
            if (other.CompareTag(damageTagTile) && other.gameObject.layer == LayerMask.NameToLayer("Ground")) {
                _player.lives.DecrementLives();
            }
        }
    }
}