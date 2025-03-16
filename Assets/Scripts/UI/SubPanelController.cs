using UnityEngine;
using UnityEngine.UI;

namespace UI {
    public class SubPanelController : MonoBehaviour {
        public GameObject[] subPanels;

        // This needs to be the same length as subPanels and in the same order
        public Image[] subPanelsButtonsImages;

        public Color32 activeColor = new(159, 122, 222, 255); // Purple
        public Color32 inactiveColor = new(207, 201, 217, 255); // Magenta

        public void SetActiveSubPanel(int index) {
            for (int i = 0; i < subPanels.Length; i++) {
                bool active = i == index;
                if (subPanels[i].activeSelf != active) {
                    subPanels[i].SetActive(active);
                }

                subPanelsButtonsImages[i].color = active ? activeColor : inactiveColor;
            }
        }

        private void OnEnable() {
            SetActiveSubPanel(0);
        }
    }
}