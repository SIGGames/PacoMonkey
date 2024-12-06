using UnityEngine;

namespace Managers {
    public class FlipManager {
        private SpriteRenderer _spriteRenderer;
        private BoxCollider2D _boxCollider;
        private float _offsetChange;
        private bool _isFacingRight;

        public FlipManager(SpriteRenderer spriteRenderer,
            BoxCollider2D boxCollider,
            float offsetChange,
            bool initialFacingRight = true
        ) {
            _spriteRenderer = spriteRenderer;
            _boxCollider = boxCollider;
            _offsetChange = offsetChange;
            _isFacingRight = initialFacingRight;

            UpdateFlip();
            UpdateColliderOffset();
        }

        public bool Flip(bool shouldFaceRight) {
            if (_isFacingRight == shouldFaceRight) {
                return _isFacingRight;
            }

            _isFacingRight = shouldFaceRight;
            UpdateFlip();
            UpdateColliderOffset();

            return _isFacingRight;
        }

        public bool IsFacingRight() {
            return _isFacingRight;
        }

        private void UpdateFlip() {
            _spriteRenderer.flipX = !_isFacingRight;
        }

        private void UpdateColliderOffset() {
            if (_boxCollider == null) {
                return;
            }

            Vector2 offset = _boxCollider.offset;
            offset.x = _isFacingRight ? -_offsetChange : _offsetChange;
            _boxCollider.offset = offset;
        }
    }
}