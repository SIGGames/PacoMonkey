using Model;
using Platformer.Core;
using UnityEngine;

namespace UI.Tokens {
    public class PlayerTokenCollision : Simulation.Event<PlayerTokenCollision> {
        public TokenInstance token;

        PlatformerModel _model = Simulation.GetModel<PlatformerModel>();

        public override void Execute() {
            AudioSource.PlayClipAtPoint(token.tokenCollectAudio, token.transform.position);
        }
    }
}