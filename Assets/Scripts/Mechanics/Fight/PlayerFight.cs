using System.Collections;
using Controllers;
using UnityEngine;
using static PlayerInput.KeyBinds;
using static Utils.AnimatorUtils;

namespace Mechanics.Fight {
    [RequireComponent(typeof(PlayerController))]
    public class PlayerFight : MonoBehaviour {
        [SerializeField] private PlayerController playerController;
        [SerializeField] private LayerMask enemyLayer;
        [SerializeField] private float meleeRange = 1f;
        [SerializeField] private int meleeDamage = 200;
        [SerializeField] private float cooldownTime = 0.5f;
        private Animator _animator;
        private bool _canAttack = true;

        private void Awake() {
            if (playerController == null) {
                playerController = GetComponent<PlayerController>();
            }

            _animator = playerController.animator;
        }

        private void Update() {
            if (GetMeleeKey() && _canAttack) {
                TryMeleeAttack();
            }
        }

        private void TryMeleeAttack() {
            Debug.Log("Attacking");
            Collider[] enemiesInRange = Physics.OverlapSphere(transform.position, meleeRange, enemyLayer);

            if (enemiesInRange.Length > 0) {
                _animator.SetTrigger(MeleeAttack);
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
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, meleeRange);
        }
    }
}
