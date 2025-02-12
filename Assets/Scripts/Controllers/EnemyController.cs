using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Enums;
using Gameplay;
using Health.UI;
using Managers;
using Mechanics;
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

        public LayerMask groundLayer, playerLayer;
        [SerializeField, Range(0, 10)] public float sightRange = 1.5f;
        [SerializeField, Range(0, 5)] public float attackRange = 1f;
        [SerializeField, Range(0, 2)] private float groundOffset = 0.5f;

        [Header("Attack Settings")]
        [SerializeField, HalfStepSlider(0, 10)]
        private float attackDamage = 1f;

        [SerializeField, Range(0, 5)] private float cooldownTime = 1.5f;
        [SerializeField, Range(0, 10)] private float bounceForce = 4f;

        [SerializeField, Range(0, 3)] private float distanceAfterAttack = 1f;

        [Header("Movement Settings")]
        [SerializeField] private float fallSpeedMultiplier = 2.5f;

        [SerializeField] private bool isFacingRight = true;

        private Vector3 _walkPoint;
        [SerializeField] private bool playerInSightRange;
        [SerializeField] private bool playerInAttackRange;

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

        void Awake() {
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

        void OnCollisionEnter2D(Collision2D other) {
            if (other.gameObject.GetComponent<PlayerController>() == null) {
                return;
            }

            _currentPlayer.lives.DecrementLives(attackDamage);
            BouncePlayer();
        }

        void Update() {
            _currentPlayer = CharacterManager.currentPlayerController;
            if (Input.GetKeyDown(KeyCode.F2)) {
                // TODO: This will be removed
                AttackPlayer();
            }

            UpdateVelocity();

            float distanceToPlayer = Vector3.Distance(transform.position, _currentPlayer.transform.position);
            playerInSightRange = distanceToPlayer <= sightRange;
            playerInAttackRange = distanceToPlayer <= attackRange;

            if (_attackCooldownTimer > 0f) {
                _attackCooldownTimer -= Time.deltaTime;
            }

            if (playerInSightRange && !playerInAttackRange) {
                ChasePlayer();
            } else if (playerInAttackRange && _attackCooldownTimer <= 0f) {
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
            if (_currentPlayer.transform.position.x > transform.position.x && !isFacingRight) {
                Flip();
            } else if (_currentPlayer.transform.position.x < transform.position.x && isFacingRight) {
                Flip();
            }
        }

        private void ChasePlayer() {
            Vector2 playerPos = _currentPlayer.transform.position;
            RaycastHit2D hit = Physics2D.Raycast(playerPos, Vector2.down, 100f, groundLayer);
            float targetY = hit.collider != null ? hit.point.y + groundOffset : transform.position.y;
            targetY = Mathf.Min(targetY, transform.position.y);
            Vector2 newDestination = new Vector2(playerPos.x, targetY);
            navAgent.SetDestination(newDestination);
        }

        private void IsGrounded() {
            float rayLength = 0.1f;
            RaycastHit2D hit = Physics2D.Raycast(col.bounds.center, Vector2.down, col.bounds.extents.y + rayLength,
                groundLayer);
            bool isGrounded = hit.collider != null;
            animator.SetBool(Grounded, isGrounded);
        }

        void HandleLives() {
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

            animator.SetTrigger(Attack);
            navAgent.ResetPath();
            _attackCooldownTimer = cooldownTime;
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

        public void SetPositionAfterAttack() {
            Vector3 enemyPosition = transform.position;
            float offsetOnFinishAttack = isFacingRight ? distanceAfterAttack : -distanceAfterAttack;
            Vector3 newPosition = new Vector3(enemyPosition.x + offsetOnFinishAttack, enemyPosition.y, enemyPosition.z);
            transform.position = newPosition;

            // Setting the new position to the navAgent
            Vector3 newPositionOffset = Vector3.zero;
            newPositionOffset.x = offsetOnFinishAttack;
            navAgent.Move(newPositionOffset);

            if (playerInAttackRange) {
                _currentPlayer.lives.DecrementLives(attackDamage);
            }
        }

        private void BouncePlayer() {
            if (playerInAttackRange) {
                _currentPlayer.Bounce(isFacingRight ? bounceForce : -bounceForce);
            }
        }

        private void OnDrawGizmosSelected() {
            if (drawRangesInEditor) {
                Gizmos.color = Color.red;
                Vector3 position = transform.position;
                Gizmos.DrawWireSphere(position, attackRange);
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(position, sightRange);
            }
        }
    }
}