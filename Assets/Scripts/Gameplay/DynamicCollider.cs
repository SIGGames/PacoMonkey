using UnityEngine;

namespace Gameplay {
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(BoxCollider2D))]
    public class DynamicCollider : MonoBehaviour {
        private SpriteRenderer _spriteRenderer;
        private BoxCollider2D _boxCollider;

        [Header("Offset Adjustments")]
        [SerializeField] private Vector2 sizeMultiplier = Vector2.one;

        [SerializeField] private Vector2 manualOffset = Vector2.zero;

        private void Awake() {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _boxCollider = GetComponent<BoxCollider2D>();
        }

        private void Start() {
            UpdateCollider();
        }

        private void Update() {
            UpdateCollider();
        }

        private void UpdateCollider() {
            if (_spriteRenderer.sprite == null) return;

            _boxCollider.size = Vector2.Scale(_spriteRenderer.sprite.bounds.size, sizeMultiplier);

            _boxCollider.offset = _spriteRenderer.sprite.bounds.center + (Vector3)manualOffset;
        }
    }
}