using System.Collections;
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

        public void UpdateCollider(bool isCrouching, Vector2 crouchOffset, Vector2 crouchSize) {
            if (_collider is BoxCollider2D boxCollider) {
                boxCollider.offset = isCrouching ? crouchOffset : _originalOffset;
                boxCollider.size = isCrouching ? crouchSize : _originalSize;
            } else if (_collider is CapsuleCollider2D capsuleCollider) {
                capsuleCollider.offset = isCrouching ? crouchOffset : _originalOffset;
                capsuleCollider.size = isCrouching ? crouchSize : _originalSize;
            }
        }

        public void UpdateColliderWithDelay(MonoBehaviour context, bool isCrouching, Vector2 offset, Vector2 size,
            float delay) {
            context.StartCoroutine(CallUpdateColliderWithDelay(isCrouching, offset, size, delay));
        }

        private IEnumerator CallUpdateColliderWithDelay(bool isCrouching, Vector2 offset, Vector2 size, float delay) {
            yield return new WaitForSeconds(delay);
            UpdateCollider(isCrouching, offset, size);
        }
    }
}