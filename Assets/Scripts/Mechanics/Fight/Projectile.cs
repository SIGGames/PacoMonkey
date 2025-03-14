﻿using Controllers;
using UnityEngine;
using static Utils.AnimatorUtils;
using static Utils.TagUtils;
using static Utils.LayerUtils;

namespace Mechanics.Fight {
    public class Projectile : MonoBehaviour {
        private Vector2 _direction;
        private float _speed;
        private int _damage;
        private float _duration;

        [SerializeField] private Animator projectileAnimator;
        [SerializeField] private SpriteRenderer spriteRenderer;

        public void Initialize(Vector2 dir, float spd, int dmg, float duration) {
            _direction = dir;
            _speed = spd;
            _damage = dmg;
            _duration = duration;
            projectileAnimator.SetTrigger(StartProjectile);
            spriteRenderer.flipX = dir.x < 0;
            Invoke(nameof(DestroyProjectile), _duration);
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

        private void DestroyProjectile() {
            Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D collision) {
            // Check if the projectile collides with the ground
            if (GetBitMask(collision.gameObject.layer) == Ground.value || collision.CompareTag(ProjectileTag)) {
                DestroyProjectile();
                return;
            }

            // Check if the projectile collides with an enemy
            EnemyController enemyController = collision.GetComponent<EnemyController>();
            if (enemyController != null) {
                enemyController.TakeDamage(_damage);
                DestroyProjectile();
            }
        }
    }
}