using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Controllers;
using Enums;
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

        [SerializeField] private Vector2 meleeBoxSize = new(1f, 1f);
        [SerializeField] private Vector2 meleeOffset = new(0.5f, 0.5f);
        [SerializeField] [Range(0, 500)] private int meleeDamage = 200;
        [SerializeField] [Range(0, 5)] private float cooldownTime = 0.5f;

        [Header("Range Attack Settings")]
        [SerializeField] private bool isRangedActive = true;

        [SerializeField] private Vector2 rangedBoxSize = new(1f, 1f);
        [SerializeField] private Vector2 rangedOffset = new(0.5f, 0.5f);
        [SerializeField] [Range(0, 500)] private int rangedDamage = 200;

        private Animator _animator;
        private bool _canAttack = true;

        private void Awake() {
            if (playerController == null) {
                playerController = GetComponent<PlayerController>();
            }

            if (playerController == null) {
                Debug.LogError("PlayerFight script requires a PlayerController component");
                enabled = false;
                return;
            }

            _animator = playerController.animator;
            fightState = FightState.Idle;
        }

        private void Update() {
            if (isMeleeActive && GetMeleeKey() && _canAttack) {
                StartAttackAnimation();
            }
        }

        private void StartAttackAnimation() {
            playerController.FreezeHorizontalPosition();
            fightState = FightState.Melee;
            _animator.SetTrigger(MeleeAttack);
        }

        private void StartAttack() {
            // Triggered on frame 3 of animator
            Vector3 attackPosition = (Vector2)transform.position + GetDirectionOffset();
            Collider2D[] enemiesInRange = Physics2D.OverlapBoxAll(attackPosition, meleeBoxSize, 0f, enemyLayer);

            if (enemiesInRange.Length > 0) {
                foreach (Collider2D enemy in enemiesInRange) {
                    // TODO: Do a map of enemies to avoid GetComponent in every iteration
                    Enemy enemyController = enemy.GetComponent<Enemy>();
                    if (enemyController != null) {
                        enemyController.TakeDamage(meleeDamage);
                    }

                    EnemyController enemyController2 = enemy.GetComponent<EnemyController>();
                    if (enemyController2 != null) {
                        enemyController2.TakeDamage(meleeDamage);
                    }
                }
            }

            StartCoroutine(MeleeAttackCooldown());
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

        private void OnDrawGizmosSelected() {
            if (fightState == FightState.Melee) {
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(GizmoPosition(), meleeBoxSize);
            }

            if (fightState == FightState.Ranged) {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireCube(GizmoPosition(), rangedBoxSize);
            }
        }

        private Vector3 GizmoPosition() => (Vector2)transform.position + GetDirectionOffset();

        private Vector2 GetDirectionOffset() {
            return fightState switch {
                FightState.Melee => playerController.isFacingRight
                    ? new Vector2(meleeBoxSize.x / 2, meleeOffset.y)
                    : new Vector2(-meleeBoxSize.x / 2, meleeOffset.y),
                FightState.Ranged => playerController.isFacingRight
                    ? new Vector2(rangedBoxSize.x / 2, rangedOffset.y)
                    : new Vector2(-rangedBoxSize.x / 2, rangedOffset.y),
                _ => Vector2.zero
            };
        }
    }
}