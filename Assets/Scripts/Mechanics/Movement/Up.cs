﻿using Controllers;
using Enums;
using Managers;
using UnityEngine;
using static PlayerInput.KeyBinds;

namespace Mechanics.Movement {
    [RequireComponent(typeof(PlayerController))]
    public class Up : MonoBehaviour {
        [SerializeField] private float lookUpOffset = 1.3f;

        private PlayerController _playerController;

        private void Awake() {
            _playerController = GetComponent<PlayerController>();

            if (_playerController == null || CameraManager.Instance == null) {
                Debug.LogError("Up script requires a PlayerController and CameraManager component");
                enabled = false;
            }
        }

        private void Update() {
            if (GetUpKey() && _playerController.movementState == PlayerMovementState.Idle) {
                LookUp();
            }

            if (GetUpKey() ||
                PlayerMovementStateMethods.IsPlayerMoving(_playerController.movementState)) {
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