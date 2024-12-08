using UnityEngine;

namespace PlayerInput {
    public static class KeyBinds {
        private static PlayerInputActions.PlayerControlsActions PlayerControls => PlayerInputManager.Instance.InputActions.PlayerControls;

        public static bool GetJumpKeyDown() {
            return PlayerControls.Jump.triggered;
        }

        public static bool GetJumpKeyUp() {
            // Simulation of "KeyUp" using canceled in Unity Input System.
            return PlayerControls.Jump.phase == UnityEngine.InputSystem.InputActionPhase.Canceled;
        }

        public static Vector2 GetMoveAxis() {
            return PlayerControls.Move.ReadValue<Vector2>();
        }

        public static bool GetCrouchKey() {
            return PlayerControls.Crouch.IsPressed();
        }

        public static bool GetWalkKey() {
            return PlayerControls.Walk.IsPressed();
        }

        public static bool GetClimbKey() {
            return PlayerControls.Climb.IsPressed();
        }

        public static bool GetUpKey() {
            return PlayerControls.Up.IsPressed();
        }
    }
}