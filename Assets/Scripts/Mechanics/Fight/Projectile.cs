using UnityEngine;
using static Utils.AnimatorUtils;

namespace Mechanics.Fight {
    public class Projectile : MonoBehaviour {
        private Vector2 _direction;
        private float _speed;
        private int _damage;

        [SerializeField] private Animator projectileAnimator;

        public void Initialize(Vector2 dir, float spd, int dmg) {
            _direction = dir;
            _speed = spd;
            _damage = dmg;
            projectileAnimator.SetTrigger(StartProjectile);
        }

        private void Awake() {
            if (projectileAnimator == null) {
                projectileAnimator = GetComponent<Animator>();
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