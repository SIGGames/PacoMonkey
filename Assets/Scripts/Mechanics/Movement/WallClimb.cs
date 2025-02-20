﻿using Controllers;
using Enums;
using Gameplay;
using UnityEditor;
using UnityEngine;
using Utils;
using static Utils.AnimatorUtils;
using static PlayerInput.KeyBinds;

namespace Mechanics.Movement {
    [RequireComponent(typeof(PlayerController))]
    public class WallClimb : MonoBehaviour {
        [Header("Wall Climbing Settings")]
        [SerializeField] private float climbSpeed = 2f;

        [TagSelector] [SerializeField] private string wallTag = "ClimbableWall";

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
        private Hold _hold;

        private void Awake() {
            if (player == null) {
                player = GetComponent<PlayerController>();
            }

            if (ledgeCheck == null) {
                ledgeCheck = GetComponentInChildren<LedgeDetection>();
            }

            if (_hold == null) {
                _hold = GetComponent<Hold>();
            }

            _animator = GetComponent<Animator>();

            _gravityModifier = player.gravityModifier;

            if (player == null || ledgeCheck == null || _hold == null) {
                Debugger.Log(("Player", player), ("LedgeCheck", ledgeCheck), ("Hold", _hold));
                enabled = false;
            }
        }

        private void Update() {
            if (!_isClimbing && _canAttach && ledgeCheck.isNearWall && GetUpKey()) {
                StartClimbing();
            } else if (_isClimbing) {
                HandleClimbing();
            }
        }

        private void StartClimbing() {
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
            if (player.movementState != PlayerMovementState.WallClimb || player.IsGrounded) {
                StopClimbing();
                return;
            }

            float verticalInput = GetVerticalAxis();

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

            bool isPressingOppositeDirection = (player.isFacingRight && GetHorizontalAxis() < 0f) ||
                                               (!player.isFacingRight && GetHorizontalAxis() > 0f);

            if (isPressingOppositeDirection && GetVerticalAxis() == 0) {
                _animator.SetFloat(IsTowardsUp, 0f);
                _animator.SetBool(IsHoldingJumpOnClimb, true);
                player.flipManager.Flip(!player.isFacingRight);
            } else {
                _animator.SetBool(IsHoldingJumpOnClimb, false);
                player.flipManager.Flip(player.isFacingRight);
            }

            if (GetJumpKeyDown() && isPressingOppositeDirection) {
                StopClimbing();
                _animator.SetBool(IsHoldingJumpOnClimb, false);
                _animator.SetBool(IsJumping, true);
                player.velocity = new Vector2((player.isFacingRight ? -1f : 1f) * climbSpeed, player.jumpTakeOffSpeed);
                player.StartJump();
                player.flipManager.Flip(!player.isFacingRight);
                return;
            }

            // In case there is a ledge or the player wants to stop climbing
            if ((AnyStopClimbKeyIsPressed() || !ledgeCheck.isNearWall || ledgeCheck.isNearLedge) &&
                player.movementState != PlayerMovementState.Hold) {
                StopClimbing();
            }
        }

        private bool AnyStopClimbKeyIsPressed() {
            return GetJumpKeyDown();
        }

        private void StopClimbing() {
            if (player.movementState != PlayerMovementState.Hold) {
                // Rise a bit the player vertical position to help ledge detection
                player.AddPosition(0, 0.05f);
                _isClimbing = false;
                _canAttach = true;
                player.FreezeHorizontalPosition(false);
                SetClimbingState(false);
                player.UnlockMovementState();
                _animator.SetBool(IsClimbing, false);
                _animator.SetBool(IsHoldingJumpOnClimb, false);
            }
        }

        private void OnTriggerEnter2D(Collider2D collision) {
            if (collision.CompareTag(wallTag)) {
                _canAttach = true;
            }
        }

        private void OnTriggerExit2D(Collider2D collision) {
            if (collision.CompareTag(wallTag)) {
                _canAttach = false;
            }
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

            player.rb.velocity = Vector2.zero;
            player.velocity = Vector2.zero;
        }
    }
}