using Enums;
using Model;
using Platformer.Core;
using Platformer.Gameplay;

namespace Gameplay {
    public class PlayerSpawn : Simulation.Event<PlayerSpawn> {
        PlatformerModel model = Simulation.GetModel<PlatformerModel>();

        public override void Execute() {
            var player = model.player;
            player.collider2d.enabled = true;
            player.controlEnabled = false;
            if (player.audioSource && player.respawnAudio) {
                player.audioSource.PlayOneShot(player.respawnAudio);
            }

            player.Teleport(model.spawnPoint.transform.position);
            player.jumpState = JumpState.Grounded;
            player.animator.SetBool("dead", false);

            model.virtualCamera.m_Follow = player.transform;
            model.virtualCamera.m_LookAt = player.transform;

            Simulation.Schedule<EnablePlayerInput>(2f);
        }
    }
}