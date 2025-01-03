﻿using Controllers;
using Enums;
using Gameplay;
using UnityEngine;
using static PlayerInput.KeyBinds;

namespace Mechanics.Movement {
    [RequireComponent(typeof(PlayerController))]
    public class Hold : MonoBehaviour {
        [SerializeField] private LedgeDetection ledgeCheck;
        [SerializeField] private PlayerController player;

        [SerializeField] private Vector2 holdPositionOffset = new(0.1f, 0.1f);
        [SerializeField] private Vector2 playerMoveOnClimb = new(0f, 0.5f);

        [SerializeField] private bool isHolding;

        private Animator _animator;
        private static readonly int IsHolding = Animator.StringToHash("isHolding");

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
            if (ledgeCheck.isNearLedge && !isHolding &&
                PlayerMovementStateMethods.IsPlayerOnAir(player.movementState)) {
                StartHold();
            }

            if (isHolding && GetJumpKeyDown() && player.movementState == PlayerMovementState.Hold) {
                ClimbLedge();
            } else if (isHolding && GetJumpKeyDown()) {
                EndHold();
            }
        }

        public void StartHold() {
            _animator.SetBool(IsHolding, true);
            isHolding = true;

            float xOffset = player.isFacingRight ? holdPositionOffset.x : -holdPositionOffset.x;
            player.AddPosition(xOffset, holdPositionOffset.y);

            player.UnlockMovementState();
            player.SetMovementState(PlayerMovementState.Hold, 2);
            player.FreezePosition();
        }

        private void ClimbLedge() {
            player.UnlockMovementState();
            player.SetMovementState(PlayerMovementState.Climb, 2);

            Vector3 ledgeCheckPosition = ledgeCheck.transform.position;
            float xOffset = player.isFacingRight ? playerMoveOnClimb.x : -playerMoveOnClimb.x;
            player.SetPosition(ledgeCheckPosition.x + xOffset, ledgeCheckPosition.y + playerMoveOnClimb.y);

            EndHold();
        }

        private void EndHold() {
            isHolding = false;
            player.SetMovementState(PlayerMovementState.Idle);
            player.UnlockMovementState();
            player.FreezePosition(false);
            _animator.SetBool(IsHolding, false);
        }
    }
}