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
using UnityEditor.ColorRangeDrawers;
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

        [SerializeField, ColorRange(0, 10)]
        private ColorRangeValue chaseStopRange = new(1.5f, Color.green);

        [SerializeField, ColorRange(0, 10)]
        public ColorRangeValue attackRange = new(1f, Color.red);

        [Header("Sight Settings")]
        [ShowIf("enemyType", EnemyType.Melee), SerializeField]
        private Vector2 sightBoxSize = new(2f, 1f);

        [ShowIf("enemyType", EnemyType.Melee), SerializeField, MaxValue(5)]
        private Vector2 sightBoxOffset = new(1f, 0f);

        [ShowIf("enemyType", EnemyType.Melee), SerializeField]
        private Color sightBoxColor = Color.yellow;

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
        [SerializeField, Range(0, 3)] private float distanceAfterAttack = 1f; // TODO: Remove this

        // Melee Attack Animation Offset (this is variable depending on the frame of the enemy)
        [SerializeField, HideInInspector] private float attackAnimationOffset;

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

        private bool PlayerInAttackRange => DistanceToPlayer <= attackRange.value;

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

        [ShowIf("enemyType", EnemyType.Melee)]
        [SerializeField] private bool drawAttackAnimationInEditor = true;

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
        private bool _hasBeenAttacked;
        private Vector2 _initialColliderOffset;
        // Enemy can't chase player after being attacked until the animation is finished, so this parameter is used on animation
        [SerializeField, HideInInspector] private bool canChasePlayer = true;

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
            _initialColliderOffset = _col.offset;
            if (HasHealthBar) {
                enemyHealthBar.UpdateHealthBar(_health.CurrentHealth, _health.maxHealth);
            }
        }

        private void OnCollisionEnter2D(Collision2D other) {
            PlayerController player = other.gameObject.GetComponent<PlayerController>();
            if (player == null) {
                return;
            }

            player.IsGrounded = true;
            player.SetVelocity(Vector2.zero);
            player.SetBodyType(RigidbodyType2D.Dynamic);

            AttackPlayer();
        }

        private void OnTriggerExit2D(Collider2D other) {
            CharacterManager.Instance.currentPlayerController.SetBodyType(RigidbodyType2D.Kinematic);
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
            } else if (_hasBeenAttacked) {
                // Call chase player if enemy has been attacked
                ChasePlayer();
            }

            UpdateColliderAndHealthBar();
            HandleFlip();
            IsGrounded();
        }

        private void ChasePlayer() {
            if (!canChasePlayer) {
                return;
            }

            if (DistanceToPlayer <= chaseStopRange.value) {
                return;
            }

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

            // Reset chase player after attack
            _hasBeenAttacked = false;
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
            _hasBeenAttacked = true;

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
            float offsetOnFinishAttack = isFacingRight ? attackAnimationOffset : -attackAnimationOffset;
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
                CurrentPlayer.TakeDamage(attackDamage);
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

        private void UpdateColliderAndHealthBar() {
            float xOffset = isFacingRight ? attackAnimationOffset : -attackAnimationOffset;
            enemyHealthBar.transform.position = transform.position + new Vector3(xOffset, 0.5f, 0f);
            _col.offset = new Vector2((_initialColliderOffset.x + xOffset) * 0.6f, _initialColliderOffset.y);
        }

        private void OnDrawGizmosSelected() {
            Vector3 enemyPosition = transform.position;

            if (drawRangesInEditor) {
                // Attack range
                Gizmos.color = attackRange.color;
                Gizmos.DrawWireSphere(enemyPosition, attackRange.value);

                // Chase stop range
                Gizmos.color = chaseStopRange.color;
                Gizmos.DrawWireSphere(enemyPosition, chaseStopRange.value);

                // Sight range
                if (enemyType == EnemyType.Melee) {
                    Gizmos.color = sightBoxColor;
                    Vector2 boxCenter = (Vector2)enemyPosition +
                                        (isFacingRight ? sightBoxOffset : -sightBoxOffset);
                    Gizmos.DrawWireCube(boxCenter, sightBoxSize);
                }
            }

            // Projectile spawn position
            if (enemyType == EnemyType.Ranged && drawProjectileInEditor) {
                Vector2 spawnPos = new Vector2(enemyPosition.x + GetProjectileOffset(),
                    enemyPosition.y + projectileOffset.y);
                Gizmos.color = new Color(0.6f, 0.3f, 0.0f, 1f);
                Gizmos.DrawSphere(spawnPos, 0.05f);
            }

            // Melee Attack Animation Offset
            if (enemyType == EnemyType.Melee && drawAttackAnimationInEditor) {
                Vector3 sphereAnimationPosition = new Vector3(enemyPosition.x + attackAnimationOffset, enemyPosition.y,
                    enemyPosition.z);
                Gizmos.color = Color.cyan;
                Gizmos.DrawSphere(sphereAnimationPosition, 0.1f);
            }
        }
    }
}