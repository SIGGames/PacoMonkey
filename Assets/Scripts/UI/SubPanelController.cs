using UnityEngine;

namespace UI {
    public class SubPanelController : MonoBehaviour {
        public GameObject[] subPanels;

        public void SetActiveSubPanel(int index) {
            for (int i = 0; i < subPanels.Length; i++) {
                bool active = i == index;
                if (subPanels[i].activeSelf != active) {
                    subPanels[i].SetActive(active);
                }
            }
        }

        private void OnEnable() {
            SetActiveSubPanel(0);
        }
    }
}