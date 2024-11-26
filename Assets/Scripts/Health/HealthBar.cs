using UnityEngine;
using UnityEngine.UI;

namespace Health {
    public class HealthBar : MonoBehaviour {
        [SerializeField] private Lives playerLives;
        [SerializeField] private Image totalHealthBar;
        [SerializeField] private Image currentHealthBar;

        void Start() {
            totalHealthBar.fillAmount = playerLives.CurrentLives / playerLives.GetMaxLives();
            playerLives.OnLivesChanged += UpdateHealthBar;
        }

        private void UpdateHealthBar() {
            currentHealthBar.fillAmount = playerLives.CurrentLives / playerLives.GetMaxLives();
        }

        void OnDestroy() {
            playerLives.OnLivesChanged -= UpdateHealthBar;
        }
    }
}