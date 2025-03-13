using System.Diagnostics.CodeAnalysis;
using Managers;
using UnityEngine;

namespace Controllers {
    [RequireComponent(typeof(Collider2D)), RequireComponent(typeof(AudioSource)),
     RequireComponent(typeof(SpriteRenderer)), RequireComponent(typeof(Health.Health))]
    [SuppressMessage("ReSharper", "Unity.InefficientPropertyAccess")]
    public class BossController : MonoBehaviour {
        private static PlayerController CurrentPlayer => CharacterManager.Instance.currentPlayerController;

        [Header("Components")]
        [SerializeField]
        private SpriteRenderer spriteRenderer;

        [SerializeField]
        private Animator animator;

        [SerializeField]
        private AudioSource audioSource;

        [SerializeField]
        private Collider2D col;

        [SerializeField]
        private Health.Health health;

        [Header("Movement Settings")]
        [SerializeField] private bool isFacingRight = true;

        private Vector2 BossPosition => transform.position;
        private static Vector2 PlayerPosition => CurrentPlayer.transform.position;


        private void Awake() {
            col = GetComponent<Collider2D>();
            audioSource = GetComponent<AudioSource>();
            spriteRenderer = GetComponent<SpriteRenderer>();

            if (health == null) {
                health = GetComponent<Health.Health>();
            }

            if (animator == null) {
                animator = GetComponent<Animator>();
            }

            if (col == null || audioSource == null || spriteRenderer == null || health == null || animator == null) {
                enabled = false;
            }
        }


        private void Update() {
            HandleFlip();
        }


        private void HandleFlip() {
            if (PlayerPosition.x > BossPosition.x && !isFacingRight) {
                Flip();
            } else if (PlayerPosition.x < BossPosition.x && isFacingRight) {
                Flip();
            }
        }

        private void Flip() {
            isFacingRight = !isFacingRight;
            spriteRenderer.flipX = !isFacingRight;
        }
    }
}