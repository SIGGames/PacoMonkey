using UnityEngine;
using static Utils.AnimatorUtils;

namespace Mechanics.Fight {
    public class Projectile : MonoBehaviour {
        private Vector2 _direction;
        private float _speed;
        private int _damage;

        [SerializeField] private Animator projectileAnimator;
        [SerializeField] private SpriteRenderer spriteRenderer;

        public void Initialize(Vector2 dir, float spd, int dmg) {
            _direction = dir;
            _speed = spd;
            _damage = dmg;
            projectileAnimator.SetTrigger(StartProjectile);
            spriteRenderer.flipX = dir.x < 0;
        }

        private void Awake() {
            if (projectileAnimator == null) {
                projectileAnimator = GetComponent<Animator>();
            }

            if (spriteRenderer == null) {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }
        }

        private void Update() {
            transform.Translate(_direction * (_speed * Time.deltaTime));
        }

        private void DestroyGameObject() {
            Destroy(gameObject);
        }
    }
}