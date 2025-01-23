namespace Enums {
    public enum FightState {
        Idle,
        Melee,
        Ranged,
        FollowThrough,
        Block
    }

    public static class FightStateMethods {
        public static bool IsPlayerAttacking(FightState state) {
            return state is FightState.Melee or FightState.Ranged;
        }
    }
}