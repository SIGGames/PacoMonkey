using Controllers;
using Platformer.Core;

namespace Gameplay {
    public class PlayerJumped : Simulation.Event<PlayerJumped> {
        public PlayerController player;

        public override void Execute() {
            if (player.audioSource && player.jumpAudio) {
                player.audioSource.PlayOneShot(player.jumpAudio);
            }
        }
    }
}