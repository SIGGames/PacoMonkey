using System.Collections.Generic;
using UnityEngine;

namespace Health.UI {
    public class HealthBar : MonoBehaviour {
        public GameObject heartPrefab;
        public Lives playerLives;
        private readonly List<HealthHeart> _hearts = new();

        void Start() {
            playerLives.OnLivesChanged += UpdateUI;
            InitializeHearts();
            UpdateUI();
        }

        private void OnDestroy() {
            playerLives.OnLivesChanged -= UpdateUI;
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

        public void UpdateUI() {
            int maxLives = Mathf.CeilToInt(playerLives.GetMaxLives());

            if (_hearts.Count != maxLives) {
                AdjustHearts(maxLives);
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