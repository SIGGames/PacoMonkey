using System;
using Unity.Collections;
using UnityEngine;
using UnityEditor;
using UnityEngine.Serialization;

namespace Health {
    public class Health : MonoBehaviour {
        // Constants
        private const float MaxInspectorHealth = 3000;

        [SerializeField, HalfStepSlider(0, MaxInspectorHealth)]
        private float startingHealth = 50;

        [SerializeField, HalfStepSlider(0, MaxInspectorHealth)]
        public float maxHealth = 100;

        [SerializeField, HalfStepSlider(0, MaxInspectorHealth)]
        private float currentHealth;

        public event Action OnHealthChanged;
        public bool IsAlive => currentHealth > 0;

        private void Awake() {
            CurrentHealth = startingHealth;
        }

        public float CurrentHealth {
            get => currentHealth;
            set {
                currentHealth = Mathf.Clamp(Mathf.Round(value * 2) / 2, 0, maxHealth);
                OnHealthChanged?.Invoke();
            }
        }

        public void IncrementHealth(float amount) {
            CurrentHealth += amount;
        }

        public void DecrementHealth(float amount) {
            CurrentHealth -= amount;
        }

        public void ResetHealth() {
            CurrentHealth = startingHealth;
        }

        public float GetMaxHealth() {
            return maxHealth;
        }
    }
}