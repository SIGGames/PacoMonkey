using System;
using System.Collections;
using System.ComponentModel;
using UnityEditor;
using UnityEngine;
using static Configuration.GameConfig;

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
        private Coroutine _incrementCoroutine;
        private float _startingMaxLives;

        private void Awake() {
            CurrentLives = startingLives;
            _startingMaxLives = maxLives;
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
            #pragma warning disable CS0162 // Unreachable code detected
            if (IsGodMode) {
                return;
            }
            #pragma warning restore CS0162 // Unreachable code detected

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

        public void MultiplyLives(float multiplier) {
            // In case of a half heart, it will be rounded up
            CurrentLives = Mathf.Ceil(startingLives * multiplier);
            MaxLives = Mathf.Ceil(_startingMaxLives * multiplier);
        }
    }
}