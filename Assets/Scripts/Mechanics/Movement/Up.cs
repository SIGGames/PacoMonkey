using Controllers;
using Enums;
using Managers;
using UnityEngine;
using Utils;

namespace Mechanics.Movement {
    public class Up : MonoBehaviour {
        [SerializeField] private float lookUpOffset = 1.3f;

        private PlayerController _playerController;

        private void Awake() {
            _playerController = GetComponent<PlayerController>();
        }

        private void Update() {
            if (KeyBinds.GetUpKey()) {
                LookUp();
            }
        }

        private void LookUp() {
            if (!_playerController.IsGrounded) {
                return;
            }

            if (_playerController != null) {
                _playerController.SetMovementState(PlayerMovementState.Up);
            }

            if (CameraManager.Instance != null) {
                CameraManager.Instance.SetOffset(new Vector2(0, lookUpOffset));
            }
        }
    }
}