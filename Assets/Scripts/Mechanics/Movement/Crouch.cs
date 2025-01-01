using Controllers;
using Enums;
using Gameplay;
using Managers;
using UnityEngine;
using UnityEngine.Tilemaps;
using Utils;
using static PlayerInput.KeyBinds;

namespace Mechanics.Movement {
    [RequireComponent(typeof(PlayerController))]
    public class Crouch : MonoBehaviour {
        private bool _isCrouching;
        private bool _isSliding;
        private bool _keepFalling;
        private float _slideTimer;
        private readonly float _groundCheckDelay = 0.5f;
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
        private ColliderManager _colliderManager;

        public Animator animator;
        [SerializeField] private LedgeDetection ledgeCheck;

        private PlayerController _playerController;
        private TilemapCollider2D _tilemapCollider;
        private static readonly int IsCrouching = Animator.StringToHash("isCrouching");
        private const float DelayTimeAfterCrouch = 0.5f;

        private void Awake() {
            _slideTimer = slideDuration;

            if (_colliderManager == null) {
                Collider2D collider2d = GetComponent<Collider2D>();
                _colliderManager = new ColliderManager(collider2d);
            }

            _playerController = GetComponent<PlayerController>();

            if (animator == null) {
                animator = GetComponent<Animator>();
            }

            if (ledgeCheck == null) {
                ledgeCheck = GetComponent<LedgeDetection>();
            }

            if (oneWayTilemapGameObject != null) {
                _tilemapCollider = oneWayTilemapGameObject.GetComponent<TilemapCollider2D>();
            }

            if (_playerController == null || _colliderManager == null || animator == null || ledgeCheck == null ||
                (_tilemapCollider == null && oneWayTilemapGameObject != null)) {
                Debugger.Log(("PlayerController", _playerController), ("ColliderManager", _colliderManager),
                    ("Animator", animator), ("LedgeCheck", ledgeCheck), ("TilemapCollider", _tilemapCollider));
                enabled = false;
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

        private void PerformCrouch() {
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

            if (crouchKeyHeld && !_isCrouching &&
                PlayerMovementStateMethods.IsPlayerAbleToCrouch(_playerController.movementState)) {
                StartCrouch(isRunning);
            }
        }

        private void StartCrouch(bool isRunning) {
            _isCrouching = true;
            _playerController.SetSpeedMultiplier(crouchSpeedMultiplier);

            animator.SetBool(IsCrouching, true);
            CameraManager.Instance.SetOffset(cameraOffsetOnCrouch);

            ledgeCheck.transform.position += new Vector3(ledgeCheckOffsetOnCrouch.x, ledgeCheckOffsetOnCrouch.y, 0);

            _colliderManager.UpdateCollider(true, crouchColliderSize);

            if (isRunning) {
                StartSlide();
            } else {
                _playerController.SetMovementState(PlayerMovementState.Crouch, 2);
            }

            _playerController.targetVelocity.x *= crouchSpeedMultiplier;

            _tilemapCollider.enabled = false;
            _keepFalling = true;
            _groundCheckTimer = _groundCheckDelay;
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

            _colliderManager.UpdateCollider(false, standingColliderSize);

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
            Vector2 position = (Vector2)transform.position + _colliderManager.GetOriginalOffset();
            Vector2 size = _colliderManager.GetOriginalSize();

            Collider2D[] collisions = Physics2D.OverlapBoxAll(position, size, 0f);

            foreach (var collision in collisions) {
                // Ground layer
                if (collision != null && collision.gameObject.layer == 7) {
                    return false;
                }
            }

            return true;
        }
    }
}