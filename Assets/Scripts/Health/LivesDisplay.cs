using UnityEngine;
using UnityEngine.UI;

public class LivesDisplay : MonoBehaviour {
    [SerializeField] private Health.Lives playerLives;
    [SerializeField] private GameObject lifeIconPrefab;
    [SerializeField] private Transform livesContainer;

    private void Start() {
        UpdateLivesDisplay();
    }

    private void OnEnable() {
        if (playerLives != null) {
            playerLives.OnLivesChanged += UpdateLivesDisplay;
        }
    }

    private void OnDisable() {
        if (playerLives != null) {
            playerLives.OnLivesChanged -= UpdateLivesDisplay;
        }
    }

    private void UpdateLivesDisplay() {
        Debug.Log("Updating lives display");
        foreach (Transform child in livesContainer) {
            Destroy(child.gameObject);
        }

        int fullLives = Mathf.FloorToInt(playerLives.CurrentLives);
        bool hasHalfLife = (playerLives.CurrentLives % 1) != 0;

        for (int i = 0; i < fullLives; i++) {
            Instantiate(lifeIconPrefab, livesContainer);
        }

        if (hasHalfLife) {
            GameObject halfLifeIcon = Instantiate(lifeIconPrefab, livesContainer);
            halfLifeIcon.GetComponent<Image>().color = new Color(1, 1, 1, 0.5f);
        }
    }
}