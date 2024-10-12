using Configuration;
using Enums;
using Mechanics.Movement;
using UnityEngine;

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

            if (isClimbing && Mathf.Abs(_horizontal) > 0.01f) {
                isClimbing = false;
                StopClimbing();
            }

            if (_canClimb && Utils.Keybinds.GetClimbKey()) {
                StartClimbing();
            }

            // _animator.SetBool("Climbing", _isClimbing);
            if (!isClimbing) {
                StopClimbing();
            }
        }

        private void FixedUpdate() {
            // Here some jump or player movement conditions can be checked
            if (isClimbing) {
                _rb.velocity = new Vector2(0, _vertical * climbingSpeed);
                _playerController.velocity.y = 0f;
                _playerController.MovementState = PlayerMovementState.Climb;
                _rb.gravityScale = climbingGravityScale;
                _playerController.gravityModifier = climbingGravityScale;
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
                    _playerController.MovementState = PlayerMovementState.Idle;
                }
                else if (_playerController.velocity.y > 0) {
                    _playerController.MovementState = PlayerMovementState.Jump;
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
        }

        private void StopClimbing() {
            isClimbing = false;
            _rb.gravityScale = _previousGravityScale;
            _playerController.gravityModifier = _previousGravityScale;
        }

        private void ShowClimbIndicator(bool show) {
            // TODO: Add code to show or hide the climb indicator like enabling/disabling a UI GameObject or a message on screen
        }
    }
}