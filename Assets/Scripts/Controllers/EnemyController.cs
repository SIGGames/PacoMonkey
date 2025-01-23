using Gameplay;
using Health.UI;
using Mechanics;
using Platformer.Gameplay;
using UnityEngine;
using static Platformer.Core.Simulation;

namespace Controllers {
    [RequireComponent(typeof(AnimationController), typeof(Collider2D)), RequireComponent(typeof(AudioSource)), RequireComponent(typeof(SpriteRenderer)), RequireComponent(typeof(Health.Health))]
    public class EnemyController : MonoBehaviour {
        public PatrolPath path;
        public AudioClip ouch;

        internal PatrolPath.Mover mover;
        internal AnimationController control;
        internal Collider2D _collider;
        internal AudioSource _audio;
        SpriteRenderer spriteRenderer;
        public Health.Health health;
        [SerializeField] FloatingHealthBar healthBar;

        public Bounds Bounds => _collider.bounds;

        void Awake() {
            control = GetComponent<AnimationController>();
            _collider = GetComponent<Collider2D>();
            _audio = GetComponent<AudioSource>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            healthBar = GetComponentInChildren<FloatingHealthBar>();
            health = GetComponent<Health.Health>();
        }

        private void Start() {
            if (healthBar != null) {
                healthBar.UpdateHealthBar(health.CurrentHealth, health.maxHealth);
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
            if (path != null) {
                if (mover == null) mover = path.CreateMover(control.maxSpeed * 0.5f);
                control.move.x = Mathf.Clamp(mover.Position.x - transform.position.x, -1, 1);
            }

            HandleLives();
        }

        void HandleLives() {
            if (health.IsAlive) {
                spriteRenderer.color = Color.red;
            }
            else {
                spriteRenderer.color = Color.black;
            }

            // TODO: Remove this
            if (Input.GetKeyDown(KeyCode.U)) {
                TakeDamage(10);
            }
        }

        public void TakeDamage(float damage) {
            health.CurrentHealth -= damage;
            if (healthBar != null) {
                healthBar.UpdateHealthBar(health.CurrentHealth, health.maxHealth);
                healthBar.ShowFloatingHealthBar();
            }

            if (!health.IsAlive) {
                Schedule<EnemyDeath>().enemy = this;
            }
        }
    }
}