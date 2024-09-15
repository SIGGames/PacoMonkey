using Configuration;
using Platformer.Gameplay;
using UnityEngine;
using static Platformer.Core.Simulation;

namespace Mechanics.Health {
    public class Health : MonoBehaviour {
        private GlobalConfiguration _config;
        public bool IsAlive => currentHp > 0 && currentLives > 0; // TODO: Mabe is alive var should be removed

        [SerializeField] private int currentHp;
        [SerializeField] private int currentLives;

        public void IncrementHp(int hp = GlobalConfiguration.DefaultHpIncrement) {
            currentHp = Mathf.Clamp(currentHp + hp, 0, _config.maxHp);
        }

        public void DecrementHp(int hp = GlobalConfiguration.DefaultHpDecrement) {
            if (_config.isGodMode) return;

            currentHp = Mathf.Clamp(currentHp - hp, 0, _config.maxHp);

            if (currentHp == 0) {
                HandleLifeLoss();
            }
        }

        public void IncrementLive() {
            currentLives = Mathf.Clamp(currentLives + 1, 0, _config.maxLives);
        }

        public void DecrementLive() {
            if (_config.isGodMode) return;
            HandleLifeLoss();
        }

        private void HandleLifeLoss() {
            currentLives = Mathf.Clamp(currentLives - 1, 0, _config.maxLives);

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
            currentHp = _config.defaultHp;
        }

        private void ResetLives() {
            currentLives = _config.defaultLives;
        }
    }
}