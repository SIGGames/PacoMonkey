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
        private const float MinMaxLives = 0.5f;

        [SerializeField, HalfStepSlider(0, MaxInspectorLives)]
        private float startingLives = 3;

        [SerializeField, HalfStepSlider(0, MaxInspectorLives)]
        private float maxLives = 5;

        [SerializeField, HalfStepSlider(0, MaxInspectorLives), ReadOnly(true)]
        private float currentLives;

        public event Action OnLivesChanged;
        public bool IsAlive => currentLives > 0;
        private GlobalConfiguration _config;
        private bool _isConfigNotNull;

        private void Start() {
            _isConfigNotNull = _config != null;
        }

        private void Awake() {
            _config = GlobalConfiguration.Instance;
            CurrentLives = startingLives;
        }

        public float CurrentLives {
            get => currentLives;
            set {
                float clampedValue = Mathf.Clamp(Mathf.Round(value * 2) / 2, 0, MaxLives);
                if (Math.Abs(currentLives - clampedValue) > Mathf.Epsilon) {
                    currentLives = clampedValue;
                    OnLivesChanged?.Invoke();
                }
            }
        }

        public float MaxLives {
            get => maxLives;
            set {
                float clampedValue = Math.Max(Mathf.Clamp(value, 0, MaxInspectorLives), MinMaxLives);
                if (Math.Abs(maxLives - clampedValue) > Mathf.Epsilon) {
                    maxLives = clampedValue;
                    OnLivesChanged?.Invoke();
                }
            }
        }

        public void IncrementLive() {
            CurrentLives += 1;
        }

        public void IncrementLives(float lives) {
            CurrentLives += lives;
        }

        public void IncrementLivesToMax() {
            CurrentLives = MaxLives;
        }

        public void DecrementLive() {
            CurrentLives -= 1;
            if (!IsAlive) {
                Die();
            }
        }

        public void Die() {
            if (_isConfigNotNull && _config.isGodMode) {
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