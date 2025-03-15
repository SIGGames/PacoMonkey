using Controllers;
using Managers;
using UnityEngine;
using static PlayerInput.KeyBinds;

namespace UI {
    public class MetaGameController : MonoBehaviour {
        // The main UI object which used for the menu.
        public MainUIController mainMenu;

        // List of canvas objects which are used during gameplay (when the main ui is turned off)
        public Canvas[] gamePlayCanvasii;

        public GameController gameController;

        public static bool IsMenuOpen { get; private set; }

        private bool _showMainCanvas;
        private bool _controlWasEnabled;

        private void OnEnable() {
            _ToggleMainMenu(_showMainCanvas);
        }

        public void ToggleMainMenu(bool show) {
            if (_showMainCanvas != show) {
                _ToggleMainMenu(show);
            }
        }

        private void _ToggleMainMenu(bool show) {
            if (show) {
                Time.timeScale = 0;
                mainMenu.gameObject.SetActive(true);
                foreach (var i in gamePlayCanvasii) i.gameObject.SetActive(false);
            } else {
                Time.timeScale = 1;
                mainMenu.gameObject.SetActive(false);
                foreach (var i in gamePlayCanvasii) i.gameObject.SetActive(true);
            }

            IsMenuOpen = show;
            _showMainCanvas = show;
        }

        private void Update() {
            if (!GetMenuKey()) {
                return;
            }

            bool openingMenu = !_showMainCanvas;
            ToggleMainMenu(openingMenu);

            if (openingMenu) {
                _controlWasEnabled = CharacterManager.Instance.currentPlayerController.controlEnabled;
            }

            if (_controlWasEnabled) {
                CharacterManager.Instance.currentPlayerController.FreezePosition(openingMenu);
            }
        }
    }
}