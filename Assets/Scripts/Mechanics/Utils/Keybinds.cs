using UnityEngine;

namespace Mechanics.Utils {
    // TODO: Add keybinds by unity config
    public static class Keybinds {
        public static bool GetJumpKey() {
            return Input.GetKeyDown(KeyCode.Space);
        }

        public static bool GetCrouchKey() {
            return Input.GetKeyDown(KeyCode.S);
        }

        public static bool GetIdleKey() {
            return Input.GetKey(KeyCode.S);
        }

        public static bool GetWalkKey() {
            return Input.GetKey(KeyCode.LeftShift);
        }
    }
}