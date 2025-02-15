using Controllers;
using Mechanics.Movement;
using Model;
using Platformer.Gameplay;
using UnityEngine;
using static Platformer.Core.Simulation;

namespace Gameplay {
    public class PlayerEnemyCollision : Event<PlayerEnemyCollision> {
        public EnemyController enemy;
        public PlayerController player;

        private const int DamageToEnemy = 20;
        private const float BounceForceOnHit = 7f;
        private const float BounceForceOnKill = 3f;

        private Health.Health _enemyHealth;
        private Health.Lives _playerLives;

        public override void Execute() {
            if (_enemyHealth == null) {
                _enemyHealth = enemy.GetComponent<Health.Health>();
            }

            if (_playerLives == null) {
                _playerLives = player.GetComponent<Health.Lives>();
            }

            bool willHurtEnemy = player.Bounds.center.y >= enemy.Bounds.max.y;

            if (willHurtEnemy) {
                if (_enemyHealth != null) {
                    enemy.TakeDamage(DamageToEnemy);
                    player.BounceX(_enemyHealth.IsAlive ? BounceForceOnHit : BounceForceOnKill);
                }
                else {
                    Schedule<EnemyDeath>().enemy = enemy;
                    player.BounceX(BounceForceOnKill);
                }
            }
            else {
                _playerLives?.DecrementLive();
            }
        }
    }
}