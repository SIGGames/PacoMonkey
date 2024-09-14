using Configuration;
using Platformer.Gameplay;
using UnityEngine;
using static Platformer.Core.Simulation;

namespace Mechanics.Health {
    public class Health : MonoBehaviour {
        public bool IsAlive => currentHp > 0 && currentLives > 0; // TODO: Mabe is alive var should be removed

        [SerializeField] private int currentHp;
        [SerializeField] private int currentLives;

        public void IncrementHp(int hp = GlobalConfiguration.DefaultHpIncrement) {
            currentHp = Mathf.Clamp(currentHp + hp, 0, GlobalConfiguration.MaxHp);
        }

        public void DecrementHp(int hp = GlobalConfiguration.DefaultHpDecrement) {
            if (GlobalConfiguration.IsGodMode) return;

            currentHp = Mathf.Clamp(currentHp - hp, 0, GlobalConfiguration.MaxHp);

            if (currentHp == 0) {
                HandleLifeLoss();
            }
        }

        public void IncrementLive() {
            currentLives = Mathf.Clamp(currentLives + 1, 0, GlobalConfiguration.MaxLives);
        }

        public void DecrementLive() {
            if (GlobalConfiguration.IsGodMode) return;
            HandleLifeLoss();
        }

        private void HandleLifeLoss() {
            currentLives = Mathf.Clamp(currentLives - 1, 0, GlobalConfiguration.MaxLives);

            ResetHp();

            if (currentLives > 0) return;
            Schedule<PlayerDeath>();
        }

        public void Die() {
            if (GlobalConfiguration.IsGodMode) return;
            Schedule<PlayerDeath>();
        }

        private void Awake() {
            ResetHealth();
        }

        public void ResetHealth() {
            ResetHp();
            ResetLives();
        }

        private void ResetHp() {
            currentHp = GlobalConfiguration.DefaultHp;
        }

        private void ResetLives() {
            currentLives = GlobalConfiguration.DefaultLives;
        }
    }
}