using System.Linq;
using Controllers;
using Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using static PlayerInput.KeyBinds;

namespace UI {
    public class MetaGameController : MonoBehaviour {
        // The main UI object which used for the menu.
        public MainUIController mainMenu;

        // List of canvas objects which are used during gameplay (when the main ui is turned off)
        public Canvas[] gamePlayCanvasii;

        public GameController gameController;

        [SerializeField] private GameObject mainMenuGameObject;

        public static bool IsMenuOpen { get; private set; }

        public static bool isUIMenusDisabled;

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
                EventSystem.current.SetSelectedGameObject(mainMenuGameObject);
                foreach (Canvas i in gamePlayCanvasii) i.gameObject.SetActive(false);
            } else {
                Time.timeScale = 1;
                mainMenu.gameObject.SetActive(false);
                foreach (Canvas i in gamePlayCanvasii) i.gameObject.SetActive(true);
            }

            IsMenuOpen = show;
            _showMainCanvas = show;
        }

        private void Update() {
            // Commented since this is not used in the game, but it can be used in the future to open configuration menu
            /*
            if (GetConfigurationKey() && !IsMenuOpen) {
                if (!_showMainCanvas) {
                    ToggleMainMenu(true);
                }

                // Configuration menu
                mainMenu.SetActivePanel(2);
                return;
            }*/

            if (isUIMenusDisabled) {
                return;
            }

            if (GetPauseKey()) {
                if (IsMenuOpen && !IsInPanel("PauseMenu")) {
                    return;
                }

                bool openingMenu = !_showMainCanvas;
                ToggleMainMenu(openingMenu);
            }

            if (GetMenuKey() && QuestManager.Instance.GetActiveQuest() != null) {
                // Quest menu
                if (!IsMenuOpen) {
                    ToggleMainMenu(true);
                    mainMenu.SetActivePanel(5);
                } else {
                    // This menu can be toggled if the game is on the quest menu
                    if (IsInPanel("Quests")) {
                        ToggleMainMenu(false);
                    }
                }
            }
        }

        private bool IsInPanel(string panelName) {
            return mainMenu.panels.Any(
                panel => panel.panelGameObject.name.Contains(panelName) && panel.panelGameObject.activeSelf);
        }
    }
}