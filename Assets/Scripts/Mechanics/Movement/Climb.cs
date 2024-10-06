using Configuration;
using Enums;
using Mechanics.Movement;
using UnityEngine;
using UnityEngine.Serialization;

namespace Mechanics {
    public class Climb : MonoBehaviour {
        private float _vertical;
        [FormerlySerializedAs("isClimbing")] [SerializeField] private bool canClimb;

        [Range(0, 10)]
        [SerializeField] private float climbingSpeed = GlobalConfiguration.PlayerConfig.ClimbingSpeed;

        [Range(0, 10)]
        [SerializeField] private float climbingGravityScale;

        private Rigidbody2D _rb;
        private PlayerController _playerController;
        private float _previousGravityScale;
        // private Animator _animator;

        private void Awake() {
            _rb = GetComponent<Rigidbody2D>();
            _playerController = GetComponent<PlayerController>();
            // _animator = GetComponent<Animator>();
            _previousGravityScale = _rb.gravityScale;
        }

        private void Update() {
            _vertical = Input.GetAxis("Vertical");

            if (canClimb && Utils.Keybinds.GetClimbKey()) {
                StartClimbing();
            }

            // _animator.SetBool("Climbing", _isClimbing);
            if (!canClimb) {
                StopClimbing();
            }
        }

        private void FixedUpdate() {
            // Here some jump or player movement conditions can be checked
            if (canClimb) {
                _rb.velocity = new Vector2(0, _vertical * climbingSpeed);
                _playerController.velocity.y = 0f;
                _playerController.MovementState = PlayerMovementState.Climb;
                _rb.gravityScale = climbingGravityScale;
                _playerController.gravityModifier = climbingGravityScale;
            }
        }

        private void OnTriggerEnter2D(Collider2D collision) {
            if (collision.CompareTag("Ladder")) {
                canClimb = true;
                ShowClimbIndicator(true);
            }
        }

        private void OnTriggerExit2D(Collider2D collision) {
            if (collision.CompareTag("Ladder")) {
                canClimb = false;
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
            if (!canClimb) {
                _previousGravityScale = _rb.gravityScale;
            }

            _rb.gravityScale = climbingGravityScale;
            _playerController.gravityModifier = climbingGravityScale;
        }

        private void StopClimbing() {
            _rb.gravityScale = _previousGravityScale;
            _playerController.gravityModifier = _previousGravityScale;
        }

        private void ShowClimbIndicator(bool show) {
            // TODO: Add code to show or hide the climb indicator like enabling/disabling a UI GameObject or a message on screen
        }
    }
}