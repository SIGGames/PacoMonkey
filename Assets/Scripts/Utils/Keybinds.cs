using UnityEngine;

namespace Mechanics.Utils {
    // TODO: Add keybinds by unity config
    public static class Keybinds {
        public static bool GetJumpKey() {
            return Input.GetKeyDown(KeyCode.Space);
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