using Model;
using Platformer.Core;

namespace UI.Tokens {
    public class PlayerTokenCollision : Simulation.Event<PlayerTokenCollision> {
        public TokenInstance token;

        private readonly PlatformerModel _model = Simulation.GetModel<PlatformerModel>();

        public override void Execute() {
            _model.player.audioSource.PlayOneShot(token.tokenCollectAudio);
        }
    }
}