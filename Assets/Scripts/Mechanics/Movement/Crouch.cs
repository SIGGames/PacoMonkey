using Controllers;
using Enums;
using Managers;
using UnityEngine;
using Utils;

namespace Mechanics.Movement {
    [RequireComponent(typeof(PlayerController))]
    public class Crouch : MonoBehaviour {
        private bool _isCrouching;
        private bool _isSliding;
        private float _slideTimer;

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

        [Header("References")]
        public ColliderManager colliderManager;

        public Animator animator;

        private PlayerController _playerController;

        private void Awake() {
            _slideTimer = slideDuration;

            if (colliderManager == null) {
                Collider2D collider = GetComponent<Collider2D>();
                colliderManager = new ColliderManager(collider);
            }

            _playerController = GetComponent<PlayerController>();
        }

        private void Update() {
            HandleCrouchInput();

            if (_slideCooldownTimer > 0) {
                _slideCooldownTimer -= Time.deltaTime;
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
                    EndCrouch();
                }
            }
        }

        private void StartCrouch(bool isRunning) {
            _isCrouching = true;

            if (animator != null) animator.SetBool("isCrouching", true);
            CameraManager.Instance.SetOffset(cameraOffsetOnCrouch);

            colliderManager.UpdateCollider(true, crouchColliderOffset, crouchColliderSize);

            if (isRunning) {
                StartSlide();
            }
            else {
                _playerController.SetMovementState(PlayerMovementState.Crouch, true);
            }

            _playerController.targetVelocity.x *= crouchSpeedMultiplier;
        }

        private void EndCrouch() {
            _isCrouching = false;
            _isSliding = false;
            _slideTimer = slideDuration;
            _slideCooldownTimer = 0f;

            if (animator != null) animator.SetBool("isCrouching", false);
            CameraManager.Instance.SetOffset(Vector2.zero);

            colliderManager.UpdateCollider(false, Vector2.zero, Vector2.zero);

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
    }
}