using Configuration;
using Enums;
using Mechanics.Movement;
using UnityEditor.UIElements;
using UnityEngine;
using static Mechanics.Utils.Keybinds;

namespace Mechanics {
    [RequireComponent(typeof(PlayerController))]
    public class Climb : MonoBehaviour {
        private float _vertical;
        private float _horizontal;
        [SerializeField] private bool isClimbing;

        [Tooltip("The tag of the object that the player can climb")]
        [SerializeField] private string climbTag = "Ladder";

        [Range(0, 10)]
        [Tooltip("How fast the player will climb up or down")]
        [SerializeField] private float climbingSpeed = GlobalConfiguration.PlayerConfig.ClimbingSpeed;

        [Range(0, 10)]
        [Tooltip("This value sets the player's gravity scale while climbing, so if the player's gravity scale changes " +
                 "while climbing, it will be set to this value")]
        [SerializeField] private float climbingGravityScale;

        [Range(0, 1)]
        [Tooltip("The threshold for horizontal movement while climbing, if the player moves horizontally more than this " +
                 "value, will stop climbing")]
        [SerializeField] private float horizontalMovementThreshold = 0.01f;

        private Rigidbody2D _rb;
        private PlayerController _playerController;

        private float _previousGravityScale;

        private bool _canClimb;

        private void Awake() {
            _rb = GetComponent<Rigidbody2D>();
            _playerController = GetComponent<PlayerController>();
            _previousGravityScale = _playerController.gravityModifier;
        }

        private void Update() {
            _vertical = Input.GetAxis("Vertical");
            _horizontal = Input.GetAxis("Horizontal");

            if (isClimbing) {
                if (Mathf.Abs(_horizontal) > horizontalMovementThreshold) {
                    StopClimbing();
                }
                else {
                    _rb.gravityScale = climbingGravityScale;
                    _playerController.gravityModifier = climbingGravityScale;
                    _rb.velocity = new Vector2(0, _vertical * climbingSpeed);
                    _playerController.velocity = Vector2.zero;
                    _playerController.SetMovementState(PlayerMovementState.Climb);
                }
            }

            if (_canClimb && GetClimbKey()) {
                StartClimbing();
            }
        }

        private void OnTriggerEnter2D(Collider2D collision) {
            if (collision.CompareTag(climbTag)) {
                _canClimb = true;
                ShowClimbIndicator(true);
            }
        }

        private void OnTriggerExit2D(Collider2D collision) {
            if (collision.CompareTag(climbTag)) {
                _canClimb = false;
                StopClimbing();
                ShowClimbIndicator(false);

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
                _previousGravityScale = _playerController.gravityModifier;
                isClimbing = true;
                _rb.gravityScale = climbingGravityScale;
                _playerController.gravityModifier = climbingGravityScale;
                _playerController.velocity = Vector2.zero;
            }
        }

        private void StopClimbing() {
            if (isClimbing) {
                isClimbing = false;
                _rb.gravityScale = _previousGravityScale;
                _playerController.gravityModifier = _previousGravityScale;
            }
        }

        private void ShowClimbIndicator(bool show) {
            // TODO: Add code to show or hide the climb indicator like enabling/disabling a UI GameObject or a message on screen
        }
    }
}