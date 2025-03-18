using Controllers;
using Managers;
using UnityEngine;
using static Utils.PlayerPrefsKeys;

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

            SaveCheckPoint();
        }

        private void SaveCheckPoint() {
            PlayerPrefs.SetFloat(RespawnPositionX, _playerController.respawnPosition.x);
            PlayerPrefs.SetFloat(RespawnPositionY, _playerController.respawnPosition.y);
            PlayerPrefs.Save();
        }
    }
}