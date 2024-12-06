using UnityEngine;

namespace Utils {
    public static class KeyBinds {
        public static bool GetJumpKey() {
            return Input.GetButtonDown("Jump");
        }

        public static bool GetCrouchKey() {
            return Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);
        }

        public static bool GetIdleKey() {
            return Input.GetKey(KeyCode.S);
        }

        public static bool GetWalkKey() {
            return Input.GetKey(KeyCode.LeftShift);
        }

        public static bool GetRunKey() {
            return Input.GetKeyDown(KeyCode.W);
        }

        public static bool GetClimbKey() {
            return Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
        }

        public static bool GetUpKey() {
            return Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
        }
    }
}