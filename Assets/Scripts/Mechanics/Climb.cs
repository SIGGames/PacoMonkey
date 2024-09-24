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

        public Climb(bool isClimbing) {
            _isClimbing = isClimbing;
        }


        private void Update() {
            _vertical = Input.GetAxisRaw("Vertical");

            if (_isLadder && Mathf.Abs(_vertical) > 0f) {
                _isClimbing = true;
            }
        }

        private void FixedUpdate() {
            if (_isClimbing) {
                rb.gravityScale = 0f;
                rb.velocity = new Vector2(rb.velocity.x, _vertical * climbingSpeed);
            }
            else {
                rb.gravityScale = GlobalConfiguration.GravityScale;
            }
        }

        private void OnTriggerEnter2D(Collider2D collision) {
            if (collision.CompareTag("Ladder")) {
                _isLadder = true;
                PlayerController.PCInstance.MovementState = PlayerMovementState.Climb;
            }
        }

        private void OnTriggerExit2D(Collider2D collision) {
            if (collision.CompareTag("Ladder")) {
                _isLadder = false;
                _isClimbing = false;
                PlayerController.PCInstance.MovementState = PlayerMovementState.Idle;
            }
        }
    }
}