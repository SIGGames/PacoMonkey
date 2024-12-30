using Controllers;
using Enums;
using Gameplay;
using Managers;
using UnityEngine;
using UnityEngine.Tilemaps;
using static PlayerInput.KeyBinds;

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

        [SerializeField] private Vector2 cameraOffsetOnCrouch = new(0f, -1.3f);

        [Header("Collider Configuration")]
        [SerializeField] private Vector2 crouchColliderSize = new(0.2f, 0.47f);

        [SerializeField] private Vector2 ledgeCheckOffsetOnCrouch = new(0, -0.35f);

        [SerializeField] private Vector2 standingColliderSize = new(0.2f, 0.9f);

        [Header("Tilemap for One-Way Platforms")]
        [Tooltip("GameObject with TilemapCollider2D to interact with when crouching.")]
        public GameObject oneWayTilemapGameObject;

        [Header("References")]
        public ColliderManager colliderManager;

        public Animator animator;
        [SerializeField] private LedgeDetection ledgeCheck;

        private PlayerController _playerController;
        private TilemapCollider2D _tilemapCollider;
        private static readonly int IsCrouching = Animator.StringToHash("isCrouching");
        private bool _isledgeCheckNotNull;
        private const float DelayTimeAfterCrouch = 0.5f;

        private void Awake() {
            _slideTimer = slideDuration;

            if (colliderManager == null) {
                Collider2D collider2d = GetComponent<Collider2D>();
                colliderManager = new ColliderManager(collider2d);
            }

            _playerController = GetComponent<PlayerController>();

            if (_playerController == null) {
                Debug.LogError("Crouch script requires a PlayerController component");
                enabled = false;
                return;
            }

            if (animator == null) {
                animator = GetComponent<Animator>();
            }

            if (animator == null) {
                Debug.LogError("Crouch script requires an Animator component");
                enabled = false;
                return;
            }

            if (ledgeCheck == null) {
                ledgeCheck = GetComponent<LedgeDetection>();
            }

            _isledgeCheckNotNull = ledgeCheck != null;

            if (oneWayTilemapGameObject != null) {
                _tilemapCollider = oneWayTilemapGameObject.GetComponent<TilemapCollider2D>();
            }
        }

        private void Update() {
            PerformCrouch();

            if (_slideCooldownTimer > 0) {
                _slideCooldownTimer -= Time.deltaTime;
            }

            if (_keepFalling) {
                HandleGroundCheck();
            }
        }

        public void PerformCrouch() {
            bool crouchKeyHeld = GetCrouchKey();
            bool isRunning = Mathf.Abs(_playerController.move.x) > 0.1f;

            if (_isSliding) {
                UpdateSlide(crouchKeyHeld);
                return;
            }

            if (!crouchKeyHeld && _isCrouching) {
                EndCrouch();
                return;
            }

            if (crouchKeyHeld && !_isCrouching) {
                StartCrouch(isRunning);
            }
        }

        private void StartCrouch(bool isRunning) {
            _isCrouching = true;
            _playerController.SetSpeedMultiplier(crouchSpeedMultiplier);

            animator.SetBool(IsCrouching, true);
            CameraManager.Instance.SetOffset(cameraOffsetOnCrouch);

            ledgeCheck.transform.position += new Vector3(ledgeCheckOffsetOnCrouch.x, ledgeCheckOffsetOnCrouch.y, 0);

            colliderManager.UpdateCollider(true, crouchColliderSize);

            if (isRunning) {
                StartSlide();
            } else {
                _playerController.SetMovementState(PlayerMovementState.Crouch, true);
            }

            _playerController.targetVelocity.x *= crouchSpeedMultiplier;

            if (_tilemapCollider != null) {
                _tilemapCollider.enabled = false;
                _keepFalling = true;
                _groundCheckTimer = _groundCheckDelay;
            }
        }

        private void EndCrouch() {
            if (!CanResizeCollider()) {
                return;
            }

            _isCrouching = false;
            _isSliding = false;
            _slideTimer = slideDuration;
            _slideCooldownTimer = 0f;

            _playerController.SetSpeedMultiplier();

            animator.SetBool(IsCrouching, false);

            CameraManager.Instance.SetOffset(Vector2.zero);

            ledgeCheck.transform.position -= new Vector3(ledgeCheckOffsetOnCrouch.x, ledgeCheckOffsetOnCrouch.y, 0);

            colliderManager.UpdateCollider(false, standingColliderSize);

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

        private bool CanResizeCollider() {
            if (_isledgeCheckNotNull) {
                Vector3 ledgeCheckPosition = ledgeCheck.transform.position + new Vector3(0, 0.2f, 0);
                Collider2D[] ledgeCheckCollisions = Physics2D.OverlapCircleAll(ledgeCheckPosition, 0.1f);

                foreach (var collision in ledgeCheckCollisions) {
                    if (collision != null && collision.gameObject.layer == 7) {
                        // Layer 7 (Ground)
                        return false;
                    }
                }
            }

            return true;
        }
    }
}