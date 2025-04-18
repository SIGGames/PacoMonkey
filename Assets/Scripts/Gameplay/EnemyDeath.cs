using Controllers;
using Platformer.Core;
using System;

namespace Gameplay {
    public class EnemyDeath : Simulation.Event<EnemyDeath> {
        public EnemyController enemy;
        public static event Action<EnemyController> OnEnemyDeath;

        public override void Execute() {
            if (enemy.audioSource && enemy.ouch) {
                enemy.audioSource.PlayOneShot(enemy.ouch);
            }

            OnEnemyDeath?.Invoke(enemy);
        }
    }
}