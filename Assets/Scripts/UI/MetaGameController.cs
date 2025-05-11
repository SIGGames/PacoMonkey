using System.Collections;
using System.Linq;
using Controllers;
using Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using static PlayerInput.KeyBinds;

namespace UI {
    public class MetaGameController : MonoBehaviour {
        public static MetaGameController Instance { get; private set; }

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
        private bool _openedFromInGame;

        private static MusicType? _currentMusicType;
        private static Coroutine _delayedMusicCoroutine;

        private void Awake() {
            Instance = this;
        }

        private void Start() {
            if (gameController.showMenuAtStart) {
                ToggleMainMenu(true);
                // Title screen
                mainMenu.SetActivePanel(Panel.TitleScreen);
            }

            PlayMusicAudio();
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

            PlayMusicAudio();
        }

        public void GoBackOrInGame(int panelIdx) {
            // This is used to go back to the main menu or to the game depending if the menu has been accessed from the game or
            // from the main menu
            if (_openedFromInGame) {
                mainMenu.SetActivePanel(Panel.PauseMenu);
                _openedFromInGame = false;
            } else {
                mainMenu.SetActivePanel(panelIdx);
            }
        }

        public void SetOpenFromInGame(bool openPanelFromInGame) {
            _openedFromInGame = openPanelFromInGame;
        }

        private void _ToggleMainMenu(bool show) {
            if (show) {
                Time.timeScale = 0;
                mainMenu.gameObject.SetActive(true);
                EventSystem.current.SetSelectedGameObject(mainMenuGameObject);
                foreach (Canvas i in gamePlayCanvasii) i.gameObject.SetActive(false);
                SetCursorVisible(true);
            } else {
                Time.timeScale = 1;
                mainMenu.gameObject.SetActive(false);
                foreach (Canvas i in gamePlayCanvasii) i.gameObject.SetActive(true);
                SetCursorVisible(false);
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
                    mainMenu.SetActivePanel(Panel.Quests);
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

        private static void SetCursorVisible(bool show) {
            #if UNITY_EDITOR || UNITY_WEBGL
            return;
            #endif
            #pragma warning disable CS0162 // Unreachable code detected
            Cursor.visible = show;
            Cursor.lockState = show ? CursorLockMode.None : CursorLockMode.Locked;
            #pragma warning restore CS0162 // Unreachable code detected
        }

        private MusicType? GetTargetMusicType() {
            if (!IsMenuOpen) {
                return MusicType.Game;
            }

            if (IsInPanel("PauseMenu") || IsInPanel("Quests")) {
                return null;
            }

            return MusicType.Menu;
        }

        private static IEnumerator DelayedMusicRetry() {
            const float retryDelay = 0.3f;
            yield return new WaitForSecondsRealtime(retryDelay);

            _delayedMusicCoroutine = null;
            PlayMusicAudio();
        }

        private static void PlayMusicAudio() {
            MusicType? newType = Instance.GetTargetMusicType();

            // When there is no new type its because the game is on a menu that it must do not modify the current music,
            // but it must retry the search the music in case the user moves into another menu that needs to play menu music.
            if (newType == null) {
                _delayedMusicCoroutine ??= Instance.StartCoroutine(DelayedMusicRetry());
                return;
            }

            if (_currentMusicType == newType) {
                return;
            }

            _currentMusicType = newType;
            if (AudioManager.Instance != null) {
                AudioManager.Instance.PlayMusic(newType.Value, MusicSoundType.Calm);
            }
        }
    }
}