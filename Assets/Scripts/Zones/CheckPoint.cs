using System.Collections.Generic;
using Controllers;
using UnityEngine;
using static Utils.PlayerPrefsKeys;
using static Utils.TagUtils;

namespace Zones {
    public class CheckPoint : MonoBehaviour {
        private List<PlayerController> _playerControllers = new();

        private void Awake() {
            _playerControllers = new List<PlayerController>(FindObjectsOfType<PlayerController>(true));
        }

        private void OnTriggerEnter2D(Collider2D col) {
            if (!col.CompareTag(Player)) {
                return;
            }

            foreach (PlayerController playerController in _playerControllers) {
                playerController.respawnPosition = transform.position;
            }

            SaveCheckPoint();
        }

        private void SaveCheckPoint() {
            PlayerPrefs.SetFloat(RespawnPositionX, transform.position.x);
            PlayerPrefs.SetFloat(RespawnPositionY, transform.position.y);
            PlayerPrefs.Save();
        }
    }
}