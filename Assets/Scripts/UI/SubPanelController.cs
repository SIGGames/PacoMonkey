using Controllers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI {
    public class SubPanelController : MonoBehaviour, IPointerEnterHandler, ISelectHandler {
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

        public void OnPointerEnter(PointerEventData eventData) {
                Debug.Log("1");
            if (setActiveOnSelect && eventData.pointerEnter != null) {
                for (int i = 0; i < subPanels.Length; i++) {
                    if (eventData.pointerEnter == subPanels[i]) {
                        SetActiveSubPanel(i);
                        break;
                    }
                }
            }
        }

        public void OnSelect(BaseEventData eventData) {
                Debug.Log("2");
            if (setActiveOnSelect && eventData.selectedObject != null) {
                for (int i = 0; i < subPanels.Length; i++) {
                    if (eventData.selectedObject == subPanels[i]) {
                        SetActiveSubPanel(i);
                        break;
                    }
                }
            }
        }
    }
}