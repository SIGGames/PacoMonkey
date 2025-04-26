using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;
using static Utils.PlayerPrefsKeys;

namespace Managers {
    public class LevelManager : MonoBehaviour {
        public static LevelManager Instance { get; private set; }

        [SerializeField] private List<LevelConfig> levels = new();

        [SerializeField] private Level defaultLevel;

        private void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            FindDuplicates();
        }

        private void Start() {
            LoadLevel();
        }

        public void SetLevel(Level level) {
            LevelConfig levelConfig = levels.Find(l => l.level == level);
            if (levelConfig == null) {
                return;
            }

            foreach (LevelConfig config in levels) {
                config.levelGameObject.SetActive(config == levelConfig);
            }

            SaveLevel(level);
        }

        public void ResetLevel() {
            SetLevel(defaultLevel);
        }

        private static void SaveLevel(Level level) {
            PlayerPrefs.SetInt(LevelKey, (int)level);
            PlayerPrefs.Save();
        }

        private void LoadLevel() {
            Level level = (Level)PlayerPrefs.GetInt(LevelKey, (int)defaultLevel);
            SetLevel(level);
        }

        private void FindDuplicates() {
            HashSet<Level> uniqueLevels = new();
            HashSet<GameObject> uniqueGameObjects = new();

            foreach (LevelConfig levelConfig in levels) {
                if (!uniqueLevels.Add(levelConfig.level)) {
                    Debug.LogError($"Duplicate level enum found: {levelConfig.level}");
                    enabled = false;
                }
                if (!uniqueGameObjects.Add(levelConfig.levelGameObject)) {
                    Debug.LogError($"Duplicate GameObject found for level: {levelConfig.level}");
                    enabled = false;
                }
            }
        }
    }

    [Serializable]
    public class LevelConfig {
        public Level level;
        public GameObject levelGameObject;
    }
}