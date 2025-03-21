using System.Linq;
using Controllers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI {
    public class SubPanelController : MonoBehaviour {
        public PanelObject[] subPanels;

        [SerializeField] private bool setActiveOnSelect = true;
        [SerializeField] private bool isLoadSavePanel;

        public void SetActiveSubPanel(int index) {
            for (int i = 0; i < subPanels.Length; i++) {
                bool active = i == index;
                if (subPanels[i].panelGameObject.activeSelf != active) {
                    subPanels[i].panelGameObject.SetActive(active);
                }

                if (active && isLoadSavePanel) {
                    EventSystem.current.SetSelectedGameObject(subPanels[index].firstSelectedButton);
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
            if (subPanels.Any(subPanel => subPanel.panelGameObject.name == current.name)) {
                SetActiveSubPanel(subPanels.ToList().FindIndex(subPanel => subPanel.panelGameObject.name == current.name));
            }
        }
    }
}