using System.Diagnostics.CodeAnalysis;
using Configuration;
using Enums;
using Gameplay;
using Health.UI;
using Managers;
using Mechanics;
using Platformer.Gameplay;
using UnityEngine;
using UnityEngine.AI;
using static Platformer.Core.Simulation;
using static Utils.AnimatorUtils;

namespace Controllers {
    [RequireComponent(typeof(AnimationController), typeof(Collider2D)),
     RequireComponent(typeof(AudioSource)), RequireComponent(typeof(SpriteRenderer)),
     RequireComponent(typeof(Health.Health))]
    [SuppressMessage("ReSharper", "Unity.InefficientPropertyAccess")]
    public class EnemyController : MonoBehaviour {
        public AudioClip ouch;

        internal PatrolPath.Mover mover;
        internal Collider2D col;
        internal AudioSource audioSource;
        SpriteRenderer _spriteRenderer;
        private Health.Health _health;
        [SerializeField] private Animator animator;
        [SerializeField] FloatingHealthBar healthBar;
        private static CharacterManager CharacterManager => CharacterManager.Instance;
        private Character _currentCharacter;
        private PlayerController _currentPlayer;

        [SerializeField] private EnemyType enemyType = EnemyType.Melee;

        [Header("AI Settings")]
        public NavMeshAgent navAgent;

        public LayerMask groundLayer, playerLayer;
        public float sightRange = 1.5f;
        public float attackRange = 1f;
        [SerializeField] private float distanceAfterAttack = 0.5f;

        [SerializeField] private float fallSpeedMultiplier = 2.5f;
        private Vector3 _walkPoint;
        [SerializeField] private bool playerInSightRange;
        [SerializeField] private bool playerInAttackRange;

        private bool HasHealthBar => healthBar != null;
        private Vector2 _velocity = Vector2.zero;
        [SerializeField] private bool isFacingRight = true;

        public Bounds Bounds => col.bounds;

        void Awake() {
            col = GetComponent<Collider2D>();
            audioSource = GetComponent<AudioSource>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            if (healthBar == null) {
                healthBar = GetComponentInChildren<FloatingHealthBar>();
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
            _currentCharacter = CharacterManager.GetCurrentCharacter();
            _currentPlayer = CharacterManager.GetCurrentCharacterController();

            if (HasHealthBar) {
                healthBar.UpdateHealthBar(_health.CurrentHealth, _health.maxHealth);
            }
        }

        void OnCollisionEnter2D(Collision2D collision) {
            var player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null) {
                var ev = Schedule<PlayerEnemyCollision>();
                ev.player = player;
                ev.enemy = this;
            }
        }

        void Update() {
            if (Input.GetKeyDown(KeyCode.F2)) {
                // navAgent.isStopped = true;
                animator.SetTrigger(Attack);
            }

            if (_currentCharacter != CharacterManager.GetCurrentCharacter()) {
                _currentCharacter = CharacterManager.GetCurrentCharacter();
                _currentPlayer = CharacterManager.GetCurrentCharacterController();
            }

            float distanceToPlayer = Vector3.Distance(transform.position, _currentPlayer.transform.position);
            playerInSightRange = distanceToPlayer <= sightRange;
            playerInAttackRange = distanceToPlayer <= attackRange;

            if (playerInSightRange && !playerInAttackRange) {
                ChasePlayer();
            } else if (playerInAttackRange) {
                // AttackPlayer();
            } else {
                // navAgent.ResetPath();
                navAgent.isStopped = true;
                animator.SetFloat(VelocityX, 0f);
            }

            HandleLives();
            ApplyGravity();
            HandleFlip();
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

        private void ApplyGravity() {
            /*float gravityScale = GlobalConfiguration.GravityScale * Time.deltaTime;
            if (_velocity.y < 0) {
                gravityScale *= fallSpeedMultiplier;
            }

            _velocity += Physics2D.gravity * gravityScale;*/
        }

        private void ChasePlayer() {
            navAgent.SetDestination(_currentPlayer.transform.position);
            animator.SetFloat(VelocityX, 0.6f);
            navAgent.isStopped = false;
        }

        void HandleLives() {
            if (_health.IsAlive) {
                _spriteRenderer.color = Color.white;
            } else {
                _spriteRenderer.color = Color.black;
            }
        }

        private void Flip() {
            isFacingRight = !isFacingRight;
            _spriteRenderer.flipX = !isFacingRight;
        }

        private void AttackPlayer() {
            var ev = Schedule<PlayerEnemyCollision>();
            ev.player = _currentPlayer;
            ev.enemy = this;
        }

        public void TakeDamage(float damage) {
            _health.DecrementHealth(damage);
            if (HasHealthBar) {
                healthBar.UpdateHealthBar(_health.CurrentHealth, _health.maxHealth);
                healthBar.ShowFloatingHealthBar();
            }

            if (!_health.IsAlive) {
                Schedule<EnemyDeath>().enemy = this;
            }
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