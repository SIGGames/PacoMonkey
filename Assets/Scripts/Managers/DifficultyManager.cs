using Controllers;
using Enums;
using UnityEngine;
using static Utils.PlayerPrefsKeys;

namespace Managers {
    public class DifficultyManager : MonoBehaviour {
        public static DifficultyManager Instance { get; private set; }
        public Difficulty currentDifficulty = Difficulty.Normal;

        [Header("Multiplier values relative to Normal difficulty")]
        [SerializeField, Range(0.1f, 1f)]
        private float easyMultiplier = 0.7f;

        [SerializeField, Range(1f, 2f)]
        private float hardMultiplier = 1.2f;

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

                enemy.SetDifficultyMultiplier(GetDifficultyMultiplier());
            }

            SetPlayerDifficultyMultiplier();

            // TODO: Update missions timers
        }

        public void SetPlayerDifficultyMultiplier() {
            if (CharacterManager.Instance == null || CharacterManager.Instance.currentPlayerController == null) {
                return;
            }

            CharacterManager.Instance.currentPlayerController.SetDifficultyMultiplier(GetDifficultyMultiplier());
        }

        private float GetDifficultyMultiplier() {
            return currentDifficulty switch {
                Difficulty.Easy => easyMultiplier,
                Difficulty.Hard => hardMultiplier,
                _ => 1f
            };
        }
    }
}