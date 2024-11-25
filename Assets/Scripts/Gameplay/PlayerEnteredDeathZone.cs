using Mechanics;
using Model;
using Platformer.Core;

namespace Platformer.Gameplay {
    /// <summary>
    /// Fired when a player enters a trigger with a DeathZone component.
    /// </summary>
    /// <typeparam name="PlayerEnteredDeathZone"></typeparam>
    public class PlayerEnteredDeathZone : Simulation.Event<PlayerEnteredDeathZone> {
        public DeathZone deathzone;

        PlatformerModel model = Simulation.GetModel<PlatformerModel>();

        public override void Execute() {
            model.player.lives.Die();
        }
    }
}