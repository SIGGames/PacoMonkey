using System;
using System.Collections.Generic;
using System.Linq;
using Enums;
using Localization;
using TMPro;
using UI.TextSetters;
using UnityEngine;
using static Utils.PlayerPrefsKeys;

namespace Managers {
    public class QuestManager : MonoBehaviour {
        public static QuestManager Instance { get; private set; }

        [SerializeField] private bool debugMissingTextKeys = true;
        private string _activeQuestId = "";

        [Header("Components")]
        public TextMeshProUGUI questNameText;
        public TextMeshProUGUI questTypeText;
        public TextMeshProUGUI questDescriptionText;
        public TextMeshProUGUI questCharacterText;
        public GameObject activeQuestPanel;

        public GameObject enemyCountTextPrefab;
        public TextMeshProUGUI enemyCountText;

        [SerializeField]
        private List<Quest> quests = new();

        [Header("To translate the quest name and description: Pattern + Quest Id")]
        [SerializeField] private string nameTranslationPattern = "qn-";
        [SerializeField] private string descriptionTranslationPattern = "qd-";

        private Quest _previousQuest;

        private void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start() {
            SetActiveQuest(PlayerPrefs.GetString(ActiveQuestKey));
            UpdateQuestPanelTexts();
            ShowEnemyCountText(false);
        }

        private void UpdateQuestPanelTexts() {
            Quest activeQuest = GetActiveQuest();

            if (activeQuest == null) {
                activeQuestPanel.SetActive(false);
                return;
            }
            activeQuestPanel.SetActive(true);

            if (questNameText != null) {
                questNameText.text = GetTranslatedText(nameTranslationPattern + activeQuest.id);
            }

            if (questTypeText != null) {
                questTypeText.text = GetTranslatedText(activeQuest.questType.ToString().ToLower());
            }

            if (questDescriptionText != null) {
                questDescriptionText.text = GetTranslatedText(descriptionTranslationPattern + activeQuest.id);
            }

            if (questCharacterText != null) {
                // Character is not translatable
                questCharacterText.text = GetCharacterName(activeQuest.questCharacter);
            }

            // Update the quest name in the UI
            SetActiveQuestName activeQuestNameTextSetter = FindObjectOfType<SetActiveQuestName>();
            if (activeQuestNameTextSetter != null) {
                activeQuestNameTextSetter.UpdateTextName(GetActiveQuestName());
            }
        }

        private string GetActiveQuestName() {
            return GetTranslatedText(nameTranslationPattern + _activeQuestId);
        }

        public void SetActiveQuest(string id) {
            SetActiveQuest(FindQuest(id));
        }

        public void SetQuestAvailable(string id) {
            Quest quest = FindQuest(id);
            if (quest != null) {
                quest.isAvailable = true;
            }
        }

        public void ResetQuests() {
            _activeQuestId = null;
            _previousQuest = null;
            UpdateQuestPanelTexts();
            ShowEnemyCountText(false);
            foreach (Quest quest in quests) {
                ShowDisabledGameObject(true, quest);
            }
        }

        private void SetActiveQuest(Quest quest) {
            if (quest == null) {
                if (_previousQuest == null) {
                    // If this is the first time, where the quest is null then we can reset the camera for initial scene
                    CameraManager.Instance.FollowAndLookAt(null);
                }
                return;
            }

            if (quest is not { isAvailable: true }) {
                return;
            }

            _previousQuest = GetActiveQuest();
            ManageSpecialActions(quest);
            ShowDisabledGameObject(false, quest);

            _activeQuestId = quest.id;

            PlayerPrefs.SetString(ActiveQuestKey, _activeQuestId);
            PlayerPrefs.Save();

            UpdateQuestPanelTexts();
        }

        public Quest GetActiveQuest() {
            return FindQuest(_activeQuestId);
        }

        private Quest FindQuest(string id) {
            return quests.FirstOrDefault(q => q.id == id);
        }

        private static void ManageSpecialActions(Quest quest) {
            switch (quest.id) {
                case "1":
                    CameraManager.Instance.FollowAndLookAt(CharacterManager.Instance.currentPlayerController.transform);
                    break;
                case "2.1":
                    CinematicManager.Instance.StopTimer();
                    break;
            }
        }

        public void ShowEnemyCountText(bool show, int enemyCount = 0, int originalEnemyCount = 0) {
            if (enemyCountTextPrefab != null) {
                enemyCountTextPrefab.SetActive(show);
            }
            if (enemyCountText != null) {
                enemyCountText.text = $"[{originalEnemyCount - enemyCount}/{originalEnemyCount}]";
            }
        }

        private static void ShowDisabledGameObject(bool show, Quest quest) {
            if (quest.disableGameObject && quest.gameObjectToDisable != null) {
                quest.gameObjectToDisable.SetActive(show);
            }
        }

        private string GetTranslatedText(string key) {
            string text = LocalizationManager.Instance.GetLocalizedText(key, debugMissingTextKeys);
            if (string.IsNullOrEmpty(text) || text.Contains("MISSING")) {
                return key;
            }

            return text;
        }

        private static string GetCharacterName(Character character) {
            return new string(character.ToString().Where(c => !char.IsDigit(c)).ToArray());
        }

        [Serializable]
        public class Quest {
            public string id;
            public QuestType questType;
            public Character questCharacter;
            public bool isAvailable = true;
            public bool disableGameObject;
            public GameObject gameObjectToDisable;
        }

        public enum QuestType {
            MainQuest,
            SideQuest,
        }
    }
}