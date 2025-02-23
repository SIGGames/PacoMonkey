using System;
using System.Collections;
using System.ComponentModel;
using Configuration;
using UnityEditor;
using UnityEngine;

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
        private Coroutine _incrementCoroutine;

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

        public void IncrementLives(float lives = 1) {
            CurrentLives += lives;
        }

        public void IncrementLivesToMaxSlow(float timeBetweenIncrements = 1f, float healthIncrement = 0.5f) {
            StopIncrementingLives();
            _incrementCoroutine = StartCoroutine(IncrementLivesSlowRoutine(timeBetweenIncrements, healthIncrement));
        }

        public void StopIncrementingLives() {
            if (_incrementCoroutine != null) {
                StopCoroutine(_incrementCoroutine);
                _incrementCoroutine = null;
            }
        }

        private IEnumerator IncrementLivesSlowRoutine(float timeBetweenIncrements, float healthIncrement = 0.5f) {
            while (CurrentLives < MaxLives) {
                CurrentLives = Mathf.Clamp(CurrentLives + healthIncrement, 0, MaxLives);
                yield return new WaitForSeconds(timeBetweenIncrements);
            }
        }

        public void DecrementLives(float lives = 1) {
            CurrentLives -= lives;
        }

        public void Die() {
            if (_isConfigNotNull && _config.isGodMode) {
                return;
            }

            DecrementLives(CurrentLives);
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