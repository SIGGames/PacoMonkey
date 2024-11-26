using UnityEngine;
using UnityEngine.UI;

namespace Health.UI {
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

        private void HideHealthBar() {
            gameObject.SetActive(false);
        }

        private void ShowHealthBar() {
            gameObject.SetActive(true);
            currentHealthBar.fillAmount = currentHealthBar.fillAmount;
        }

        void OnDestroy() {
            playerLives.OnLivesChanged -= UpdateHealthBar;
        }
    }
}