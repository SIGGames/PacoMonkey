using Controllers;
using Managers;
using Platformer.Core;

namespace Gameplay {
    public class PlayerLanded : Simulation.Event<PlayerLanded> {
        public PlayerController player;

        public override void Execute() {
            // Since player is grounded, we can unlock movement state
            player.UnlockMovementState();

            // Play landing audio
            if (player.audioSource) {
                player.audioSource.PlayOneShot(AudioManager.GetRandomAudioClip(player.landAudios), player.landAudioVolume);
            }
        }
    }
}