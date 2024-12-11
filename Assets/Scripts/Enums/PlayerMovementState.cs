namespace Enums {
    public enum PlayerMovementState {
        Idle,
        Walk,
        Run,
        Crouch,
        Jump,
        DoubleJump,
        Climb,
        Hold,
        Up
    }

    public static class PlayerMovementStateMethods {
        public static bool IsPlayerMoving(PlayerMovementState state) {
            return state == PlayerMovementState.Walk || state == PlayerMovementState.Run ||
                   state == PlayerMovementState.Crouch || state == PlayerMovementState.Jump;
        }

        public static bool IsPlayerOnAir(PlayerMovementState state) {
            return state == PlayerMovementState.Jump || state == PlayerMovementState.DoubleJump;
        }
    }
}