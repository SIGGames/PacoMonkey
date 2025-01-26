using Configuration;
using Controllers;
using Enums;
using UnityEditor;
using UnityEngine;
using static PlayerInput.KeyBinds;

namespace Mechanics.Movement {
    [RequireComponent(typeof(PlayerController))]
    public class Climb : MonoBehaviour {
        [Header("Climbing Configuration")]
        [Tooltip("How fast the player will climb up or down")]
        [Range(0, 10)]
        [SerializeField] private float climbingSpeed = GlobalConfiguration.PlayerConfig.ClimbingSpeed;

        [Tooltip("This value sets the player's gravity scale while climbing, so if the player's gravity scale " +
                 "changes while climbing, it will be set to this value")]
        [Range(0, 10)]
        [SerializeField] private float climbingGravityScale;

        [Tooltip("The threshold for horizontal movement while climbing, if the player moves horizontally more than " +
                 "this value, will stop climbing")]
        [Range(0.01f, 1)]
        [SerializeField] private float horizontalMovementThreshold = 0.01f;

        [Tooltip("The tag of the object that the player can climb")]
        [TagSelector]
        [SerializeField] private string climbTag = "Ladder";

        [Tooltip("The layer that will stop the player from climbing")]
        [LayerSelector]
        [SerializeField] private int contactLayer;

        [Header("Climbing State")]
        [Tooltip("Tell if the player is currently climbing")]
        [SerializeField] private bool isClimbing;

        [Space]
        private float _vertical;

        private float _horizontal;

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
            _vertical = GetVerticalAxis();
            _horizontal = GetHorizontalAxis();

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

        private void OnCollisionEnter2D(Collision2D collision) {
            if (collision.gameObject.layer == contactLayer) {
                StopClimbing();
            }
        }

        private void OnCollisionStay2D(Collision2D collision) {
            if (collision.gameObject.layer == contactLayer) {
                StopClimbing();
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
                _playerController.SetVelocity(Vector2.zero);

                if (_playerController.movementState is PlayerMovementState.Run or PlayerMovementState.Walk) {
                    _playerController.jumpState = JumpState.InFlight;
                }
                else {
                    _playerController.jumpState = JumpState.Grounded;
                }
            }
        }

        private void ShowClimbIndicator(bool show) {
            // TODO: Add code to show or hide the climb indicator like enabling/disabling a UI GameObject or a message on screen
        }
    }
}