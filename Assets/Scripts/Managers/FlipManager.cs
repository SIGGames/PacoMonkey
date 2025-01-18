using UnityEngine;

namespace Managers {
    public class FlipManager {
        private readonly SpriteRenderer _spriteRenderer;
        private readonly BoxCollider2D _boxCollider;
        private readonly Animator _animator;
        private float _offsetChange;
        private bool _isFacingRight;
        private static readonly int IsFlipping = Animator.StringToHash("isFlipping");

        public FlipManager(SpriteRenderer spriteRenderer,
            BoxCollider2D boxCollider,
            Animator animator,
            float offsetChange,
            bool initialFacingRight = true
        ) {
            _spriteRenderer = spriteRenderer;
            _boxCollider = boxCollider;
            _animator = animator;
            _offsetChange = offsetChange;
            _isFacingRight = initialFacingRight;

            if (_spriteRenderer == null || boxCollider == null || animator == null) {
                Debug.LogError("FlipManager requires a SpriteRenderer, BoxCollider2D, and Animator component");
            }

            UpdateFlip();
            UpdateColliderOffset();
        }

        public bool Flip(bool shouldFaceRight) {
            if (_isFacingRight == shouldFaceRight) {
                return _isFacingRight;
            }

            AnimateFlip(true);
            _isFacingRight = shouldFaceRight;
            UpdateFlip();
            UpdateColliderOffset();

            return _isFacingRight;
        }

        public void AnimateFlip(bool isFlipping) {
            _animator.SetBool(IsFlipping, isFlipping);
        }

        private void UpdateFlip() {
            _spriteRenderer.flipX = !_isFacingRight;
        }

        private void UpdateColliderOffset() {
            Vector2 offset = _boxCollider.offset;
            offset.x = _isFacingRight ? -_offsetChange : _offsetChange;
            _boxCollider.offset = offset;
        }
    }
}