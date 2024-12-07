using Controllers;
using Enums;
using Managers;
using UnityEngine;
using Utils;

namespace Mechanics.Movement {
    [RequireComponent(typeof(PlayerController))]
    public class Up : MonoBehaviour {
        [SerializeField] private float lookUpOffset = 1.3f;

        private PlayerController _playerController;
        private bool _isCameraManagerValid;

        private void Awake() {
            _playerController = GetComponent<PlayerController>();
            _isCameraManagerValid = CameraManager.Instance != null;

            if (_playerController == null) {
                Debug.LogWarning("PlayerController is not set.");
            }

            if (!_isCameraManagerValid) {
                Debug.LogWarning("CameraManager is not set.");
            }
        }

        private void Update() {
            if (KeyBinds.GetUpKey()) {
                LookUp();
            }

            if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.UpArrow)) {
                CameraManager.Instance.ResetCamera();
            }
        }

        private void LookUp() {
            if (!_playerController.IsGrounded) {
                return;
            }

            _playerController.SetMovementState(PlayerMovementState.Up);

            CameraManager.Instance.SetOffset(new Vector2(0, lookUpOffset));
        }
    }
}