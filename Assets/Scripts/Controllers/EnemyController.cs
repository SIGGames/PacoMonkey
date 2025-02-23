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
using static Platformer.Core.Simulation;
using static Utils.AnimatorUtils;
using static Utils.LayerUtils;

namespace Controllers {
    [RequireComponent(typeof(Collider2D)), RequireComponent(typeof(AudioSource)),
     RequireComponent(typeof(SpriteRenderer)), RequireComponent(typeof(Health.Health))]
    [SuppressMessage("ReSharper", "Unity.InefficientPropertyAccess")]
    public class EnemyController : MonoBehaviour {
        public AudioClip ouch;
        internal PatrolPath.Mover mover;
        private Collider2D _col;
        internal AudioSource audioSource;
        private SpriteRenderer _spriteRenderer;
        private Health.Health _health;
        private static PlayerController CurrentPlayer => CharacterManager.Instance.currentPlayerController;

        [SerializeField] private EnemyType enemyType = EnemyType.Melee;

        [Header("AI Settings")]
        [SerializeField, Range(0, 5)] private float moveSpeed = 2f;

        [Header("Sight Settings")]
        [ShowIf("enemyType", EnemyType.Melee)]
        [SerializeField] private Vector2 sightBoxSize = new(2f, 1f);

        [ShowIf("enemyType", EnemyType.Melee)]
        [SerializeField, MaxValue(5)] private Vector2 sightBoxOffset;

        [SerializeField, Range(0, 5)] public float attackRange = 1f;
        [SerializeField, Range(0, 2)] private float groundOffset = 0.5f; // TODO: Maybe remove

        [Header("Attack Settings")]
        [SerializeField, HalfStepSlider(0, 10)]
        private float attackDamage = 1f;

        [SerializeField, Range(0, 5)] private float cooldownTime = 1.5f;

        [ShowIf("enemyType", EnemyType.Ranged)]
        [SerializeField] private GameObject projectilePrefab;

        [ShowIf("enemyType", EnemyType.Ranged)]
        [SerializeField, Range(0, 10)] private float projectileSpeed = 5f;

        [ShowIf("enemyType", EnemyType.Ranged)]
        [SerializeField, Range(0, 5)] private float projectileDuration = 2f;

        [ShowIf("enemyType", EnemyType.Ranged)]
        [SerializeField] private Vector2 projectileOffset = new(0.1f, 0.1f);

        [ShowIf("enemyType", EnemyType.Melee)]
        [SerializeField, Range(0, 10)] private float bounceForce = 4f;

        [ShowIf("enemyType", EnemyType.Melee)]
        [SerializeField, Range(0, 3)] private float distanceAfterAttack = 1f;

        [Header("Movement Settings")]
        [SerializeField] private bool isFacingRight = true;

        private Vector3 _walkPoint;
        private float DistanceToPlayer => Vector3.Distance(transform.position, CurrentPlayer.transform.position);

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
                return sightRect.Contains(CurrentPlayer.transform.position);
            }
        }

        private bool PlayerInAttackRange => DistanceToPlayer <= attackRange;

        [Header("Enemy Settings")]
        [SerializeField, Range(0, 3)] private float deathTime = 0.3f;

        [Header("Enemy Controller Components")]
        [SerializeField] private Animator animator;

        [SerializeField] FloatingHealthBar enemyHealthBar;
        [SerializeField] private GameObject bloodPrefab;

        [Header("Debug")]
        [SerializeField] private GameObject enemyPrefabAsset;

        [SerializeField] private bool drawRangesInEditor = true;

        [ShowIf("enemyType", EnemyType.Ranged)]
        [SerializeField] private bool drawProjectileInEditor = true;

        public Bounds Bounds => _col.bounds;
        private bool HasHealthBar => enemyHealthBar != null;
        private float _attackCooldownTimer;

        private bool IsEnemyGrounded {
            get {
                const float rayLength = 0.1f;
                RaycastHit2D hit = Physics2D.Raycast(_col.bounds.center, Vector2.down,
                    _col.bounds.extents.y + rayLength, Ground);
                return hit.collider != null;
            }
        }

        private Vector3 _lastPosition;
        private Vector3 _velocity;

        private void Awake() {
            _col = GetComponent<Collider2D>();
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

            if (_col == null || audioSource == null || _spriteRenderer == null || _health == null || animator == null) {
                enabled = false;
            }

            if (!EnemySpawnManager.EnemySpawnList.Exists(data =>
                    data.spawnPosition == transform.position && data.enemyType == enemyType)) {
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
            _lastPosition = transform.position;
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
            if (!CurrentPlayer.lives.IsAlive) {
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

            HandleFlip();
            IsGrounded();
        }

        private void ChasePlayer() {
            if (ThereIsAWallBetweenEnemyAndPlayer()) {
                return;
            }

            Vector3 pos = transform.position;
            float step = moveSpeed * Time.deltaTime;
            float targetX = CurrentPlayer.transform.position.x;
            float newX = Mathf.MoveTowards(pos.x, targetX, step);
            transform.position = new Vector3(newX, pos.y, pos.z);
        }

        private bool ThereIsAWallBetweenEnemyAndPlayer() {
            Vector2 playerPos = CurrentPlayer.transform.position;
            Vector2 enemyPos = transform.position;
            RaycastHit2D hit = Physics2D.Raycast(enemyPos, playerPos - enemyPos, Vector2.Distance(playerPos, enemyPos),
                Ground);
            return hit.collider != null;
        }

        private void IsGrounded() {
            const float rayLength = 0.1f;
            RaycastHit2D hit = Physics2D.Raycast(_col.bounds.center, Vector2.down, _col.bounds.extents.y + rayLength,
                Ground);
            bool isGrounded = hit.collider != null;
            animator.SetBool(Grounded, isGrounded);
        }

        private void HandleFlip() {
            if (CurrentPlayer.transform.position.x > transform.position.x && !isFacingRight) {
                Flip();
            } else if (CurrentPlayer.transform.position.x < transform.position.x && isFacingRight) {
                Flip();
            }
        }

        private void UpdateVelocity() {
            _velocity = (transform.position - _lastPosition) / Time.deltaTime;
            _lastPosition = transform.position;
            if (Mathf.Approximately(_velocity.x, 0f)) {
                _velocity.x = 0f;
            }
            if (Mathf.Approximately(_velocity.y, 0f)) {
                _velocity.y = 0f;
            }
            animator.SetFloat(VelocityX, Mathf.Abs(_velocity.x));
            animator.SetFloat(VelocityY, Mathf.Abs(_velocity.y));
        }

        private void Flip() {
            isFacingRight = !isFacingRight;
            _spriteRenderer.flipX = !isFacingRight;
        }

        private void AttackPlayer() {
            if (_attackCooldownTimer > 0f || ThereIsAWallBetweenEnemyAndPlayer()) {
                return;
            }

            // If melee enemy after attack will be in wall, don't attack
            if (enemyType == EnemyType.Melee && !CanAttackWallCheck()) {
                return;
            }

            animator.SetTrigger(Attack);
            _attackCooldownTimer = cooldownTime;
        }

        private bool CanAttackWallCheck() {
            Vector3 enemyPosition = transform.position;
            const float extraOffset = 0.5f;
            Vector2 direction = isFacingRight ? Vector2.right : Vector2.left;
            RaycastHit2D hit = Physics2D.Raycast(enemyPosition, direction, distanceAfterAttack + extraOffset, Ground);
            Debug.DrawRay(enemyPosition, direction * (distanceAfterAttack + extraOffset), Color.red);
            return hit.collider == null;
        }

        public void TakeDamage(float damage) {
            ChasePlayer();
            _health.DecrementHealth(damage);
            if (HasHealthBar) {
                enemyHealthBar.UpdateHealthBar(_health.CurrentHealth, _health.maxHealth);
                enemyHealthBar.ShowFloatingHealthBar();
            }

            if (!_health.IsAlive) {
                Schedule<EnemyDeath>().enemy = this;
                animator.SetTrigger(Death);
                _attackCooldownTimer = 0f;
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
            transform.position = new Vector3(enemyPosition.x + offsetOnFinishAttack, enemyPosition.y, enemyPosition.z);
        }

        private void BouncePlayer(bool bounceOnAllDirections = false, float bounceForceDecrease = 1f) {
            float decreasedBounceForce = bounceForce * bounceForceDecrease;
            const float verticalPercentage = 0.7f;
            if (bounceOnAllDirections) {
                CurrentPlayer.BounceX(isFacingRight ? decreasedBounceForce : -decreasedBounceForce);
                CurrentPlayer.BounceY(decreasedBounceForce * verticalPercentage);
            } else {
                CurrentPlayer.BounceX(isFacingRight ? decreasedBounceForce : -decreasedBounceForce);
                if (!CurrentPlayer.IsGrounded) {
                    CurrentPlayer.BounceY(decreasedBounceForce * verticalPercentage);
                }
            }
        }

        private void BouncePlayerOnAnimation() {
            if (PlayerInAttackRange) {
                BouncePlayer();
                CurrentPlayer.lives.DecrementLives(attackDamage);
            }
        }

        private void RangedAttack() {
            Vector2 enemyPos = transform.position;
            Vector2 spawnPos = new Vector2(enemyPos.x + GetProjectileOffset(), enemyPos.y + projectileOffset.y);
            Vector2 playerPos = CurrentPlayer.transform.position;
            Vector2 direction = (playerPos - enemyPos).normalized;
            RaycastHit2D hit = Physics2D.Raycast(spawnPos, direction, Vector2.Distance(enemyPos, playerPos),
                Ground);
            if (hit.collider != null && hit.collider.gameObject != CurrentPlayer.gameObject) {
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