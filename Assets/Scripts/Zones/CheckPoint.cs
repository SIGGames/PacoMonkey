using Controllers;
using Enums;
using Managers;
using UnityEngine;

namespace Zones {
    public class CheckPoint : MonoBehaviour {
        [SerializeField] private CharacterManager characterManager;
        private Character _currentCharacter;
        private PlayerController _playerController;

        private void Awake() {
            if (characterManager == null) {
                characterManager = FindObjectOfType<CharacterManager>();
            }

            _currentCharacter = characterManager.GetCurrentCharacter();
            _playerController = characterManager.GetCurrentCharacterController();
        }

        private void OnTriggerEnter2D(Collider2D col) {
            if (col.gameObject.GetComponent<PlayerController>() == null) {
                return;
            }

            if (characterManager.GetCurrentCharacter() != _currentCharacter) {
                _playerController = characterManager.GetCurrentCharacterController();
                _currentCharacter = characterManager.GetCurrentCharacter();
            }

            _playerController.respawnPosition = transform.position;
        }
    }
}