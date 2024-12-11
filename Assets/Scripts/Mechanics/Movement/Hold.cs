using Controllers;
using Enums;
using Gameplay;
using static PlayerInput.KeyBinds;
using UnityEngine;

namespace Mechanics.Movement {
    [RequireComponent(typeof(PlayerController))]
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

            if (ledgeCheck == null || player == null) {
                Debug.LogError("Hold script requires a LedgeDetection and PlayerController component");
                enabled = false;
                return;
            }

            _animator = GetComponent<Animator>();
        }

        private void Update() {
            if (ledgeCheck.isNearLedge && !isHolding && PlayerMovementStateMethods.IsPlayerOnAir(player.movementState)) {
                StartHold();
            }

            if (isHolding && GetUpKey() && player.movementState == PlayerMovementState.Hold) {
                ClimbLedge();
            } else if (isHolding && GetUpKey()) {
                EndHold();
            }
        }

        private void StartHold() {
            // TODO: Add animation
            isHolding = true;
            player.UnlockMovementState();
            player.SetMovementState(PlayerMovementState.Hold, true);
            player.FreezePosition();
        }

        private void ClimbLedge() {
            player.UnlockMovementState();
            player.SetMovementState(PlayerMovementState.Climb, true);
            Vector3 ledgeCheckPosition = ledgeCheck.transform.position;
            player.transform.position = new Vector3(ledgeCheckPosition.x + playerMoveOnClimb.x,
                ledgeCheckPosition.y + playerMoveOnClimb.y, player.transform.position.z);
            EndHold();
        }

        private void EndHold() {
            isHolding = false;
            player.SetMovementState(PlayerMovementState.Idle);
            player.UnlockMovementState();
            player.FreezePosition(false);
        }
    }
}