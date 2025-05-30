﻿using Controllers;
using Enums;
using Gameplay;
using UnityEngine;
using static PlayerInput.KeyBinds;
using static Utils.AnimatorUtils;

namespace Mechanics.Movement {
    [RequireComponent(typeof(PlayerController))]
    public class Hold : MonoBehaviour {
        [SerializeField] private LedgeDetection ledgeCheck;
        [SerializeField] private PlayerController player;

        [SerializeField] private Vector2 holdPositionOffset = new(0.1f, 0.1f);
        [SerializeField] private Vector2 playerMoveOnClimb = new(0f, 0.5f);

        [SerializeField] private bool isHolding;

        private Animator _animator;

        private void Awake() {
            if (player == null) {
                player = GetComponent<PlayerController>();
            }

            if (ledgeCheck == null) {
                ledgeCheck = GetComponent<LedgeDetection>();
            }

            if (ledgeCheck == null || player == null) {
                Debug.LogError("Hold script requires a LedgeDetection and PlayerController component");
                enabled = false;
                return;
            }

            _animator = GetComponent<Animator>();
        }

        private void Update() {
            if (!player.lives.IsAlive) {
                return;
            }

            if (ledgeCheck.isNearLedge && !isHolding && !player.IsGrounded) {
                StartHold();
            }

            if (isHolding && (GetJumpKeyDown() || GetUpKeyDown())) {
                ClimbLedge();
            }
        }

        public void StartHold() {
            _animator.SetBool(IsHolding, true);
            isHolding = true;

            // Snaps the player to the ledge corner using collider size and facing direction,
            // then applies extra offset for fine-tuning. Should prevent getting stuck inside tiles (I hope).
            Vector3 ledgePos = ledgeCheck.transform.position;
            float xOffset = player.isFacingRight ? -player.boxCollider.size.x / 2f : player.boxCollider.size.x / 2f;
            float yOffset = -player.boxCollider.size.y / 2f;

            Vector3 autoOffset = new Vector3(xOffset, yOffset, 0f);
            Vector3 manualOffset = new Vector3(player.isFacingRight ? holdPositionOffset.x : -holdPositionOffset.x,
                holdPositionOffset.y, 0f);

            Vector3 holdPosition = ledgePos + autoOffset + manualOffset;
            player.SetPosition(holdPosition);

            player.UnlockMovementState();
            player.SetMovementState(PlayerMovementState.Hold, 2);
            player.FreezePosition();
        }

        private void ClimbLedge() {
            player.UnlockMovementState();
            player.SetMovementState(PlayerMovementState.Climb, 2);

            // To ensure that map is not colliding with player during the ledge climb
            player.boxCollider.isTrigger = true;

            // Set player grounded since it will be grounded after the ledge climb, this is to prevent the player falling animation
            player.IsGrounded = true;
            _animator.SetBool(IsHolding, false);

            player.UnlockMovementState();
        }

        private void EndHold() {
            // This is called once the last ledge climb animation is played
            isHolding = false;
            player.FreezePosition(false);

            float xOffset = player.isFacingRight ? playerMoveOnClimb.x : -playerMoveOnClimb.x;
            player.AddPosition(xOffset, playerMoveOnClimb.y);

            player.boxCollider.isTrigger = false;
            player.UnlockMovementState();
        }
    }
}