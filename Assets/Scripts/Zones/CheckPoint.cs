using Controllers;
using Enums;
using Managers;
using UnityEngine;

namespace Zones {
    public class CheckPoint : MonoBehaviour {
        private static CharacterManager CharacterManager => CharacterManager.Instance;
        private PlayerController _playerController;

        private void Awake() {
            _playerController = CharacterManager.currentPlayerController;
        }

        private void OnTriggerEnter2D(Collider2D col) {
            if (col.gameObject.GetComponent<PlayerController>() == null) {
                return;
            }

            _playerController = CharacterManager.currentPlayerController;

            _playerController.respawnPosition = transform.position;
        }
    }
}