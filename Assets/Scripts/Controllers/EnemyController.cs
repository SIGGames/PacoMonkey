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

        [ShowIf("enemyType", EnemyType.Ranged)]
        [SerializeField] private Vector2 projectileOffset = new(0.1f, 0.1f);

        [ShowIf("enemyType", EnemyType.Melee)]
        [SerializeField, Range(0, 10)] private float bounceForce = 4f;

        [ShowIf("enemyType", EnemyType.Melee)]
        [SerializeField, Range(0, 3)] private float distanceAfterAttack = 1f;

        [Header("Movement Settings")]
        [SerializeField] private bool isFacingRight = true;

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
        [SerializeField] private GameObject enemyPrefabAsset;

        [SerializeField] private bool drawRangesInEditor = true;

        [ShowIf("enemyType", EnemyType.Ranged)]
        [SerializeField] private bool drawProjectileInEditor = true;

        [SerializeField] private bool applyGravityOnEnemy = true;

        public Bounds Bounds => col.bounds;
        private bool HasHealthBar => enemyHealthBar != null;
        private Vector2 _velocity = Vector2.zero;
        private float _attackCooldownTimer;
        private bool IsEnemyGrounded => animator.GetBool(Grounded);


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

            if (!EnemySpawnManager.EnemySpawnList.Exists(data => data.spawnPosition == transform.position
                                                                 && data.enemyType == enemyType)) {
                EnemySpawnData data = new EnemySpawnData {
                    enemyPrefab = enemyPrefabAsset,
                    spawnPosition = transform.position,
                    spawnRotation = transform.rotation,
                    enemyType = enemyType
                };
                EnemySpawnManager.EnemySpawnList.Add(data);
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
        }

        private void Update() {
            _currentPlayer = CharacterManager.currentPlayerController;

            if (!IsEnemyAbleToPlay()) {
                return;
            }

            UpdateVelocity();

            if (_attackCooldownTimer > 0f) {
                _attackCooldownTimer -= Time.deltaTime;
            }

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

            if (PlayerInAttackRange && _attackCooldownTimer <= 0f && IsEnemyGrounded) {
                AttackPlayer();
            }

            if (!_currentPlayer.lives.IsAlive) {
                navAgent.ResetPath();
            }

            EnsureEnemyGroundedMovement();
            HandleLives();
            HandleFlip();
            IsGrounded();
        }

        private bool IsEnemyAbleToPlay() {
            return _currentPlayer.lives.IsAlive;
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
            if (PlayerInSightRange || PlayerInAttackRange) {
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

        private void EnsureEnemyGroundedMovement() {
            if (!IsEnemyGrounded && applyGravityOnEnemy) {
                RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 100f, groundLayer);
                if (hit.collider != null) {
                    float groundY = hit.point.y + groundOffset + 0.5f;
                    Vector2 newDestination = new Vector2(transform.position.x, groundY);
                    navAgent.Warp(newDestination);
                }
            }
        }

        private void ChasePlayer() {
            if (ThereIsAWallBetweenEnemyAndPlayer()) {
                return;
            }

            Vector2 playerPos = _currentPlayer.transform.position;

            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 100f, groundLayer);
            float groundY = hit.collider != null ? hit.point.y + groundOffset : transform.position.y;
            float targetX = playerPos.x;

            if (!IsEnemyGrounded) {
                targetX = transform.position.x;
            }

            Vector2 newDestination = new Vector2(targetX, groundY);
            navAgent.SetDestination(newDestination);
        }

        private bool ThereIsAWallBetweenEnemyAndPlayer() {
            Vector2 playerPos = _currentPlayer.transform.position;
            Vector2 enemyPos = transform.position;
            RaycastHit2D hit = Physics2D.Raycast(enemyPos, playerPos - enemyPos, Vector2.Distance(playerPos, enemyPos),
                groundLayer);
            return hit.collider != null;
        }

        private void IsGrounded() {
            const float rayLength = 0.1f;
            RaycastHit2D hit = Physics2D.Raycast(col.bounds.center, Vector2.down, col.bounds.extents.y + rayLength,
                groundLayer);
            bool isGrounded = hit.collider != null;
            animator.SetBool(Grounded, isGrounded);
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

            if (!CanAttackWallCheck()) {
                return;
            }

            animator.SetTrigger(Attack);
            navAgent.ResetPath();
            _attackCooldownTimer = cooldownTime;
        }

        private bool CanAttackWallCheck() {
            Vector3 enemyPosition = transform.position;
            const float extraOffset = 0.5f;

            Vector2 direction = isFacingRight ? Vector2.right : Vector2.left;
            RaycastHit2D hit = Physics2D.Raycast(enemyPosition, direction,
                distanceAfterAttack + extraOffset, groundLayer);
            Debug.DrawRay(enemyPosition, direction * (distanceAfterAttack + extraOffset), Color.red);
            return hit.collider == null;
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
            } else {
                animator.SetTrigger(Hurt);
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
            navAgent.velocity = Vector3.zero;
        }

        private void BouncePlayer(bool bounceOnAllDirections = false, float bounceForceDecrease = 1f) {
            float decreasedBounceForce = bounceForce * bounceForceDecrease;
            const float verticalPercentage = 0.7f;
            if (bounceOnAllDirections) {
                _currentPlayer.BounceX(isFacingRight ? decreasedBounceForce : -decreasedBounceForce);
                _currentPlayer.BounceY(decreasedBounceForce * verticalPercentage);
            } else {
                _currentPlayer.BounceX(isFacingRight ? decreasedBounceForce : -decreasedBounceForce);

                if (!_currentPlayer.IsGrounded) {
                    _currentPlayer.BounceY(decreasedBounceForce * verticalPercentage);
                }
            }
        }

        private void BouncePlayerOnAnimation() {
            if (PlayerInSightRange) {
                BouncePlayer();
                _currentPlayer.lives.DecrementLives(attackDamage);
            }
        }

        private void RangedAttack() {
            Vector2 enemyPos = transform.position;
            Vector2 spawnPos = new Vector2(enemyPos.x + GetProjectileOffset(), enemyPos.y + projectileOffset.y);
            Vector2 playerPos = _currentPlayer.transform.position;
            Vector2 direction = (playerPos - enemyPos).normalized;

            // Check if there are obstacles between the enemy and the player
            RaycastHit2D hit = Physics2D.Raycast(spawnPos, direction, Vector2.Distance(enemyPos, playerPos),
                groundLayer);
            if (hit.collider != null && hit.collider.gameObject != _currentPlayer.gameObject) {
                return;
            }

            GameObject projectile = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
            EnemyProjectile projectileScript = projectile.GetComponent<EnemyProjectile>();
            if (projectileScript != null) {
                projectileScript.Initialize(direction, projectileSpeed, attackDamage, projectileDuration);
            }
        }

        private float GetProjectileOffset() {
            return isFacingRight ? projectileOffset.x : -projectileOffset.x;
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

            if (enemyType == EnemyType.Ranged && drawProjectileInEditor) {
                Vector2 enemyPos = transform.position;
                Vector2 spawnPos = new Vector2(enemyPos.x + GetProjectileOffset(), enemyPos.y + projectileOffset.y);
                Gizmos.color = new Color(0.6f, 0.3f, 0.0f, 1f);
                Gizmos.DrawSphere(spawnPos, 0.05f);
            }
        }
    }
}