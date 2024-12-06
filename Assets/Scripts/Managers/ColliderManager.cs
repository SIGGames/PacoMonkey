using UnityEngine;

namespace Managers {
    public class ColliderManager {
        private Collider2D _collider;
        private Vector2 _crouchSize;
        private Vector2 _originalSize;
        private Vector2 _crouchOffset;
        private Vector2 _originalOffset;

        public ColliderManager(Collider2D collider, Vector2 crouchOffset, Vector2 crouchSize) {
            _collider = collider;

            if (_collider is BoxCollider2D boxCollider) {
                _originalOffset = boxCollider.offset;
                _originalSize = boxCollider.size;
            } else if (_collider is CapsuleCollider2D capsuleCollider) {
                _originalOffset = capsuleCollider.offset;
                _originalSize = capsuleCollider.size;
            }

            _crouchOffset = crouchOffset;
            _crouchSize = crouchSize;
        }

        public void UpdateCollider(bool isCrouching) {
            if (_collider is BoxCollider2D boxCollider) {
                boxCollider.offset = isCrouching ? _crouchOffset : _originalOffset;
                boxCollider.size = isCrouching ? _crouchSize : _originalSize;
            } else if (_collider is CapsuleCollider2D capsuleCollider) {
                capsuleCollider.offset = isCrouching ? _crouchOffset : _originalOffset;
                capsuleCollider.size = isCrouching ? _crouchSize : _originalSize;
            }
        }
    }
}