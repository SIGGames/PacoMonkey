using Configuration;
using Enums;
using Mechanics.Movement;
using UnityEngine;
using static Mechanics.Utils.Keybinds;

namespace Mechanics {
    [RequireComponent(typeof(PlayerController))]
    public class Climb : MonoBehaviour {
        private float _vertical;
        private float _horizontal;
        [SerializeField] private bool isClimbing;

        [Range(0, 10)]
        [SerializeField] private float climbingSpeed = GlobalConfiguration.PlayerConfig.ClimbingSpeed;

        [Range(0, 10)]
        [SerializeField] private float climbingGravityScale;

        private Rigidbody2D _rb;
        private PlayerController _playerController;

        private float _previousGravityScale;

        // private Animator _animator;
        private bool _canClimb;

        private void Awake() {
            _rb = GetComponent<Rigidbody2D>();
            _playerController = GetComponent<PlayerController>();
            _previousGravityScale = _rb.gravityScale;
        }

        private void Update() {
            _vertical = Input.GetAxis("Vertical");
            _horizontal = Input.GetAxis("Horizontal");

            if (isClimbing) {
                if (Mathf.Abs(_horizontal) > 0.01f) {
                    isClimbing = false;
                    StopClimbing();
                }
                else {
                    _rb.velocity = new Vector2(0, _vertical * climbingSpeed);
                    _playerController.velocity = Vector2.zero;
                    _playerController.SetMovementState(PlayerMovementState.Climb);
                    _rb.gravityScale = climbingGravityScale;
                    _playerController.gravityModifier = climbingGravityScale;
                }
            }
            else {
                _rb.gravityScale = _previousGravityScale;
                _playerController.gravityModifier = 1f;
            }

            if (_canClimb && GetClimbKey()) {
                StartClimbing();
            }

            // _animator.SetBool("Climbing", _isClimbing);
            if (!isClimbing) {
                StopClimbing();
            }
        }

        private void OnTriggerEnter2D(Collider2D collision) {
            if (collision.CompareTag("Ladder")) {
                _canClimb = true;
                ShowClimbIndicator(true);
            }
        }

        private void OnTriggerExit2D(Collider2D collision) {
            if (collision.CompareTag("Ladder")) {
                _canClimb = false;
                isClimbing = false;
                StopClimbing();
                ShowClimbIndicator(false);

                // Reset player movement state
                if (_playerController.velocity.y == 0) {
                    _playerController.SetMovementState(PlayerMovementState.Idle);
                }
                else if (_playerController.velocity.y > 0) {
                    _playerController.SetMovementState(PlayerMovementState.Jump);
                    _playerController.jumpState = JumpState.InFlight;
                }
            }
        }

        private void StartClimbing() {
            if (!isClimbing) {
                _previousGravityScale = _rb.gravityScale;
            }

            isClimbing = true;
            _rb.gravityScale = climbingGravityScale;
            _playerController.gravityModifier = climbingGravityScale;
            _playerController.velocity = Vector2.zero;
        }

        private void StopClimbing() {
            isClimbing = false;
            _rb.gravityScale = _previousGravityScale;
            _playerController.gravityModifier = 1f;
        }

        private void ShowClimbIndicator(bool show) {
            // TODO: Add code to show or hide the climb indicator like enabling/disabling a UI GameObject or a message on screen
        }
    }
}