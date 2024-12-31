using System;
using Vector2 = UnityEngine.Vector2;

namespace PlayerInput {
    public static class KeyBinds {
        private static PlayerInputActions.PlayerControlsActions PlayerControls =>
            PlayerInputManager.Instance.InputActions.PlayerControls;

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

        public static float GetHorizontalAxis() {
            return Math.Sign(GetMoveAxis().x);
        }

        public static float GetVerticalAxis() {
            return Math.Sign(GetMoveAxis().y);
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

        public static bool GetMenuKey() {
            return PlayerControls.Menu.triggered;
        }

        public static bool GetConfigurationKey() {
            return PlayerControls.Configuration.triggered;
        }

        private static Vector2 GetCameraMoveAxis() {
            return PlayerControls.CameraMove.ReadValue<Vector2>();
        }

        private static float GetCameraHorizontalAxis() {
            return GetCameraMoveAxis().x;
        }

        private static float GetCameraVerticalAxis() {
            return GetCameraMoveAxis().y;
        }

        public static bool GetCameraUpKey() {
            return GetCameraVerticalAxis() > 0;
        }

        public static bool GetCameraDownKey() {
            return GetCameraVerticalAxis() < 0;
        }

        public static bool GetCameraLeftKey() {
            return GetCameraHorizontalAxis() < 0;
        }

        public static bool GetCameraRightKey() {
            return GetCameraHorizontalAxis() > 0;
        }
    }
}