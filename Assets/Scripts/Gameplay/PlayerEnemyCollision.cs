using Controllers;
using Mechanics.Movement;
using Model;
using Platformer.Gameplay;
using static Platformer.Core.Simulation;

namespace Gameplay {
    public class PlayerEnemyCollision : Event<PlayerEnemyCollision> {
        public EnemyController enemy;
        public PlayerController player;

        PlatformerModel model = GetModel<PlatformerModel>();

        public override void Execute() {
            var willHurtEnemy = player.Bounds.center.y >= enemy.Bounds.max.y;

            if (willHurtEnemy) {
                var enemyHealth = enemy.GetComponent<Health.Health>();
                if (enemyHealth != null) {
                    enemy.TakeDamage(20);
                    if (!enemyHealth.IsAlive) {
                        player.Bounce(2);
                    } else {
                        player.Bounce(7);
                    }
                }
                else {
                    Schedule<EnemyDeath>().enemy = enemy;
                    player.Bounce(2);
                }
            }
            else {
                var playerLives = player.GetComponent<Health.Lives>();
                if (playerLives != null) {
                    playerLives.DecrementLive();
                }
            }
        }
    }
}