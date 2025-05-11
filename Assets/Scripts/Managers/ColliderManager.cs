using UnityEngine;

namespace Managers {
    public class ColliderManager {
        private Collider2D _collider;
        private Vector2 _originalOffset;
        private Vector2 _originalSize;

        public ColliderManager(Collider2D collider) {
            _collider = collider;

            if (_collider is BoxCollider2D boxCollider) {
                _originalOffset = boxCollider.offset;
                _originalSize = boxCollider.size;
            } else if (_collider is CapsuleCollider2D capsuleCollider) {
                _originalOffset = capsuleCollider.offset;
                _originalSize = capsuleCollider.size;
            }
        }

        public void UpdateCollider(bool isCrouching, Vector2 crouchSize) {
            if (_collider is BoxCollider2D boxCollider) {
                if (isCrouching) {
                    AdjustColliderForCrouch(boxCollider, crouchSize);
                } else {
                    boxCollider.offset = _originalOffset;
                    boxCollider.size = _originalSize;
                }
            } else if (_collider is CapsuleCollider2D capsuleCollider) {
                if (isCrouching) {
                    AdjustColliderForCrouch(capsuleCollider, crouchSize);
                } else {
                    capsuleCollider.offset = _originalOffset;
                    capsuleCollider.size = _originalSize;
                }
            }
        }

        private void AdjustColliderForCrouch(BoxCollider2D boxCollider, Vector2 newSize) {
            float heightDifference = boxCollider.size.y - newSize.y;
            Vector2 newOffset = boxCollider.offset;
            newOffset.y -= heightDifference / 2;

            boxCollider.offset = newOffset;
            boxCollider.size = newSize;
        }

        private void AdjustColliderForCrouch(CapsuleCollider2D capsuleCollider, Vector2 newSize) {
            float heightDifference = capsuleCollider.size.y - newSize.y;
            Vector2 newOffset = capsuleCollider.offset;
            newOffset.y -= heightDifference / 2;

            capsuleCollider.offset = newOffset;
            capsuleCollider.size = newSize;
        }

        public Vector2 GetOriginalOffset() {
            return _originalOffset;
        }

        public Vector2 GetOriginalSize() {
            return _originalSize;
        }
    }
}