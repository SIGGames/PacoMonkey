using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace Controllers {
    public class Enemy : MonoBehaviour {
        public NavMeshAgent navAgent;
        public Transform player;
        public LayerMask groundLayer, playerLayer;
        public float health;
        public float walkPointRange;
        public float timeBetweenAttacks;
        public float sightRange;
        public float attackRange;
        public int damage;
        public Animator animator;
        public ParticleSystem hitEffect;

        private Vector3 _walkPoint;
        private bool _walkPointSet;
        private bool _alreadyAttacked;
        private bool _takeDamage;

        private void Awake() {
            if (player == null || navAgent == null || animator == null) {
                Debug.LogError("Enemy script requires a Player, NavMeshAgent, and Animator component");
                enabled = false;
            }
        }

        private void Update() {
            Vector3 position = transform.position;
            float distanceToPlayer = Vector3.Distance(position, player.position);

            bool playerInSightRange = Physics.CheckSphere(position, sightRange, playerLayer);
            bool playerInAttackRange = Physics.CheckSphere(position, attackRange, playerLayer);

            if (distanceToPlayer <= sightRange) {
                playerInSightRange = true;
            }
            if (distanceToPlayer <= attackRange) {
                playerInAttackRange = true;
            }

            if (playerInSightRange) {
                ChasePlayer();
            }

            /*if (!playerInSightRange && !playerInAttackRange) {
                Patroling();
            } else if (playerInSightRange && !playerInAttackRange) {
                ChasePlayer();
            } else if (playerInAttackRange && playerInSightRange) {
                AttackPlayer();
            } else if (!playerInSightRange && _takeDamage) {
                ChasePlayer();
            }*/
        }

        private void Patroling() {
            if (!_walkPointSet) {
                SearchWalkPoint();
            }

            if (_walkPointSet) {
                navAgent.SetDestination(_walkPoint);
            }

            Vector3 distanceToWalkPoint = transform.position - _walkPoint;
            animator.SetFloat("Velocity", 0.2f);

            if (distanceToWalkPoint.magnitude < 1f) {
                _walkPointSet = false;
            }
        }

        private void SearchWalkPoint() {
            float randomZ = Random.Range(-walkPointRange, walkPointRange);
            float randomX = Random.Range(-walkPointRange, walkPointRange);
            _walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

            if (Physics.Raycast(_walkPoint, -transform.up, 2f, groundLayer)) {
                _walkPointSet = true;
            }
        }

        private void ChasePlayer() {
            navAgent.SetDestination(player.position);
            // navAgent.SetDestination(player.position);
            animator.SetFloat("Velocity", 0.6f);
            navAgent.isStopped = false; // Add this line
        }


        private void AttackPlayer() {
            navAgent.SetDestination(transform.position);

            if (!_alreadyAttacked) {
                transform.LookAt(player.position);
                _alreadyAttacked = true;
                animator.SetBool("Attack", true);
                Invoke(nameof(ResetAttack), timeBetweenAttacks);

                RaycastHit hit;
                if (Physics.Raycast(transform.position, transform.forward, out hit, attackRange)) {
                 /*
                 YOU CAN USE THIS TO GET THE PLAYER HUD AND CALL THE TAKE DAMAGE FUNCTION

                PlayerHUD playerHUD = hit.transform.GetComponent<PlayerHUD>();
                if (playerHUD != null) {
                    playerHUD.takeDamage(damage);
                }
                */
                }
            }
        }


        private void ResetAttack() {
            _alreadyAttacked = false;
            animator.SetBool("Attack", false);
        }

        public void TakeDamage(float damage) {
            health -= damage;
            hitEffect.Play();
            StartCoroutine(TakeDamageCoroutine());

            if (health <= 0) {
                Invoke(nameof(DestroyEnemy), 0.5f);
            }
        }

        private IEnumerator TakeDamageCoroutine() {
            _takeDamage = true;
            yield return new WaitForSeconds(2f);
            _takeDamage = false;
        }

        private void DestroyEnemy() {
            StartCoroutine(DestroyEnemyCoroutine());
        }

        private IEnumerator DestroyEnemyCoroutine() {
            animator.SetBool("Dead", true);
            yield return new WaitForSeconds(1.8f);
            Destroy(gameObject);
        }

        private void OnDrawGizmosSelected() {
            Gizmos.color = Color.red;
            Vector3 position = transform.position;
            Gizmos.DrawWireSphere(position, attackRange);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(position, sightRange);
        }
    }
}