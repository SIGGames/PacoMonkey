using Enums;
using Mechanics.Utils;
using UnityEditor;
using UnityEngine;

namespace Mechanics.Movement {
    [RequireComponent(typeof(PlayerController))]
    public class WallClimb : MonoBehaviour {
        [Header("Wall Climb Configuration")]
        [TagSelector]
        [SerializeField] private string wallTag = "Wall";

        [LayerSelector]
        [SerializeField] private int wallLayer;

        [Tooltip("Gravity scale while attached to a wall")]
        [Range(0, 10)]
        [SerializeField] private float wallGravityScale = 0.5f;

        [Tooltip("How fast the player can climb the wall")]
        [Range(0, 10)]
        [SerializeField] private float climbingSpeed = 3f;

        [Tooltip("Jump force away from wall")]
        [Range(0, 20)]
        [SerializeField] private Vector2 wallJumpForce = new Vector2(10f, 15f);

        private PlayerController _playerController;
        private Rigidbody2D _rb;

        private bool _isTouchingWall;
        private bool _isClimbingWall;

        private void Awake() {
            _playerController = GetComponent<PlayerController>();
            _rb = GetComponent<Rigidbody2D>();
        }

        private void Update() {
            if (_isTouchingWall && Keybinds.GetClimbKey()) {
                StartWallClimb();
            }

            if (_isClimbingWall) {
                HandleWallClimb();
            }

            if (_isClimbingWall && Input.GetButtonDown("Jump")) {
                WallJump();
            }
        }

        private void OnTriggerEnter2D(Collider2D collision) {
            if (collision.CompareTag(wallTag) || collision.gameObject.layer == wallLayer) {
                _isTouchingWall = true;
            }
        }

        private void OnTriggerExit2D(Collider2D collision) {
            if (collision.CompareTag(wallTag) || collision.gameObject.layer == wallLayer) {
                StopWallClimb();
                _isTouchingWall = false;
            }
        }

        private void StartWallClimb() {
            if (!_isClimbingWall) {
                _isClimbingWall = true;
                _rb.gravityScale = wallGravityScale;
                _playerController.velocity = Vector2.zero;
                _playerController.SetMovementState(PlayerMovementState.Climb);
            }
        }

        private void HandleWallClimb() {
            float vertical = Input.GetAxis("Vertical");
            _rb.velocity = new Vector2(0, vertical * climbingSpeed);

            if (Mathf.Abs(vertical) < 0.01f) {
                _playerController.SetMovementState(PlayerMovementState.Idle);
            }
            else {
                _playerController.SetMovementState(PlayerMovementState.Climb);
            }
        }

        private void WallJump() {
            _isClimbingWall = false;
            _rb.gravityScale = _playerController.gravityModifier;
            _rb.velocity = wallJumpForce * new Vector2(_playerController.IsFacingRight() ? -1 : 1, 1);
            _playerController.SetMovementState(PlayerMovementState.Jump);
        }

        private void StopWallClimb() {
            if (_isClimbingWall) {
                _isClimbingWall = false;
                _rb.gravityScale = _playerController.gravityModifier;
            }
        }
    }
}