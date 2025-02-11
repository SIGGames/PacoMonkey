using Controllers;
using Enums;
using Managers;
using UnityEngine;

namespace Zones {
    public class CheckPoint : MonoBehaviour {
        private static CharacterManager CharacterManager => CharacterManager.Instance;
        private Character _currentCharacter;
        private PlayerController _playerController;

        private void Awake() {
            _currentCharacter = CharacterManager.GetCurrentCharacter();
            _playerController = CharacterManager.GetCurrentCharacterController();
        }

        private void OnTriggerEnter2D(Collider2D col) {
            if (col.gameObject.GetComponent<PlayerController>() == null) {
                return;
            }

            if (CharacterManager.GetCurrentCharacter() != _currentCharacter) {
                _playerController = CharacterManager.GetCurrentCharacterController();
                _currentCharacter = CharacterManager.GetCurrentCharacter();
            }

            _playerController.respawnPosition = transform.position;
        }
    }
}