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
            _originalOffset = collider.offset;
            _originalSize = collider.bounds.size;
            _crouchOffset = crouchOffset;
            _crouchSize = crouchSize;
        }

        public void UpdateCollider(bool isCrouching) {
            if (_collider is BoxCollider2D boxCollider) {
                boxCollider.offset = isCrouching ? _crouchOffset : _originalOffset;
                boxCollider.size = isCrouching ? _crouchSize : _originalSize;
            }
        }
    }
}