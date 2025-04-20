using Controllers;
using Platformer.Core;
using System;
using Managers;

namespace Gameplay {
    public class EnemyDeath : Simulation.Event<EnemyDeath> {
        public EnemyController enemy;
        public static event Action<EnemyController> OnEnemyDeath;

        public override void Execute() {
            if (enemy.audioSource && enemy.deathAudios.Count > 0) {
                enemy.audioSource.PlayOneShot(AudioManager.GetRandomAudioClip(enemy.deathAudios));
            }

            OnEnemyDeath?.Invoke(enemy);
        }
    }
}