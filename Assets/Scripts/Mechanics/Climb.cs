using Configuration;
using Enums;
using UnityEngine;

namespace Mechanics {
    public class Climb : MonoBehaviour {
        private static GlobalConfiguration.PlayerConfig _playerConfig;
        private float _vertical;
        [SerializeField] private float climbingSpeed = _playerConfig.climbingSpeed;
        private bool _isLadder;
        private bool _isClimbing;
        [SerializeField] private Rigidbody2D rb;

        public bool IsClimbing() {
            return _isClimbing;
        }

        public void SetClimbingState(bool isClimbing) {
            _isClimbing = isClimbing;
        }

        public void ClimbMovement(float verticalInput) {
            _vertical = verticalInput;
            if (_isLadder && Mathf.Abs(_vertical) > 0f) {
                _isClimbing = true;
                rb.gravityScale = 0f;
                rb.velocity = new Vector2(rb.velocity.x, _vertical * climbingSpeed);
            } else {
                _isClimbing = false;
                rb.gravityScale = GlobalConfiguration.GravityScale;
            }
        }

        private void OnTriggerEnter2D(Collider2D collision) {
            if (collision.CompareTag("Climb")) {
                _isLadder = true;
                PlayerController.PCInstance.MovementState = PlayerMovementState.Climb;
            }
        }

        private void OnTriggerExit2D(Collider2D collision) {
            if (collision.CompareTag("Climb")) {
                _isLadder = false;
                _isClimbing = false;
                PlayerController.PCInstance.MovementState = PlayerMovementState.Idle;
            }
        }
    }
}