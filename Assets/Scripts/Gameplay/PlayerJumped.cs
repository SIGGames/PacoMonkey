using Controllers;
using Managers;
using Platformer.Core;

namespace Gameplay {
    public class PlayerJumped : Simulation.Event<PlayerJumped> {
        public PlayerController player;

        public override void Execute() {
            if (player.audioSource) {
                player.audioSource.PlayOneShot(AudioManager.GetRandomAudioClip(player.jumpAudios));
            }
        }
    }
}