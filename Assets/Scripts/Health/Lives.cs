using System;
using System.ComponentModel;
using Configuration;
using Platformer.Gameplay;
using UnityEditor;
using UnityEngine;
using static Platformer.Core.Simulation;

namespace Health {
    public class Lives : MonoBehaviour {
        private const float MaxInspectorLives = 10;

        [SerializeField, HalfStepSlider(0, MaxInspectorLives)]
        private float startingLives = 3;

        [SerializeField, HalfStepSlider(0, MaxInspectorLives)]
        private float maxLives = 5;

        [SerializeField, HalfStepSlider(0, MaxInspectorLives), ReadOnly(true)]
        private float currentLives;

        public event Action OnLivesChanged;
        public bool IsAlive => currentLives > 0;
        private GlobalConfiguration _config;

        private void Awake() {
            _config = GlobalConfiguration.Instance;
            CurrentLives = startingLives;
        }

        public float CurrentLives {
            get => currentLives;
            set {
                currentLives = Mathf.Clamp(Mathf.Round(value * 2) / 2, 0, MaxLives);
                OnLivesChanged?.Invoke();
            }
        }

        public float MaxLives {
            get => maxLives;
            set {
                maxLives = Mathf.Clamp(value, 0, MaxInspectorLives);
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

        public float GetMaxLives() {
            return MaxLives;
        }

        private void OnValidate() {
            maxLives = Mathf.Clamp(maxLives, 0, MaxInspectorLives);
            currentLives = Mathf.Clamp(currentLives, 0, maxLives);
            OnLivesChanged?.Invoke();
        }
    }
}