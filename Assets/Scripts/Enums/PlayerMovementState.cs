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
            return state is PlayerMovementState.Walk or PlayerMovementState.Run or PlayerMovementState.Crouch
                or PlayerMovementState.Jump or PlayerMovementState.DoubleJump or PlayerMovementState.Climb
                or PlayerMovementState.WallClimb or PlayerMovementState.Slide;
        }

        public static bool IsPlayerOnAir(PlayerMovementState state) {
            return state is PlayerMovementState.Jump or PlayerMovementState.DoubleJump;
        }

        public static bool IsPlayerAbleToCrouch(PlayerMovementState state) {
            return state is PlayerMovementState.Idle or PlayerMovementState.Walk or PlayerMovementState.Run;
        }

        public static bool IsPlayerAbleToJump(PlayerMovementState state) {
            return state is PlayerMovementState.Idle or PlayerMovementState.Walk or PlayerMovementState.Run;
        }

        public static bool PlayerCanDieNotGrounded(PlayerMovementState state) {
            return state is PlayerMovementState.Climb or PlayerMovementState.WallClimb;
        }

        public static bool CanNotMoveWhileHurt(PlayerMovementState state) {
            return state is PlayerMovementState.Climb or PlayerMovementState.WallClimb or PlayerMovementState.Hold;
        }
    }
}