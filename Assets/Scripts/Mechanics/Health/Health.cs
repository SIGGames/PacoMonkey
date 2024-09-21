using Configuration;
using Platformer.Gameplay;
using UnityEngine;
using static Platformer.Core.Simulation;

namespace Mechanics.Health {
    public class Health : MonoBehaviour {
        private GlobalConfiguration _config;
        public bool IsAlive => currentHp > 0 && currentLives > 0; // TODO: Maybe is alive var should be removed
        // TODO: This class must be generic, so it can be used for any entity that has health

        [SerializeField] private int currentHp;
        [SerializeField] private int currentLives;

        public void IncrementHp(int hp = GlobalConfiguration.HealthConfig.DefaultHpIncrement) {
            currentHp = Mathf.Clamp(currentHp + hp, 0, _config.playerHealthSettings.maxHp);
        }

        public void DecrementHp(int hp = GlobalConfiguration.HealthConfig.DefaultHpDecrement) {
            if (_config.isGodMode) return;

            currentHp = Mathf.Clamp(currentHp - hp, 0, _config.playerHealthSettings.maxHp);

            if (currentHp == 0) {
                HandleLifeLoss();
            }
        }

        public void IncrementLive() {
            currentLives = Mathf.Clamp(currentLives + 1, 0, _config.playerHealthSettings.maxLives);
        }

        public void DecrementLive() {
            if (_config.isGodMode) return;
            HandleLifeLoss();
        }

        private void HandleLifeLoss() {
            currentLives = Mathf.Clamp(currentLives - 1, 0, _config.playerHealthSettings.maxLives);

            ResetHp();

            if (currentLives > 0) return;
            Schedule<PlayerDeath>();
        }

        public void Die() {
            if (_config.isGodMode) return;
            Schedule<PlayerDeath>();
        }

        private void Awake() {
            _config = GlobalConfiguration.Instance;
            ResetHealth();
        }

        public void ResetHealth() {
            ResetHp();
            ResetLives();
        }

        private void ResetHp() {
            currentHp = _config.playerHealthSettings.defaultHp;
        }

        private void ResetLives() {
            currentLives = _config.playerHealthSettings.defaultLives;
        }
    }
}
