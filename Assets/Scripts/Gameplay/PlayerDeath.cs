using Model;
using Platformer.Core;
using Platformer.Gameplay;

namespace Gameplay {
    public class PlayerDeath : Simulation.Event<PlayerDeath> {
        readonly PlatformerModel _model = Simulation.GetModel<PlatformerModel>();

        public override void Execute() {
            var player = _model.player;
            player.lives.ResetLives();
            _model.virtualCamera.m_Follow = null;
            _model.virtualCamera.m_LookAt = null;
            // player.collider.enabled = false;
            player.controlEnabled = false;

            if (player.audioSource && player.ouchAudio) {
                player.audioSource.PlayOneShot(player.ouchAudio);
            }

            // TODO: Enable this when death animation is implemented
            // player.animator.SetTrigger("hurt");
            // player.animator.SetBool("dead", true);
            Simulation.Schedule<PlayerSpawn>(2);
        }
    }
}