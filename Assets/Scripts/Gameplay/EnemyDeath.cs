using Controllers;
using Platformer.Core;

namespace Gameplay {
    public class EnemyDeath : Simulation.Event<EnemyDeath> {
        public EnemyController enemy;

        public override void Execute() {
            if (enemy.audioSource && enemy.ouch) {
                enemy.audioSource.PlayOneShot(enemy.ouch);
            }
        }
    }
}