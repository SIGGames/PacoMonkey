using Controllers;
using Enums;
using UnityEngine;
using static Utils.PlayerPrefsKeys;

namespace Managers {
    public class DifficultyManager : MonoBehaviour {
        public static DifficultyManager Instance { get; private set; }
        public Difficulty currentDifficulty = Difficulty.Normal;

        [Header("Multiplier values relative to Normal difficulty")]
        [Header("Player Multipliers")]
        [SerializeField, Range(0.1f, 1f)]
        private float easyPlayerMultiplier = 0.7f;

        [SerializeField, Range(1f, 2f)]
        private float hardPlayerMultiplier = 1.2f;

        [Header("Enemies Multipliers")]
        [SerializeField, Range(0.1f, 1f)]
        private float easyEnemyMultiplier = 0.7f;

        [SerializeField, Range(1f, 2f)]
        private float hardEnemyMultiplier = 1.2f;

        private void Awake() {
            if (Instance == null) {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            } else {
                Destroy(gameObject);
            }
        }

        private void Start() {
            LoadDifficulty();
        }

        private void LoadDifficulty() {
            currentDifficulty = (Difficulty)PlayerPrefs.GetInt(DifficultyKey, (int)currentDifficulty);
        }

        public void SetDifficulty(string difficulty) {
            Difficulty newDifficulty = difficulty.ToLower() switch {
                "easy" => Difficulty.Easy,
                "hard" => Difficulty.Hard,
                _ => Difficulty.Normal
            };

            SetDifficulty(newDifficulty);
        }

        private void SetDifficulty(Difficulty newDifficulty) {
            currentDifficulty = newDifficulty;

            PlayerPrefs.SetInt(DifficultyKey, (int)currentDifficulty);
            PlayerPrefs.Save();

            UpdateMultiplierValues();
        }

        private void UpdateMultiplierValues() {
            EnemyController[] enemies = FindObjectsOfType<EnemyController>(true);

            foreach (EnemyController enemy in enemies) {
                if (enemy == null) {
                    continue;
                }

                enemy.SetDifficultyMultiplier(GetEnemyDifficultyMultiplier(currentDifficulty));
            }

            SetPlayerDifficultyMultiplier();
        }

        public void SetPlayerDifficultyMultiplier() {
            if (CharacterManager.Instance == null || CharacterManager.Instance.currentPlayerController == null) {
                return;
            }

            CharacterManager.Instance.currentPlayerController.SetDifficultyMultiplier(GetPlayerDifficultyMultiplier(currentDifficulty));
        }

        public float GetPlayerDifficultyMultiplier(Difficulty difficulty) {
            return difficulty switch {
                Difficulty.Easy => easyPlayerMultiplier,
                Difficulty.Hard => hardPlayerMultiplier,
                _ => 1f
            };
        }

        private float GetEnemyDifficultyMultiplier(Difficulty difficulty) {
            return difficulty switch {
                Difficulty.Easy => easyEnemyMultiplier,
                Difficulty.Hard => hardEnemyMultiplier,
                _ => 1f
            };
        }
    }
}