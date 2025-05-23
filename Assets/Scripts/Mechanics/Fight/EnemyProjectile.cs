﻿using System.Collections;
using Controllers;
using Enums;
using Managers;
using UnityEngine;
using static Utils.AnimatorUtils;
using static Utils.TagUtils;
using static Utils.LayerUtils;

namespace Mechanics.Fight {
    public class EnemyProjectile : MonoBehaviour {
        private Vector2 _direction;
        private float _speed;
        private float _damage;
        private float _duration;

        [SerializeField] private Animator projectileAnimator;
        [SerializeField] private SpriteRenderer spriteRenderer;

        public void Initialize(Vector2 dir, float spd, float dmg, float duration) {
            _direction = dir;
            _speed = spd;
            _damage = dmg;
            _duration = duration;
            projectileAnimator.SetTrigger(StartProjectile);
            spriteRenderer.flipX = dir.x < 0;
            DestroyProjectile(_duration);
        }

        private void Awake() {
            if (projectileAnimator == null) {
                projectileAnimator = GetComponent<Animator>();
            }

            if (spriteRenderer == null) {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (projectileAnimator == null || spriteRenderer == null) {
                Debug.LogError("Projectile Animator or SpriteRenderer is not assigned");
                enabled = false;
            }
        }

        private void Update() {
            transform.Translate(_direction * (_speed * Time.deltaTime));
        }

        private void DestroyProjectile(float waitTime = 0f) {
            StartCoroutine(DestroyProjectileCoroutine(waitTime));
        }

        private IEnumerator DestroyProjectileCoroutine(float waitTime) {
            yield return new WaitForSeconds(waitTime);
            Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D collision) {
            // Check if the projectile collides with the ground
            if (GetBitMask(collision.gameObject.layer) == Ground.value || collision.CompareTag(ProjectileTag)) {
                // If the player is holding the projectile, disable the sprite renderer since it needs to keep moving
                // in order to hit the player but it can be invisible to simulate the that projectile has been destroyed
                if (CharacterManager.Instance.currentPlayerController.movementState == PlayerMovementState.Hold) {
                    spriteRenderer.enabled = false;
                    DestroyProjectile(0.1f);
                } else {
                    DestroyProjectile();
                    return;
                }
            }

            // Check if the projectile collides with the player
            PlayerController playerController = collision.GetComponent<PlayerController>();
            if (playerController != null && collision.CompareTag(Player)) {
                playerController.TakeDamage(_damage);
                DestroyProjectile();
            }
        }
    }
}