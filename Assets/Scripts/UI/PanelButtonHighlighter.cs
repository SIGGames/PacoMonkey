using System;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI {
    public class PanelButtonHighlighter : MonoBehaviour {
        [SerializeField] private SubPanelController subPanelController;

        [SerializeField] private bool useDefaultActiveColor = true;
        [SerializeField] private bool useDefaultNormalColor;

        [SerializeField, HideIf("useDefaultActiveColor")]
        private Color activeColor = new(185f / 255f, 171f / 255f, 209f / 255f, 1f);

        [SerializeField, HideIf("useDefaultNormalColor")]
        private Color inactiveColor = new(227 / 255f, 220f / 255f, 230f / 255f, 1f);

        private void Awake() {
            if (subPanelController == null) {
                Debug.LogError("SubPanelController is not set in PanelButtonHighlighter");
                enabled = false;
            }
        }

        private void Start() {
            if (useDefaultActiveColor) {
                activeColor = GetComponent<Button>().colors.selectedColor;
            }

            if (useDefaultNormalColor) {
                inactiveColor = GetComponent<Button>().colors.normalColor;
            }
        }

        private void Update() {
            UpdateHighlight();
        }

        private void UpdateHighlight() {
            // This is really inefficient, I could just do this in the button's OnSelect event but I'm lazy. Anyway, this is UI code so it does not affect in-game performance.
            GameObject currentSelected = EventSystem.current.currentSelectedGameObject;
            Button[] buttons = GetComponentsInChildren<Button>(true);
            foreach (Button button in buttons) {
                bool isActive = false;
                if (currentSelected != null) {
                    // My brain was really fried when I wrote this, but it works.
                    GameObject buttonIndexPanel = subPanelController.subPanels.First(panel => panel.firstSelectedButton.name == button.name).panelGameObject;
                    isActive = currentSelected == button.gameObject || currentSelected.transform.IsChildOf(buttonIndexPanel.transform);
                }

                Image img = button.GetComponent<Image>();
                if (img != null) {
                    img.color = isActive ? activeColor : inactiveColor;
                }
            }
        }
    }
}