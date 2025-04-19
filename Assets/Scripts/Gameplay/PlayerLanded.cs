using Controllers;
using Platformer.Core;

namespace Gameplay {
    public class PlayerLanded : Simulation.Event<PlayerLanded> {
        public PlayerController player;

        public override void Execute() {
            // Since player is grounded, we can unlock movement state
            player.UnlockMovementState();
        }
    }
}