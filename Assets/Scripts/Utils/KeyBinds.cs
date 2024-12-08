using UnityEngine;
using PlayerInput;

namespace Utils {
    public static class KeyBinds {
        private static PlayerInputActions Actions => PlayerInputManager.Instance.InputActions;

        public static bool GetJumpKeyDown() {
            return Actions.PlayerControls.Jump.triggered;
        }

        public static bool GetJumpKeyUp() {
            // Simulation of "KeyUp" using canceled in Unity Input System.
            return Actions.PlayerControls.Jump.phase == UnityEngine.InputSystem.InputActionPhase.Canceled;
        }

        public static Vector2 GetMoveAxis() {
            return Actions.PlayerControls.Move.ReadValue<Vector2>();
        }

        public static bool GetCrouchKey() {
            return Actions.PlayerControls.Crouch.IsPressed();
        }

        public static bool GetWalkKey() {
            return Actions.PlayerControls.Walk.IsPressed();
        }

        public static bool GetClimbKey() {
            return Actions.PlayerControls.Climb.IsPressed();
        }

        public static bool GetUpKey() {
            return Actions.PlayerControls.Up.IsPressed();
        }
    }
}