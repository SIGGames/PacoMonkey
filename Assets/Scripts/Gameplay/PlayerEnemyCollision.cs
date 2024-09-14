using Mechanics;
using Mechanics.Health;
using Model;
using Platformer.Core;
using static Platformer.Core.Simulation;

namespace Platformer.Gameplay {
    /// <summary>
    /// S'executa quan el jugador col·lisiona amb un enemic.
    /// </summary>
    public class PlayerEnemyCollision : Simulation.Event<PlayerEnemyCollision> {
        public EnemyController enemy;
        public PlayerController player;

        PlatformerModel model = Simulation.GetModel<PlatformerModel>();

        public override void Execute() {
            var willHurtEnemy = player.Bounds.center.y >= enemy.Bounds.max.y;

            var enemyHealth = enemy.GetComponent<Health>();

            if (willHurtEnemy && enemyHealth != null) {
                enemyHealth.Die(); // TODO: Ensure this is correct or playerHealth should be checked

                if (!enemyHealth.IsAlive) {
                    Schedule<EnemyDeath>().enemy = enemy;
                    player.Bounce(2);
                } else {
                    player.Bounce(7);
                }
            } else if (!willHurtEnemy) {
                player.health.DecrementLive();
            }
        }
    }
}