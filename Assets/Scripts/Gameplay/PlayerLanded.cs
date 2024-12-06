using Controllers;
using Mechanics.Movement;
using Platformer.Core;

namespace Platformer.Gameplay {
    public class PlayerLanded : Simulation.Event<PlayerLanded> {
        public PlayerController player;

        public override void Execute() {
        }
    }
}