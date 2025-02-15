using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Enums;
using Gameplay;
using Health.UI;
using Managers;
using Mechanics;
using Mechanics.Fight;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using static Platformer.Core.Simulation;
using static Utils.AnimatorUtils;

namespace Controllers {
    [RequireComponent(typeof(Collider2D)), RequireComponent(typeof(AudioSource)),
     RequireComponent(typeof(SpriteRenderer)), RequireComponent(typeof(Health.Health))]
    [SuppressMessage("ReSharper", "Unity.InefficientPropertyAccess")]
    public class EnemyController : MonoBehaviour {
        public AudioClip ouch;
        internal PatrolPath.Mover mover;
        internal Collider2D col;
        internal AudioSource audioSource;
        private SpriteRenderer _spriteRenderer;
        private Health.Health _health;
        private static CharacterManager CharacterManager => CharacterManager.Instance;
        private PlayerController _currentPlayer;

        [SerializeField] private EnemyType enemyType = EnemyType.Melee;

        [Header("AI Settings")]
        public NavMeshAgent navAgent;

        public LayerMask groundLayer;

        [Header("Sight Settings")]
        [ShowIf("enemyType", EnemyType.Melee)]
        [SerializeField] private Vector2 sightBoxSize = new(2f, 1f);

        [ShowIf("enemyType", EnemyType.Melee)]
        [SerializeField, MaxValue(5)] private Vector2 sightBoxOffset;

        [SerializeField, Range(0, 5)] public float attackRange = 1f;
        [SerializeField, Range(0, 2)] private float groundOffset = 0.5f;

        [Header("Attack Settings")]
        [SerializeField, HalfStepSlider(0, 10)]
        private float attackDamage = 1f;

        [SerializeField, Range(0, 5)] private float cooldownTime = 1.5f;

        [ShowIf("enemyType", EnemyType.Ranged)]
        [SerializeField] private GameObject projectilePrefab;

        [ShowIf("enemyType", EnemyType.Ranged)]
        [SerializeField] private float projectileSpeed = 5f;

        [ShowIf("enemyType", EnemyType.Ranged)]
        [SerializeField] private float projectileDuration = 2f;

        [ShowIf("enemyType", EnemyType.Melee)]
        [SerializeField, Range(0, 10)] private float bounceForce = 4f;

        [ShowIf("enemyType", EnemyType.Melee)]
        [SerializeField, Range(0, 3)] private float distanceAfterAttack = 1f;

        [Header("Movement Settings")]
        [SerializeField] private bool isFacingRight = true;

        private bool _attacking;

        private Vector3 _walkPoint;
        private float DistanceToPlayer => Vector3.Distance(transform.position, _currentPlayer.transform.position);

        private bool PlayerInSightRange {
            get {
                Vector2 offset = sightBoxOffset;
                if (!isFacingRight) {
                    offset.x = -offset.x;
                }

                Vector2 boxCenter = (Vector2)transform.position + offset;
                Rect sightRect = new Rect(
                    boxCenter.x - sightBoxSize.x / 2,
                    boxCenter.y - sightBoxSize.y / 2,
                    sightBoxSize.x,
                    sightBoxSize.y
                );
                return sightRect.Contains(_currentPlayer.transform.position);
            }
        }

        private bool PlayerInAttackRange {
            get {
                float distanceToPlayer = DistanceToPlayer;
                return distanceToPlayer <= attackRange;
            }
        }

        [Header("Enemy Settings")]
        [SerializeField] private float deathTime = 0.3f;

        [Header("Enemy Controller Components")]
        [SerializeField] private Animator animator;

        [SerializeField] FloatingHealthBar enemyHealthBar;
        [SerializeField] private GameObject bloodPrefab;

        [Header("Debug")]
        [SerializeField] private bool drawRangesInEditor = true;

        public Bounds Bounds => col.bounds;
        private bool HasHealthBar => enemyHealthBar != null;
        private Vector2 _velocity = Vector2.zero;
        private float _attackCooldownTimer;
        private float prevgroundY;

        private void Awake() {
            col = GetComponent<Collider2D>();
            audioSource = GetComponent<AudioSource>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            if (enemyHealthBar == null) {
                enemyHealthBar = GetComponentInChildren<FloatingHealthBar>();
            }

            if (_health == null) {
                _health = GetComponent<Health.Health>();
            }

            if (animator == null) {
                animator = GetComponent<Animator>();
            }

            if (navAgent == null) {
                navAgent = GetComponent<NavMeshAgent>();
            }

            // Used to prevent the enemy from rotating (since it's a 2D game)
            navAgent.updateRotation = false;
            navAgent.updateUpAxis = false;
            navAgent.updatePosition = false;
            if (navAgent == null || col == null || audioSource == null || _spriteRenderer == null ||
                _health == null || animator == null) {
                enabled = false;
            }
        }

        private void Start() {
            _currentPlayer = CharacterManager.currentPlayerController;

            if (HasHealthBar) {
                enemyHealthBar.UpdateHealthBar(_health.CurrentHealth, _health.maxHealth);
            }
        }

        private void OnCollisionEnter2D(Collision2D other) {
            if (other.gameObject.GetComponent<PlayerController>() == null) {
                return;
            }

            AttackPlayer();
            BouncePlayer();
        }

        private void Update() {
            _currentPlayer = CharacterManager.currentPlayerController;

            UpdateVelocity();

            if (_attackCooldownTimer > 0f) {
                _attackCooldownTimer -= Time.deltaTime;
            }

            CheckIfIsAscending();

            if (enemyType == EnemyType.Melee) {
                if (PlayerInSightRange) {
                    ChasePlayer();
                }
            }

            if (enemyType == EnemyType.Ranged) {
                if (PlayerInAttackRange) {
                    ChasePlayer();
                }
            }

            if (PlayerInAttackRange && _attackCooldownTimer <= 0f) {
                AttackPlayer();
            }

            if (!_currentPlayer.lives.IsAlive) {
                navAgent.ResetPath();
            }

            HandleLives();
            HandleFlip();
            IsGrounded();
        }

        private void UpdateVelocity() {
            _velocity = navAgent.velocity;
            animator.SetFloat(VelocityX, Mathf.Abs(_velocity.x));
            animator.SetFloat(VelocityY, Mathf.Abs(_velocity.y));
        }

        private void HandleFlip() {
            Vector3 agentPos = navAgent.nextPosition;
            agentPos.y += _velocity.y * Time.deltaTime;
            transform.position = agentPos;
            navAgent.nextPosition = transform.position;
            if (PlayerInSightRange) {
                if (_currentPlayer.transform.position.x > transform.position.x && !isFacingRight) {
                    Flip();
                } else if (_currentPlayer.transform.position.x < transform.position.x && isFacingRight) {
                    Flip();
                }
            } else {
                if (_velocity.x > 0 && !isFacingRight) {
                    Flip();
                } else if (_velocity.x < 0 && isFacingRight) {
                    Flip();
                }
            }
        }

        private void CheckIfIsAscending() {
            if (_velocity.y > 0) {
                navAgent.ResetPath();
                navAgent.isStopped = true;
            } else {
                navAgent.isStopped = false;
            }
        }

        private void ChasePlayer() {
            Vector2 playerPos = _currentPlayer.transform.position;
            float targetX = playerPos.x;
            float targetY;
            if (!IsGrounded()) {
                RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 100f, groundLayer);
                targetY = hit.collider != null ? hit.point.y + groundOffset : transform.position.y;
            } else {
                RaycastHit2D hit = Physics2D.Raycast(playerPos, Vector2.down, 100f, groundLayer);
                targetY = hit.collider != null ? hit.point.y + groundOffset : transform.position.y;
                targetY = Mathf.Min(targetY, transform.position.y);
            }

            Vector2 newDestination = new Vector2(targetX, targetY);
            if (Physics2D.OverlapPoint(newDestination, groundLayer) == null) {
                navAgent.SetDestination(newDestination);
            }
        }


        private bool IsGrounded() {
            const float rayLength = 0.1f;
            RaycastHit2D hit = Physics2D.Raycast(col.bounds.center, Vector2.down, col.bounds.extents.y + rayLength,
                groundLayer);
            bool isGrounded = hit.collider != null || _attacking;
            animator.SetBool(Grounded, isGrounded);
            return isGrounded;
        }

        private void HandleLives() {
            _spriteRenderer.color = _health.IsAlive ? Color.white : Color.black;
        }

        private void Flip() {
            isFacingRight = !isFacingRight;
            _spriteRenderer.flipX = !isFacingRight;
        }

        private void AttackPlayer() {
            if (_attackCooldownTimer > 0f) {
                return;
            }

            _attacking = true;
            animator.SetTrigger(Attack);
            navAgent.ResetPath();
            _attackCooldownTimer = cooldownTime;

            if (enemyType == EnemyType.Ranged) {
                RangedAttack();
            }
        }

        public void TakeDamage(float damage) {
            _health.DecrementHealth(damage);
            if (HasHealthBar) {
                enemyHealthBar.UpdateHealthBar(_health.CurrentHealth, _health.maxHealth);
                enemyHealthBar.ShowFloatingHealthBar();
            }

            if (!_health.IsAlive) {
                Schedule<EnemyDeath>().enemy = this;
                animator.SetTrigger(Death);
                _velocity = Vector2.zero;
                navAgent.ResetPath();
                DestroyEnemy();
            }
        }

        private void DestroyEnemy() {
            StartCoroutine(DestroyEnemyCoroutine());
        }

        private IEnumerator DestroyEnemyCoroutine() {
            if (bloodPrefab != null) {
                Instantiate(bloodPrefab, transform.position, Quaternion.identity);
            }

            yield return new WaitForSeconds(deathTime);
            Destroy(gameObject);
        }

        public void OnFinishEnemyAttackAnimation() {
            Vector3 enemyPosition = transform.position;
            float offsetOnFinishAttack = isFacingRight ? distanceAfterAttack : -distanceAfterAttack;
            Vector3 newPosition = new Vector3(enemyPosition.x + offsetOnFinishAttack, enemyPosition.y, enemyPosition.z);
            transform.position = newPosition;

            // Setting the new position to the navAgent
            Vector3 newPositionOffset = Vector3.zero;
            newPositionOffset.x = offsetOnFinishAttack;
            navAgent.Move(newPositionOffset);
            _attacking = false;
        }

        private void BouncePlayer(bool bounceOnAllDirections = false) {
            const float verticalPercentage = 0.7f;
            if (bounceOnAllDirections) {
                _currentPlayer.BounceX(isFacingRight ? bounceForce : -bounceForce);
                _currentPlayer.BounceY(bounceForce * verticalPercentage);
            } else {
                _currentPlayer.BounceX(isFacingRight ? bounceForce : -bounceForce);

                if (!_currentPlayer.IsGrounded) {
                    _currentPlayer.BounceY(bounceForce * verticalPercentage);
                }
            }
        }

        private void BouncePlayerOnAnimation() {
            if (PlayerInAttackRange) {
                BouncePlayer();
                _currentPlayer.lives.DecrementLives(attackDamage);
            }
        }

        private void RangedAttack() {
            Vector2 playerPosition = _currentPlayer.transform.position;
            Vector2 spawnPos = new Vector2(transform.position.x, transform.position.y);
            GameObject projectile = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
            EnemyProjectile projectileScript = projectile.GetComponent<EnemyProjectile>();
            if (projectileScript != null) {
                projectileScript.Initialize(isFacingRight ? Vector2.right : Vector2.left, projectileSpeed,
                    attackDamage, projectileDuration);
            }
        }

        private void OnDrawGizmosSelected() {
            if (drawRangesInEditor) {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, attackRange);
                if (enemyType == EnemyType.Melee) {
                    Gizmos.color = Color.yellow;
                    Vector2 offset = sightBoxOffset;
                    if (!isFacingRight) {
                        offset.x = -offset.x;
                    }

                    Vector2 boxCenter = (Vector2)transform.position + offset;
                    Gizmos.DrawWireCube(boxCenter, sightBoxSize);
                }
            }
        }
    }
}