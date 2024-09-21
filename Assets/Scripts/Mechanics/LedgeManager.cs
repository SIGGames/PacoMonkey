using UnityEngine;

namespace Mechanics {
    public class LedgeManager {
        private Transform _playerTransform;
        private Rigidbody2D _playerRb;
        private Collider2D _playerCollider;
        private Animator _playerAnimator;
        private LayerMask _ledgeLayerMask;
        private bool _isClimbing;

        public Transform ledgeCheckPoint;
        public float ledgeDetectionDistance = 1.0f;
        public Vector2 ledgeOffset;

        public LedgeManager(Transform playerTransform, Rigidbody2D playerRb, Collider2D playerCollider, Animator playerAnimator, LayerMask ledgeLayerMask) {
            _playerTransform = playerTransform;
            _playerRb = playerRb;
            _playerCollider = playerCollider;
            _playerAnimator = playerAnimator;
            _ledgeLayerMask = ledgeLayerMask;
        }

        public bool DetectLedge() {
            RaycastHit2D hit = Physics2D.Raycast(ledgeCheckPoint.position, Vector2.up, ledgeDetectionDistance, _ledgeLayerMask);
            return hit.collider != null;
        }

        public void ClimbLedge() {
            if (_isClimbing) return;

            _isClimbing = true;
            _playerAnimator.SetTrigger("ledgeClimb");

            // Disable player controls while climbing
            _playerRb.gravityScale = 0;
            _playerRb.velocity = Vector2.zero;

            // Move the player to the final ledge position (adjust with ledgeOffset)
            Vector2 targetPosition = new Vector2(_playerTransform.position.x + ledgeOffset.x, _playerTransform.position.y + ledgeOffset.y);
            _playerTransform.position = targetPosition;

            // Enable gravity and reset climbing state after a short delay (for animation to finish)
            _playerRb.gravityScale = 1;
            _isClimbing = false;
        }
    }
}