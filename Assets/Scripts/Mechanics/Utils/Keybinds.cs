using UnityEngine;

namespace Mechanics.Utils
{
    public static class Keybinds
    {
        public static bool GetJumpKey()
        {
            return Input.GetButtonDown("Jump");
        }

        public static bool GetJumpKeyHeld()
        {
            return Input.GetButton("Jump");
        }

        public static bool GetJumpKeyUp()
        {
            return Input.GetButtonUp("Jump");
        }

        public static bool GetCrouchKey()
        {
            return Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);
        }

        public static bool GetWalkKey()
        {
            return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        }

        public static bool GetClimbKey()
        {
            return Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
        }

        public static float GetHorizontalAxis()
        {
            return Input.GetAxis("Horizontal");
        }

        public static float GetVerticalAxis()
        {
            return Input.GetAxis("Vertical");
        }

        public static bool AnyKeyPressed()
        {
            return Input.anyKey;
        }
    }
}