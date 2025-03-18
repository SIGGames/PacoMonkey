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

        public void SetDifficulty(Difficulty newDifficulty) {
            if (currentDifficulty != newDifficulty) {
                currentDifficulty = newDifficulty;
                PlayerPrefs.SetInt(DifficultyKey, (int)currentDifficulty);
                PlayerPrefs.Save();
            }
        }
    }
}