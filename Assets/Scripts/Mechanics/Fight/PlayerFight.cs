using System.Collections;
using Controllers;
using Enums;
using UnityEngine;
using static PlayerInput.KeyBinds;
using static Utils.AnimatorUtils;

namespace Mechanics.Fight {
    [RequireComponent(typeof(PlayerController))]
    public class PlayerFight : MonoBehaviour {
        [SerializeField] private FightState fightState = FightState.Idle;
        [SerializeField] private PlayerController playerController;
        [SerializeField] private LayerMask enemyLayer;

        [Header("Melee Attack Settings")]
        [SerializeField] private bool isMeleeActive = true;

        [SerializeField] private float meleeRange = 1f;
        [SerializeField] private int meleeDamage = 200;
        [SerializeField] private float cooldownTime = 0.5f;

        [Header("Range Attack Settings")]
        [SerializeField] private bool isRangedActive = true;

        [SerializeField] private float rangedRange = 10f;
        [SerializeField] private int rangedDamage = 200;

        private Animator _animator;
        private bool _canAttack = true;

        private void Awake() {
            if (playerController == null) {
                playerController = GetComponent<PlayerController>();
            }

            _animator = playerController.animator;
        }

        private void Update() {
            if (isMeleeActive && GetMeleeKey() && _canAttack) {
                TryMeleeAttack();
            }
        }

        private void TryMeleeAttack() {
            fightState = FightState.Melee;
            Collider[] enemiesInRange = Physics.OverlapSphere(transform.position, meleeRange, enemyLayer);
           _animator.SetTrigger(MeleeAttack);

            if (enemiesInRange.Length > 0) {
                foreach (Collider enemy in enemiesInRange) {
                    // TODO: Do a map of enemies to avoid GetComponent in every iteration
                    Enemy enemyController = enemy.GetComponent<Enemy>();
                    if (enemyController != null) {
                        enemyController.TakeDamage(meleeDamage);
                    }
                }

                StartCoroutine(MeleeAttackCooldown());
            }
        }

        private IEnumerator MeleeAttackCooldown() {
            _canAttack = false;
            yield return new WaitForSeconds(cooldownTime);
            _canAttack = true;
        }

        private void OnDrawGizmosSelected() {
            if (fightState == FightState.Melee) {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, meleeRange);
            }

            if (fightState == FightState.Ranged) {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(transform.position, rangedRange);
            }
        }

        public void SetIdleFightState() {
            // This method is called by the animator when the attack animation ends
            fightState = FightState.Idle;
        }
    }
}