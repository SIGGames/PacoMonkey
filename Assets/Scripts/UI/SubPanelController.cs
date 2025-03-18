using Controllers;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
    public class SubPanelController : MonoBehaviour {
        public GameObject[] subPanels;

        [SerializeField]
        private bool configureButtons = true;

        [SerializeField, ShowIf("configureButtons"),
         Tooltip("The images of the sub-panel buttons, this must be at the same order as the sub-panels")]
        private Image[] subPanelsButtonsImages;

        [SerializeField, ShowIf("configureButtons")]
        private Color32 activeColor = new(159, 122, 222, 255); // Purple

        [SerializeField, ShowIf("configureButtons")]
        private Color32 inactiveColor = new(207, 201, 217, 255); // Magenta

        [SerializeField]
        private bool isLoadSavePanel;

        public void SetActiveSubPanel(int index) {
            for (int i = 0; i < subPanels.Length; i++) {
                bool active = i == index;
                if (subPanels[i].activeSelf != active) {
                    subPanels[i].SetActive(active);
                }

                if (configureButtons) {
                    subPanelsButtonsImages[i].color = active ? activeColor : inactiveColor;
                }
            }
        }

        private void OnEnable() {
            if (isLoadSavePanel && GameController.Instance != null && subPanels.Length > 1) {
                SetActiveSubPanel(GameController.Instance.existsGameProgress ? 1 : 0);
            } else {
                SetActiveSubPanel(0);
            }
        }
    }
}