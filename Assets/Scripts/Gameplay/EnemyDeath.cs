using Controllers;
using Mechanics;
using Platformer.Core;

namespace Platformer.Gameplay {
    /// <summary>
    /// Fired when the health component on an enemy has a hitpoint value of  0.
    /// </summary>
    /// <typeparam name="EnemyDeath"></typeparam>
    public class EnemyDeath : Simulation.Event<EnemyDeath> {
        public EnemyController enemy;

        public override void Execute() {
            enemy.col.enabled = false;
            if (enemy.audioSource && enemy.ouch)
                enemy.audioSource.PlayOneShot(enemy.ouch);
        }
    }
}