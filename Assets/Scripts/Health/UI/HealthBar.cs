using System.Collections.Generic;
using Enums;
using Managers;
using NaughtyAttributes;
using UnityEngine;

namespace Health.UI {
    public class HealthBar : MonoBehaviour {
        public GameObject heartPrefab;
        public Lives playerLives;
        private readonly List<HealthHeart> _hearts = new();
        [SerializeField] private bool isDifficultyChooseUI;

        [SerializeField, ShowIf("isDifficultyChooseUI")]
        private Difficulty difficulty;

        private CharacterManager CharacterManager => CharacterManager.Instance;

        private void Start() {
            playerLives.OnLivesChanged += UpdateUI;
            InitializeHearts();
            UpdateUI();
        }

        private void OnDestroy() {
            playerLives.OnLivesChanged -= UpdateUI;
        }

        private void Update() {
            playerLives = CharacterManager.Instance.currentPlayerController.lives;
        }

        private void InitializeHearts() {
            ClearHearts();
            int maxLives = Mathf.CeilToInt(playerLives.GetMaxLives());
            for (int i = 0; i < maxLives; i++) {
                GameObject newHeart = Instantiate(heartPrefab, transform);
                newHeart.transform.SetParent(transform);
                _hearts.Add(newHeart.GetComponent<HealthHeart>());
            }
        }

        private void UpdateUI() {
            int maxLives = Mathf.CeilToInt(playerLives.GetMaxLives());

            // This is kinda broken since this rounding it's not consistent with in-game UI, but it works for now
            if (isDifficultyChooseUI) {
                maxLives = Mathf.RoundToInt(maxLives * GetDifficultyMultiplier());
            }

            if (_hearts.Count != maxLives) {
                AdjustHearts(maxLives);
            }

            // Since this is just for the UI, we want to show the max lives
            if (isDifficultyChooseUI) {
                foreach (HealthHeart heart in _hearts) {
                    heart.SetHeartImage(HeartState.Full);
                }

                return;
            }

            float currentLives = playerLives.CurrentLives;
            for (int i = 0; i < _hearts.Count; i++) {
                if (currentLives >= i + 1) {
                    _hearts[i].SetHeartImage(HeartState.Full);
                } else if (currentLives > i && currentLives < i + 1) {
                    _hearts[i].SetHeartImage(HeartState.Half);
                } else {
                    _hearts[i].SetHeartImage(HeartState.Empty);
                }
            }
        }

        private void AdjustHearts(int maxLives) {
            if (_hearts.Count < maxLives) {
                for (int i = _hearts.Count; i < maxLives; i++) {
                    GameObject newHeart = Instantiate(heartPrefab, transform);
                    newHeart.transform.SetParent(transform);
                    _hearts.Add(newHeart.GetComponent<HealthHeart>());
                }
            } else if (_hearts.Count > maxLives) {
                for (int i = _hearts.Count - 1; i >= maxLives; i--) {
                    Destroy(_hearts[i].gameObject);
                    _hearts.RemoveAt(i);
                }
            }
        }

        private float GetDifficultyMultiplier() {
            // Since this is a difficulty multiplier, the lives are the inverse of the multiplier
            return 1 / DifficultyManager.Instance.GetDifficultyMultiplier(difficulty);
        }

        private void ClearHearts() {
            foreach (Transform t in transform) {
                Destroy(t.gameObject);
            }

            _hearts.Clear();
        }

        public void SetPlayerLives(Lives lives) {
            playerLives = lives;
            playerLives.OnLivesChanged += UpdateUI;
            UpdateUI();
        }
    }
}