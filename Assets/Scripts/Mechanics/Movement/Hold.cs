using Controllers;
using Enums;
using Gameplay;
using UnityEngine;
using Utils;

namespace Mechanics.Movement {
    public class Hold : MonoBehaviour {
        [SerializeField] private LedgeDetection ledgeCheck;
        [SerializeField] private PlayerController player;

        [SerializeField] private Vector2 playerMoveOnClimb = new(0.5f, 1f);

        [SerializeField] private bool isHolding;

        private Animator _animator;

        private void Awake() {
            if (player == null) {
                player = GetComponent<PlayerController>();
            }

            if (ledgeCheck == null) {
                ledgeCheck = GetComponent<LedgeDetection>();
            }

            _animator = GetComponent<Animator>();
        }

        private void Update() {
            if (ledgeCheck == null) {
                return;
            }

            if (ledgeCheck.isNearLedge && !isHolding) {
                StartHold();
            }

            if (isHolding) {
                HandlePlayerActions();
            }
        }

        private void StartHold() {
            // TODO: Freeze player movement and add animation
            isHolding = true;
            player.SetMovementState(PlayerMovementState.Hold, true);
            player.velocity = Vector2.zero;
            // player.controlEnabled = false;
        }

        private void HandlePlayerActions() {
            if (ledgeCheck.isNearLedge) {
                if (KeyBinds.GetUpKey()) {
                    ClimbLedge();
                }
            } else {
                EndHold();
            }
        }


        private void ClimbLedge() {
            player.SetMovementState(PlayerMovementState.Climb);
            Vector3 ledgeCheckPosition = ledgeCheck.transform.position;
            player.transform.position = new Vector3(ledgeCheckPosition.x + playerMoveOnClimb.x,
                ledgeCheckPosition.y + playerMoveOnClimb.y, player.transform.position.z);
            EndHold();
        }

        private void EndHold() {
            isHolding = false;
            player.SetMovementState(PlayerMovementState.Idle);
            player.UnlockMovementState();
            player.controlEnabled = true;
        }
    }
}