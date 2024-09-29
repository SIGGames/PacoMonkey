﻿using Configuration;
using Enums;
using UnityEngine;

namespace Mechanics {
    public class Climb : MonoBehaviour {
        private float _vertical;
        [SerializeField] private float climbingSpeed = GlobalConfiguration.PlayerConfig.ClimbingSpeed;
        [SerializeField] private bool isClimbing;

        private Rigidbody2D _rb;
        private PlayerController _playerController;
        // private Animator _animator;

        private void Awake() {
            _rb = GetComponent<Rigidbody2D>();
            _playerController = GetComponent<PlayerController>();
            // _animator = GetComponent<Animator>();
        }

        private void Update() {
            _vertical = Input.GetAxis("Vertical");

            if (isClimbing && Utils.Keybinds.GetClimbKey()) {
                StartClimbing();
            }

            // _animator.SetBool("Climbing", _isClimbing);
            if (!isClimbing) {
                StopClimbing();
            }
        }

        private void FixedUpdate() {
            if (isClimbing && _playerController.jumpState == JumpState.Grounded) {
                _rb.gravityScale = 0f;
                _rb.velocity = new Vector2(0, _vertical * climbingSpeed);
                _playerController.velocity.y = 0f;
                _playerController.MovementState = PlayerMovementState.Climb;
            }
            else {
                _rb.gravityScale = GlobalConfiguration.GravityScale;
            }
        }

        private void OnTriggerEnter2D(Collider2D collision) {
            if (collision.CompareTag("Ladder")) {
                isClimbing = true;
                ShowClimbIndicator(true);
            }
        }

        private void OnTriggerExit2D(Collider2D collision) {
            if (collision.CompareTag("Ladder")) {
                isClimbing = false;
                StopClimbing();
                ShowClimbIndicator(false);

                switch (_playerController.velocity.y) {
                    case 0 when _playerController.jumpState == JumpState.Grounded:
                        _playerController.MovementState = PlayerMovementState.Idle;
                        break;
                    case > 0 when _playerController.jumpState != JumpState.Grounded:
                        _playerController.MovementState = PlayerMovementState.Jump;
                        _playerController.jumpState = JumpState.InFlight;
                        break;
                }
            }
        }

        private void StartClimbing() {
            _rb.gravityScale = 0f;
        }

        private void StopClimbing() {
            _rb.gravityScale = GlobalConfiguration.GravityScale;
        }

        private void ShowClimbIndicator(bool show) {
            // TODO: Add code to show or hide the climb indicator like enabling/disabling a UI GameObject or a message on screen
        }
    }
}