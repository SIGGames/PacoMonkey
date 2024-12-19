using Mechanics;
using UnityEngine;
using static PlayerInput.KeyBinds;

namespace UI {
    public class MetaGameController : MonoBehaviour {
        // The main UI object which used for the menu.
        public MainUIController mainMenu;

        // List of canvas objects which are used during gameplay (when the main ui is turned off)
        public Canvas[] gamePlayCanvasii;

        public GameController gameController;

        bool showMainCanvas = false;

        void OnEnable() {
            _ToggleMainMenu(showMainCanvas);
        }

        public void ToggleMainMenu(bool show) {
            if (this.showMainCanvas != show) {
                _ToggleMainMenu(show);
            }
        }

        void _ToggleMainMenu(bool show) {
            if (show) {
                Time.timeScale = 0;
                mainMenu.gameObject.SetActive(true);
                foreach (var i in gamePlayCanvasii) i.gameObject.SetActive(false);
            }
            else {
                Time.timeScale = 1;
                mainMenu.gameObject.SetActive(false);
                foreach (var i in gamePlayCanvasii) i.gameObject.SetActive(true);
            }

            showMainCanvas = show;
        }

        void Update() {
            if (GetMenuKey()) {
                ToggleMainMenu(show: !showMainCanvas);
            }
        }
    }
}