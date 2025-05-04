using Managers;
using UnityEngine;
using static Utils.TagUtils;

namespace Gameplay {
    [RequireComponent(typeof(Collider2D), typeof(SpriteRenderer))]
    public class HiddenElement : MonoBehaviour {
        [Header("Hidden Element Settings")]
        [SerializeField] private bool isHidden = true;

        private string _elementId;
        private SpriteRenderer _spriteRenderer;

        private void Awake() {
            _spriteRenderer = GetComponent<SpriteRenderer>();

            if (_spriteRenderer.sprite == null) {
                Debug.LogWarning("Hidden element does not have any sprite", this);
                enabled = false;
            }
        }

        private void Start() {
            int posX = Mathf.RoundToInt(transform.position.x * 100f);
            int posY = Mathf.RoundToInt(transform.position.y * 100f);
            _elementId = $"{gameObject.name}_{posX}_{posY}";

            bool picked = HiddenElementsManager.Instance.IsElementPicked(_elementId);
            ShowSprite(picked);
        }

        private void OnTriggerEnter2D(Collider2D other) {
            if (other.CompareTag(Player) && isHidden) {
                HiddenElementsManager.Instance.RegisterHiddenElement(_elementId);
                ShowSprite(true);
            }
        }

        public void ShowSprite(bool show) {
            if (_spriteRenderer != null) {
                _spriteRenderer.enabled = show;
            }
            isHidden = !show;
        }
    }
}