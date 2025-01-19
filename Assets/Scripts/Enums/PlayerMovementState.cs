namespace Enums {
    public enum PlayerMovementState {
        Idle,
        Walk,
        Run,
        Crouch,
        Jump,
        DoubleJump,
        Climb,
        WallClimb,
        Hold,
        Up,
        Slide
    }

    public static class PlayerMovementStateMethods {
        public static bool IsPlayerMoving(PlayerMovementState state) {
            return state == PlayerMovementState.Walk || state == PlayerMovementState.Run ||
                   state == PlayerMovementState.Crouch || state == PlayerMovementState.Jump ||
                   state == PlayerMovementState.DoubleJump || state == PlayerMovementState.Climb ||
                   state == PlayerMovementState.WallClimb || state == PlayerMovementState.Slide;
        }

        public static bool IsPlayerOnAir(PlayerMovementState state) {
            return state == PlayerMovementState.Jump || state == PlayerMovementState.DoubleJump;
        }

        public static bool IsPlayerAbleToCrouch(PlayerMovementState state) {
            return state == PlayerMovementState.Idle || state == PlayerMovementState.Walk ||
                   state == PlayerMovementState.Run;
        }

        public static bool IsPlayerAbleToJump(PlayerMovementState state) {
            return state == PlayerMovementState.Idle || state == PlayerMovementState.Walk ||
                   state == PlayerMovementState.Run;
        }

        public static bool IsPlayerAbleToHold(PlayerMovementState state) {
            return IsPlayerOnAir(state) || state == PlayerMovementState.WallClimb;
        }
    }
}