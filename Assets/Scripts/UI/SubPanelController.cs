using System.Linq;
using Controllers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI {
    public class SubPanelController : MonoBehaviour {
        public GameObject[] subPanels;

        [SerializeField] private bool setActiveOnSelect = true;
        [SerializeField] private bool isLoadSavePanel;

        public void SetActiveSubPanel(int index) {
            for (int i = 0; i < subPanels.Length; i++) {
                bool active = i == index;
                if (subPanels[i].activeSelf != active) {
                    subPanels[i].SetActive(active);
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

        private void Update() {
            if (!setActiveOnSelect) {
                return;
            }

            // Actives the sub panel when a button is selected
            GameObject current = EventSystem.current.currentSelectedGameObject;
            if (subPanels.Any(subPanel => subPanel.name == current.name)) {
                SetActiveSubPanel(subPanels.ToList().FindIndex(subPanel => subPanel.name == current.name));
            }
        }
    }
}