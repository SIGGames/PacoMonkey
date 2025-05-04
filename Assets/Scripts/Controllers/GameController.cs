using System.Collections.Generic;
using Enums;
using Managers;
using Platformer.Core;
using UnityEngine;
using Zones;
using static Utils.PlayerPrefsKeys;

namespace Controllers {
    public class GameController : MonoBehaviour {
        public static GameController Instance { get; private set; }

        public bool showMenuAtStart = true;

        public bool existsGameProgress;

        public MapZone currentMapZone = DefaultMapZone;

        private const MapZone DefaultMapZone = MapZone.TracHouse;

        private void OnEnable() {
            Instance = this;
        }

        private void OnDisable() {
            if (Instance == this) Instance = null;
        }

        private void Start() {
            #if !UNITY_EDITOR
            // On build, the menu should always be shown at the start
            showMenuAtStart = true;
            #endif

            // Checks if this is the first time the game is started
            if (PlayerPrefs.GetInt(IsGameStartedKey) == 0) {
                PlayerPrefs.SetInt(IsGameStartedKey, 1);
                PlayerPrefs.Save();
            } else {
                existsGameProgress = true;
            }
        }

        private void Update() {
            if (Instance == this) {
                Simulation.Tick();
            }
        }

        public void NewGame() {
            ResetInGameProgressData();
            existsGameProgress = true;
            PlayerPrefs.SetInt(IsGameStartedKey, 1);
            PlayerPrefs.Save();
            RevertGameStateToDefault();
        }

        public void StartNewGame() {
            CinematicManager.Instance.StartCinematic(Cinematic.NewGame, true);
        }

        private void ResetInGameProgressData() {
            PlayerPrefs.DeleteKey(ActiveQuestKey);
            PlayerPrefs.DeleteKey(CurrentCharacterKey);
            PlayerPrefs.DeleteKey(CurrentPlayTimeKey);
            PlayerPrefs.DeleteKey(DifficultyKey);
            PlayerPrefs.DeleteKey(IsGameStartedKey);
            PlayerPrefs.DeleteKey(RespawnPositionX);
            PlayerPrefs.DeleteKey(RespawnPositionY);

            PlayerPrefs.Save();
            existsGameProgress = false;
        }

        private void RevertGameStateToDefault() {
            CameraManager.Instance.ResetCamera();
            CharacterManager.ResetState();
            currentMapZone = DefaultMapZone;
            PlayTimeManager.Instance.ResetPlayTime();
            DialogueManager.ResetDialogues();
            QuestManager.Instance.ResetQuests();
            LevelManager.Instance.ResetLevel();
            HiddenElementsManager.Instance.ResetHiddenElements();
            ResetMapZones();
        }

        private static void ResetMapZones() {
            List<ShowTutorialKeysZone> mapZones = new(FindObjectsOfType<ShowTutorialKeysZone>());
            foreach (ShowTutorialKeysZone mapZone in mapZones) {
                mapZone.ResetZone();
            }
        }
    }
}