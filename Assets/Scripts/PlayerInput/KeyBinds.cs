using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PlayerInput {
    public static class KeyBinds {
        private static PlayerInputActions.PlayerControlsActions PlayerControls =>
            PlayerInputManager.Instance.InputActions.PlayerControls;

        public static bool GetJumpKeyDown() {
            return PlayerControls.Jump.triggered;
        }

        public static bool GetJumpKeyUp() {
            // Simulation of "KeyUp" using canceled in Unity Input System.
            return PlayerControls.Jump.phase == InputActionPhase.Canceled;
        }

        public static bool GetJumpKeyHeld() {
            return PlayerControls.Jump.IsPressed();
        }

        public static Vector2 GetMoveAxis() {
            return PlayerControls.Move.ReadValue<Vector2>();
        }

        public static float GetHorizontalAxis() {
            float horizontal = GetMoveAxis().x;
            if (Mathf.Abs(horizontal) < 0.2f) {
                return 0f;
            }

            return Math.Sign(horizontal);
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

        public static bool GetUpKeyDown() {
            return PlayerControls.Up.triggered;
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

        public static bool GetMeleeKey() {
            return PlayerControls.Melee.triggered;
        }

        public static bool GetRangeKey() {
            return PlayerControls.Range.triggered;
        }

        public static bool GetParryKey() {
            return PlayerControls.Parry.triggered;
        }

        public static bool GetInteractKey() {
            return PlayerControls.Interact.triggered;
        }

        public static bool GetQuestKey() {
            return PlayerControls.Quest.triggered;
        }
    }
}