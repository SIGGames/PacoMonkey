using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI {
    /// <summary>
    /// A simple controller for switching between UI panels.
    /// </summary>
    public class MainUIController : MonoBehaviour {
        public PanelObject[] panels;

        private GameObject _lastValidSelection;

        public void SetActivePanel(Panel panel) {
            SetActivePanel((int)panel);
        }

        public void SetActivePanel(int index) {
            for (int i = 0; i < panels.Length; i++) {
                bool active = i == index;
                var g = panels[i].panelGameObject;
                if (g.activeSelf != active) {
                    g.SetActive(active);
                }
            }

            StartCoroutine(SelectFirst(panels[index].firstSelectedButton));
        }

        private IEnumerator SelectFirst(GameObject button) {
            yield return null; // Wait for the next frame to ensure the object is active
            if (button != null) {
                EventSystem.current.SetSelectedGameObject(button);
                _lastValidSelection = button;
            }
        }

        private void OnEnable() {
            SetActivePanel(Panel.PauseMenu);
        }

        private void Update() {
            var eventSystem = EventSystem.current;
            if (eventSystem.currentSelectedGameObject == null ||
                !IsSelectable(eventSystem.currentSelectedGameObject)) {
                eventSystem.SetSelectedGameObject(_lastValidSelection);
            } else {
                _lastValidSelection = eventSystem.currentSelectedGameObject;
            }
        }

        private static bool IsSelectable(GameObject obj) {
            return obj != null && obj.GetComponent<Selectable>() != null;
        }
    }

    [Serializable]
    public class PanelObject {
        public GameObject panelGameObject;
        public GameObject firstSelectedButton;
    }

    public enum Panel {
        PauseMenu = 0,
        TitleScreen = 1,
        Configuration = 2,
        Credits = 3,
        LoadGame = 4,
        Quests = 5,
    }
}