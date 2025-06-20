﻿using Controllers;
using Enums;
using Gameplay;
using UI;
using UnityEngine;
using Utils;
using static Utils.AnimatorUtils;
using static PlayerInput.KeyBinds;
using static Utils.LayerUtils;

namespace Mechanics.Movement {
    [RequireComponent(typeof(PlayerController))]
    public class WallClimb : MonoBehaviour {
        [Header("Wall Climbing Settings")]
        [SerializeField] private float climbSpeed = 2f;

        [Range(0, 5)]
        [SerializeField] private float slideEffect = 5f;

        [Tooltip("Offset applied to the player when attached to the wall")]
        [SerializeField] private Vector2 attachOffset = new(0.2f, 0.3f);

        [Header("References")]
        [SerializeField] private LedgeDetection ledgeCheck;

        [SerializeField] private PlayerController player;

        private bool _isClimbing;
        private bool _canAttach;
        private float _gravityModifier;

        private Animator _animator;

        private void Awake() {
            if (player == null) {
                player = GetComponent<PlayerController>();
            }

            if (ledgeCheck == null) {
                ledgeCheck = GetComponentInChildren<LedgeDetection>();
            }

            _animator = GetComponent<Animator>();

            _gravityModifier = player.gravityModifier;

            if (player == null || ledgeCheck == null) {
                Debugger.Log(("Player", player), ("LedgeCheck", ledgeCheck));
                enabled = false;
            }
        }

        private void Update() {
            if (MetaGameController.IsMenuOpen) {
                return;
            }

            if (!player.lives.IsAlive) {
                if (_isClimbing) {
                    StopClimbing();
                    player.FreezeHorizontalPosition();
                }
                return;
            }

            if (!_isClimbing && ledgeCheck.isCloseToClimbableWall && ledgeCheck.isNearWall && !player.IsGrounded) {
                StartClimbing();
            } else if (!_isClimbing && ledgeCheck.isCloseToClimbableWall && ledgeCheck.isNearWall && GetUpKey()) {
                StartClimbing();
            } else if (_isClimbing) {
                HandleClimbing();
            }
        }

        private void StartClimbing() {
            if (GetCrouchKey() || IsBelowWall()) {
                return;
            }
            player.SetBodyType(RigidbodyType2D.Kinematic);

            _isClimbing = true;
            player.IsGrounded = false;

            player.FreezeHorizontalPosition();
            SetClimbingState(true);

            float offsetX = player.isFacingRight ? attachOffset.x : -attachOffset.x;
            player.AddPosition(offsetX, attachOffset.y);

            player.SetMovementState(PlayerMovementState.WallClimb, 3);
            _animator.SetBool(IsClimbing, true);
        }

        private void HandleClimbing() {
            if (player.movementState != PlayerMovementState.WallClimb || player.IsGrounded || IsGrounded()) {
                StopClimbing();
                return;
            }

            float verticalInput = GetVerticalAxis();

            // Prevent player from climbing through the ceiling
            if (IsCeiling() && verticalInput > 0) {
                verticalInput = 0;
            }

            // This has been disabled since now its disabled by HoldV2 on the right moment
            if (!ledgeCheck.IsGroundAbove() && player.lives.IsAlive && _isClimbing) {
                // StopClimbing();
                 // _hold.StartHold();
            }

            if (verticalInput != 0) {
                player.AddPosition(0, climbSpeed * Time.deltaTime * verticalInput);
                player.velocity.y = climbSpeed * verticalInput;
            } else {
                player.velocity.y = 0f;
            }

            if (player.velocity.y <= 0f) {
                _animator.SetFloat(IsTowardsUp, -1f);
            } else {
                _animator.SetFloat(IsTowardsUp, 1f);
            }

            bool isPressingOppositeDirection = (player.isFacingRight && GetLeftKey()) ||
                                               (!player.isFacingRight && GetRightKey());

            if (isPressingOppositeDirection && verticalInput == 0) {
                _animator.SetFloat(IsTowardsUp, 0f);
                _animator.SetBool(IsHoldingJumpOnClimb, true);
                player.flipManager.Flip(!player.isFacingRight);
            } else {
                _animator.SetBool(IsHoldingJumpOnClimb, false);
                player.flipManager.Flip(player.isFacingRight);
            }

            if (GetJumpKeyDown()) {
                if (!isPressingOppositeDirection) {
                    return;
                }

                StopClimbing();

                // This offset is needed since on the compiled version the player falls on wall jumping since is colliding with something I can't see
                // So, this approach adds offset depending on the distance to the wall, with a minimum and maximum distance
                #if !UNITY_EDITOR
                    const float minRayDistance = 0.3f;
                    const float maxRayDistance = 0.5f;
                    Vector2 origin = transform.position;
                    Vector2 direction = player.isFacingRight ? Vector2.right : Vector2.left;
                    RaycastHit2D hit = Physics2D.Raycast(origin, direction, maxRayDistance, Ground.value);

                    float wallDistance = hit.collider != null ? hit.distance : maxRayDistance;
                    float offsetToAdd = Mathf.Clamp(minRayDistance + (maxRayDistance - wallDistance), minRayDistance, maxRayDistance);

                    player.AddPosition(player.isFacingRight ? -offsetToAdd : offsetToAdd);
                #endif

                _animator.SetBool(IsHoldingJumpOnClimb, false);
                _animator.SetBool(IsJumping, true);

                float jumpReductionFactor = 1f;
                #if !UNITY_EDITOR
                    jumpReductionFactor = 0.8f; // Reduce jump force on compiled version since its more powerful for some reason
                #endif
                player.velocity = new Vector2((player.isFacingRight ? -1f : 1f) * climbSpeed, player.jumpTakeOffSpeed * jumpReductionFactor);
                player.StartJump();
                player.flipManager.Flip(!player.isFacingRight);
                return;
            }

            // In case there is a ledge or the player wants to stop climbing
            if ((!ledgeCheck.isNearWall || ledgeCheck.isNearLedge) &&
                player.movementState != PlayerMovementState.Hold) {
                StopClimbing();
            }
        }

        public void StopClimbing() {
            if (player.movementState != PlayerMovementState.Hold) {
                // Rise a bit the player vertical position to help ledge detection
                player.AddPosition(0, 0.05f);
                _isClimbing = false;
                player.FreezeHorizontalPosition(false);
                SetClimbingState(false);
                player.UnlockMovementState();
                player.SetVelocity(Vector2.zero);
                _animator.SetBool(IsClimbing, false);
                _animator.SetBool(IsHoldingJumpOnClimb, false);
            }
        }

        private bool IsGrounded() {
            // This method is used to check if the player is close to the ground while climbing
            const float rayLength = 0.5f;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, rayLength, Ground.value);
            return player.IsGrounded = hit.collider != null;
        }

        private bool IsCeiling() {
            // This method is used to check if the player is close to a ceiling while climbing
            const float rayLength = 0.5f;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.up, rayLength, Ground.value);
            return hit.collider != null;
        }

        private bool IsBelowWall() {
            const float rayLength = 0.5f;
            // An offset it's necessary to avoid the raycast hit the current climbing wall is a ceiling
            const float xOffset = 0.1f;
            Vector2 origin = new Vector2(transform.position.x + (player.isFacingRight ? xOffset : -xOffset), transform.position.y);
            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.up, rayLength, Ground.value);
            return hit.collider != null;
        }

        private void SetClimbingState(bool isClimbing) {
            if (isClimbing) {
                SetGravity(slideEffect);
            } else {
                SetGravity(_gravityModifier * 100);
            }
        }

        private void SetGravity(float value) {
            if (value > 0) {
                player.gravityModifier = value / 100;
            } else {
                player.gravityModifier = 0;
            }

            if (player.body.bodyType != RigidbodyType2D.Static) {
                player.body.velocity = Vector2.zero;
                player.velocity = Vector2.zero;
            }
        }
    }
}