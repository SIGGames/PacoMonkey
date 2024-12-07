using Controllers;
using Enums;
using Managers;
using UnityEngine;
using UnityEngine.Tilemaps;
using Utils;

namespace Mechanics.Movement {
    [RequireComponent(typeof(PlayerController))]
    public class Crouch : MonoBehaviour {
        private bool _isCrouching;
        private bool _isSliding;
        private bool _keepFalling;
        private float _slideTimer;
        private float _groundCheckDelay = 0.5f;
        private float _groundCheckTimer;

        [Header("Crouch Configuration")]
        [Range(0, 1)] public float crouchSpeedMultiplier = 0.5f;

        [Range(0, 5)]
        public float slideDuration = 1.5f;

        [Range(0, 5)]
        public float slideCooldown = 1f;

        private float _slideCooldownTimer;

        [Range(0, 10)]
        public float slideMinSpeedMultiplier = 0.5f;

        [SerializeField] private Vector2 cameraOffsetOnCrouch = new Vector2(0f, -1.3f);

        [Header("Collider Configuration")]
        [SerializeField] private Vector2 crouchColliderOffset = new Vector2(0, -0.09f);

        [SerializeField] private Vector2 crouchColliderSize = new Vector2(0.2f, 0.47f);

        [SerializeField] private Vector2 standingColliderOffset = new Vector2(0, 0.1f);
        [SerializeField] private Vector2 standingColliderSize = new Vector2(0.2f, 0.9f);

        [Header("Tilemap for One-Way Platforms")]
        [Tooltip("GameObject with TilemapCollider2D to interact with when crouching.")]
        public GameObject oneWayTilemapGameObject;

        [Header("References")]
        public ColliderManager colliderManager;

        public Animator animator;

        private PlayerController _playerController;
        private TilemapCollider2D _tilemapCollider;

        private void Awake() {
            _slideTimer = slideDuration;

            if (colliderManager == null) {
                Collider2D collider = GetComponent<Collider2D>();
                colliderManager = new ColliderManager(collider);
            }

            _playerController = GetComponent<PlayerController>();

            if (oneWayTilemapGameObject != null) {
                _tilemapCollider = oneWayTilemapGameObject.GetComponent<TilemapCollider2D>();
            }
        }

        private void Update() {
            HandleCrouchInput();

            if (_slideCooldownTimer > 0) {
                _slideCooldownTimer -= Time.deltaTime;
            }

            if (_keepFalling) {
                HandleGroundCheck();
            }
        }

        private void HandleCrouchInput() {
            if (_playerController == null) {
                return;
            }

            bool crouchKeyHeld = KeyBinds.GetCrouchKey();
            bool isRunning = Mathf.Abs(_playerController.move.x) > 0.1f;

            PerformCrouch(crouchKeyHeld, isRunning);
        }

        public void PerformCrouch(bool crouchKeyHeld, bool isRunning) {
            if (_isSliding) {
                UpdateSlide(crouchKeyHeld);
                return;
            }

            if (_isCrouching != crouchKeyHeld) {
                if (crouchKeyHeld) {
                    StartCrouch(isRunning);
                }
                else {
                    TryEndCrouch();
                }
            }
        }

        private void StartCrouch(bool isRunning) {
            _isCrouching = true;
            _playerController.SetSpeedMultiplier(crouchSpeedMultiplier);

            if (animator != null) {
                animator.SetBool("isCrouching", true);
            }

            CameraManager.Instance.SetOffset(cameraOffsetOnCrouch);

            colliderManager.UpdateCollider(true, crouchColliderOffset, crouchColliderSize);

            if (isRunning) {
                StartSlide();
            }
            else {
                _playerController.SetMovementState(PlayerMovementState.Crouch, true);
            }

            _playerController.targetVelocity.x *= crouchSpeedMultiplier;

            if (_tilemapCollider != null) {
                _tilemapCollider.enabled = false;
                _keepFalling = true;
                _groundCheckTimer = _groundCheckDelay;
            }
        }

        private void TryEndCrouch() {
            colliderManager.UpdateCollider(false, standingColliderOffset, standingColliderSize);

            Collider2D[] overlaps = Physics2D.OverlapBoxAll(
                transform.position + (Vector3)standingColliderOffset,
                standingColliderSize,
                0
            );

            bool canStand = true;
            foreach (var overlap in overlaps) {
                if (overlap != null && overlap != _playerController.collider2d) {
                    // TransparentFX excluded
                    if (overlap.gameObject.layer == 1) {
                        continue;
                    }

                    canStand = false;
                    break;
                }
            }

            if (canStand) {
                EndCrouch();
            }
            else {
                colliderManager.UpdateCollider(true, crouchColliderOffset, crouchColliderSize);
            }
        }

        private void EndCrouch() {
            _isCrouching = false;
            _isSliding = false;
            _slideTimer = slideDuration;
            _slideCooldownTimer = 0f;

            _playerController.SetSpeedMultiplier();

            if (animator != null) {
                animator.SetBool("isCrouching", false);
            }

            CameraManager.Instance.SetOffset(Vector2.zero);

            colliderManager.UpdateCollider(false, standingColliderOffset, standingColliderSize);

            _playerController.UnlockMovementState();
            _playerController.SetMovementState(PlayerMovementState.Idle);
        }

        private void StartSlide() {
            if (_slideCooldownTimer > 0) {
                return;
            }

            _isSliding = true;
            _slideTimer = slideDuration;
            _slideCooldownTimer = slideCooldown;
        }

        private void UpdateSlide(bool crouchKeyHeld) {
            _slideTimer -= Time.deltaTime;

            float targetSlideSpeed = _playerController.maxRunSpeed * slideMinSpeedMultiplier;
            float slideProgress = 1 - (_slideTimer / slideDuration);
            float slideSpeed = Mathf.Lerp(_playerController.maxRunSpeed, targetSlideSpeed, slideProgress);
            _playerController.targetVelocity.x = _playerController.isFacingRight ? slideSpeed : -slideSpeed;

            if (_slideTimer <= 0) {
                _isSliding = false;

                if (crouchKeyHeld && _slideCooldownTimer <= 0 && Mathf.Abs(_playerController.move.x) > 0.1f) {
                    StartSlide();
                }
            }

            if (!crouchKeyHeld) {
                EndSlide();
            }
        }

        private void EndSlide() {
            _isSliding = false;
            _slideTimer = slideDuration;
        }

        private void HandleGroundCheck() {
            if (_groundCheckTimer > 0) {
                _groundCheckTimer -= Time.deltaTime;
                return;
            }

            if (_playerController.IsGrounded) {
                _tilemapCollider.enabled = true;
                _keepFalling = false;
            }
        }
    }
}