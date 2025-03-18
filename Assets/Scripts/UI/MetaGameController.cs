using System.Linq;
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

        private void Start() {
            if (gameController.showMenuAtStart) {
                ToggleMainMenu(true);
                // Title screen
                mainMenu.SetActivePanel(1);
            }
        }

        private void OnEnable() {
            _ToggleMainMenu(_showMainCanvas);
        }

        public void ToggleMainMenu(bool show) {
            if (_showMainCanvas != show) {
                if (show) {
                    _controlWasEnabled = CharacterManager.Instance.currentPlayerController.controlEnabled;
                }

                _ToggleMainMenu(show);
                if (_controlWasEnabled) {
                    CharacterManager.Instance.currentPlayerController.FreezePosition(show);
                }
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
            if (GetConfigurationKey()) {
                if (!_showMainCanvas) {
                    ToggleMainMenu(true);
                }

                // Configuration menu
                mainMenu.SetActivePanel(2);
                return;
            }

            if (GetMenuKey()) {
                if (IsMenuOpen && !CanResumeGame()) {
                    return;
                }

                bool openingMenu = !_showMainCanvas;
                ToggleMainMenu(openingMenu);
            }
        }

        private bool CanResumeGame() {
            return mainMenu.panels.Any(panel => panel.name.Contains("PauseMenu") && panel.activeSelf);
        }
    }
}