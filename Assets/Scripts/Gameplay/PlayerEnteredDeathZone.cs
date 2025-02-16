using Managers;
using Platformer.Core;

namespace Gameplay {
    public class PlayerEnteredDeathZone : Simulation.Event<PlayerEnteredDeathZone> {

        public override void Execute() {
            CharacterManager.Instance.currentPlayerController.lives.Die();
        }
    }
}