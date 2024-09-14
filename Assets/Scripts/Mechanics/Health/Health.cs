using Configuration;
using Platformer.Gameplay;
using UnityEngine;
using static Platformer.Core.Simulation;

namespace Mechanics.Health {
    public class Health : MonoBehaviour {
        public bool IsAlive => currentHp > 0 && currentLives > 0;

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

        private void HandleLifeLoss() {
            currentLives = Mathf.Clamp(currentLives - 1, 0, GlobalConfiguration.MaxLives);

            if (currentLives > 0) {
                ResetHealth();
                Schedule<PlayerSpawn>();
            }
            else {
                var ev = Schedule<HealthIsZero>();
                ev.health = this;
            }
        }

        public void Die() {
            if (GlobalConfiguration.IsGodMode) return;
            while (currentHp > 0) DecrementHp();
        }

        private void Awake() {
            ResetHealth();
        }

        public void ResetHealth() {
            SetDefaultHp();
            SetDefaultLives();
        }

        private void SetDefaultHp() {
            currentHp = GlobalConfiguration.DefaultHp;
        }

        private void SetDefaultLives() {
            currentLives = GlobalConfiguration.DefaultLives;
        }
    }
}