using Health;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour {
    [SerializeField] private Lives playerLives;
    [SerializeField] private Image totalHealthBar;
    [SerializeField] private Image currentHealthBar;


    void Start() {
        totalHealthBar.fillAmount = playerLives.CurrentLives / playerLives.GetMaxLives();
    }

    void Update() {
        currentHealthBar.fillAmount = playerLives.CurrentLives / playerLives.GetMaxLives();
    }
}