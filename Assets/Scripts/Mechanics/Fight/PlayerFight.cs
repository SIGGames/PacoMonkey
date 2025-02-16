using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Controllers;
using Enums;
using Gameplay;
using UnityEngine;
using static PlayerInput.KeyBinds;
using static Utils.AnimatorUtils;

namespace Mechanics.Fight {
    [RequireComponent(typeof(PlayerController))]
    [SuppressMessage("ReSharper", "Unity.PreferNonAllocApi")]
    public class PlayerFight : MonoBehaviour {
        [SerializeField] private FightState fightState = FightState.Idle;
        [SerializeField] private PlayerController playerController;
        [SerializeField] private LayerMask enemyLayer;

        [Header("Melee Attack Settings")]
        [SerializeField] private bool isMeleeActive = true;

        [SerializeField] private bool alwaysDrawMeleeBox;
        [SerializeField] private Vector2 meleeBoxSize = new(1f, 1f);
        [SerializeField, Range(-1, 1)] private float meleeVerticalOffset = 0.1f;
        [SerializeField, Range(0, 500)] private int meleeDamage = 200;
        [SerializeField, Range(0, 5)] private float cooldownTime = 0.5f;
        [SerializeField] private Vector2 meleeBounceForce = new(5f, 0f);
        private LedgeDetection _ledgeCheck;

        [Header("Ranged Attack Settings")]
        [SerializeField] private bool isRangedActive = true;

        [SerializeField] private GameObject rangedProjectilePrefab;
        [SerializeField] private Vector2 rangedOffset = new(0.1f, 0.1f);
        [SerializeField, Range(0, 10)] private float rangedProjectileSpeed = 5f;
        [SerializeField, Range(0, 10)] private float rangedProjectileDuration = 2f;
        [SerializeField, Range(0, 500)] private int rangedDamage = 200;
        [SerializeField, Range(0, 5)] private float rangedCooldownTime = 0.5f;

        [Header("Parry Settings")]
        [SerializeField] private bool isParryActive = true;

        [SerializeField, Range(0, 5)] private float parryCooldownTime = 1f;


        private Animator _animator;
        private bool _canAttack = true;
        private bool _canParry = true;

        private void Awake() {
            if (playerController == null) {
                playerController = GetComponent<PlayerController>();
            }

            if (playerController == null) {
                Debug.LogError("PlayerFight script requires a PlayerController component");
                enabled = false;
                return;
            }

            _ledgeCheck = GetComponentInChildren<LedgeDetection>();

            _animator = playerController.animator;

            if (_animator == null) {
                _animator = playerController.GetComponent<Animator>();
            }

            fightState = FightState.Idle;
        }

        private void Update() {
            if (isMeleeActive && GetMeleeKey() && _canAttack) {
                StartMeleeAttackAnimation();
            }

            if (isRangedActive && GetRangeKey() && _canAttack) {
                StartRangedAttackAnimation();
            }

            if (isParryActive && GetParryKey() && _canParry) {
                StartParry();
            }
        }

        private void StartMeleeAttackAnimation() {
            if (playerController.IsGrounded) {
                playerController.FreezeHorizontalPosition();
            }

            fightState = FightState.Melee;
            _animator.SetTrigger(MeleeAttack);
        }

        private void StartRangedAttackAnimation() {
            if (playerController.IsGrounded) {
                playerController.FreezeHorizontalPosition();
            }

            fightState = FightState.Ranged;
            _animator.SetTrigger(RangedAttack);
        }

        private void StartMeleeAttack() {
            // Triggered on frame 3 of animator
            Vector3 attackPosition = (Vector2)transform.position + GetDirectionOffset();
            Collider2D[] enemiesInRange = Physics2D.OverlapBoxAll(attackPosition, meleeBoxSize, 0f, enemyLayer);

            if (enemiesInRange.Length > 0) {
                foreach (Collider2D enemy in enemiesInRange) {
                    EnemyController enemyController = enemy.GetComponent<EnemyController>();
                    if (enemyController != null) {
                        enemyController.TakeDamage(meleeDamage);
                    }
                }
            }

            StartCoroutine(MeleeAttackCooldown());
        }

        // This method is called by the animator
        private void StartRangedAttack() {
            Vector2 playerPosition = transform.position;
            Vector2 spawnPos = new Vector2(playerPosition.x + GetRangedOffset(), playerPosition.y + rangedOffset.y);
            GameObject projectile = Instantiate(rangedProjectilePrefab, spawnPos, Quaternion.identity);
            Projectile projectileScript = projectile.GetComponent<Projectile>();
            if (projectileScript != null) {
                projectileScript.Initialize(playerController.isFacingRight ? Vector2.right : Vector2.left,
                    rangedProjectileSpeed, rangedDamage, rangedProjectileDuration);
            }

            StartCoroutine(RangedAttackCooldown());
        }

        private void CheckWallOnMelee() {
            if (_ledgeCheck != null && _ledgeCheck.isNearWall) {
                FinishAttack();
                playerController.Bounce(meleeBounceForce);
            }
        }

        private void StartParry() {
            fightState = FightState.Parry;
            _animator.SetTrigger(Parry);
            StartCoroutine(ParryCooldown());
        }

        private void FinishAttack() {
            fightState = FightState.Idle;
            playerController.FreezeHorizontalPosition(false);
        }

        private IEnumerator MeleeAttackCooldown() {
            _canAttack = false;
            yield return new WaitForSeconds(cooldownTime);
            _canAttack = true;
        }

        private IEnumerator RangedAttackCooldown() {
            _canAttack = false;
            yield return new WaitForSeconds(rangedCooldownTime);
            _canAttack = true;
        }

        private IEnumerator ParryCooldown() {
            _canParry = false;
            yield return new WaitForSeconds(parryCooldownTime);
            _canParry = true;
        }

        private void OnDrawGizmosSelected() {
            if (fightState == FightState.Melee || alwaysDrawMeleeBox) {
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(GetGizmoPosition(meleeBoxSize, meleeVerticalOffset), meleeBoxSize);
            }
        }

        private Vector3 GetGizmoPosition(Vector2 boxSize, float verticalOffset) {
            return (Vector2)transform.position + CalculateOffset(boxSize, verticalOffset);
        }

        private Vector2 CalculateOffset(Vector2 boxSize, float verticalOffset) {
            return playerController.isFacingRight
                ? new Vector2(boxSize.x / 2, verticalOffset)
                : new Vector2(-boxSize.x / 2, verticalOffset);
        }

        private float GetRangedOffset() {
            return playerController.isFacingRight ? rangedOffset.x : -rangedOffset.x;
        }

        private Vector2 GetDirectionOffset() {
            return fightState switch {
                FightState.Melee => CalculateOffset(meleeBoxSize, meleeVerticalOffset),
                _ => CalculateOffset(meleeBoxSize, 0)
            };
        }
    }
}