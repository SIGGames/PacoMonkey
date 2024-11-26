using System;
using Configuration;
using Platformer.Gameplay;
using UnityEditor;
using UnityEngine;
using static Platformer.Core.Simulation;

namespace Health {
    public class Lives : MonoBehaviour {
        // Note that lives can be a whole number or 0.5
        [SerializeField, HalfStepSlider(0, 10)]
        private float startingLives = 3;

        [SerializeField, HalfStepSlider(0, 10)]
        private float maxLives = 5;

        private float _currentLives;
        public event Action OnLivesChanged;
        public bool IsAlive => _currentLives > 0;
        private GlobalConfiguration _config;

        private void Awake() {
            _config = GlobalConfiguration.Instance;
            CurrentLives = startingLives;
        }

        public float CurrentLives {
            get => _currentLives;
            private set {
                _currentLives = Mathf.Clamp(Mathf.Round(value * 2) / 2, 0, maxLives);
                OnLivesChanged?.Invoke();
            }
        }

        public void IncrementLive() {
            CurrentLives += 1;
        }

        public void IncrementLives(float lives) {
            CurrentLives += lives;
        }

        public void DecrementLive() {
            CurrentLives -= 1;
            if (!IsAlive) {
                Die();
            }
        }

        public void Die() {
            if (_config.isGodMode) {
                return;
            }

            Schedule<PlayerDeath>();
        }

        public void ResetLives() {
            CurrentLives = startingLives;
        }
    }
}