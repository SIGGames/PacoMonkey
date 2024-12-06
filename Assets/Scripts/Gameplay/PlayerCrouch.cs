using Enums;
using Managers;
using UnityEngine;
using Platformer.Core;

namespace Gameplay {
    public class PlayerCrouch : Simulation.Event<PlayerCrouch> {
        private bool _isCrouching;
        private bool _isSliding;
        private float _slideTimer;

        private ColliderManager _colliderManager;
        private Animator _animator;
        private Vector2 _cameraOffset;

        public float SlideDuration { get; set; }
        public float SlideMinSpeedMultiplier { get; set; }
        public float CrouchSpeedMultiplier { get; set; }

        public PlayerCrouch(
            ColliderManager colliderManager,
            Animator animator,
            Vector2 cameraOffset,
            float slideDuration,
            float slideMinSpeedMultiplier,
            float crouchSpeedMultiplier
        ) {
            _colliderManager = colliderManager;
            _animator = animator;
            _cameraOffset = cameraOffset;
            SlideDuration = slideDuration;
            SlideMinSpeedMultiplier = slideMinSpeedMultiplier;
            CrouchSpeedMultiplier = crouchSpeedMultiplier;
            _slideTimer = slideDuration;
        }

        public void Crouch(bool value, bool isRunning, ref PlayerMovementState movementState, ref float targetSpeed) {
            if (_isCrouching == value && !_isSliding) {
                return;
            }

            _isCrouching = value;

            _colliderManager.UpdateCollider(value);

            if (_animator != null) {
                _animator.SetBool("isCrouching", value);
            }

            CameraManager.Instance.SetOffset(value ? _cameraOffset : Vector2.zero);

            if (value) {
                if (isRunning && !_isSliding) {
                    StartSlide(ref movementState);
                } else {
                    movementState = PlayerMovementState.Crouch;
                }
                targetSpeed *= CrouchSpeedMultiplier;
            } else {
                StopSlide(ref movementState);
            }
        }

        private void StartSlide(ref PlayerMovementState movementState) {
            _isSliding = true;
            _slideTimer = SlideDuration;
            movementState = PlayerMovementState.Crouch;
        }

        private void StopSlide(ref PlayerMovementState movementState) {
            _isSliding = false;
            _slideTimer = SlideDuration;
            movementState = PlayerMovementState.Idle;
        }

        public void UpdateSlide(
            ref PlayerMovementState movementState,
            ref float targetVelocity,
            bool crouchKeyHeld,
            float maxRunSpeed,
            bool isFacingRight
        ) {
            if (_isSliding) {
                _slideTimer -= Time.deltaTime;

                float targetSlideSpeed = maxRunSpeed * SlideMinSpeedMultiplier;
                float slideProgress = 1 - (_slideTimer / SlideDuration);
                float slideSpeed = Mathf.Lerp(maxRunSpeed, targetSlideSpeed, slideProgress);
                targetVelocity = isFacingRight ? slideSpeed : -slideSpeed;

                if (_slideTimer <= 0 || !crouchKeyHeld) {
                    StopSlide(ref movementState);
                }
            }
        }

        public override void Execute() {
        }
    }
}