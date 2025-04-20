using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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

        [SerializeField]
        private List<Quest> quests = new();

        [Header("To translate the quest name and description: Pattern + Quest Id")]
        [SerializeField] private string nameTranslationPattern = "qn-";

        [SerializeField] private string descriptionTranslationPattern = "qd-";

        private void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start() {
            _activeQuestId = PlayerPrefs.GetString(ActiveQuestKey);
            UpdateQuestPanelTexts();
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
            UpdateQuestPanelTexts();
        }

        private void SetActiveQuest(Quest quest) {
            if (quest is not { isAvailable: true }) {
                return;
            }

            ManageSpecialActions(quest);

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
            if (quest.id == "2.1") {
                CinematicManager.Instance.StopTimer();
            }
        }

        private string GetTranslatedText(string key) {
            string text = LocalizationManager.Instance.GetLocalizedText(key, debugMissingTextKeys);
            if (string.IsNullOrEmpty(text) || text.Contains("MISSING")) {
                return key;
            }

            return FormatText(text);
        }

        private static string FormatText(string input) {
            input = input.Replace("\\n", "\n");
            input = Regex.Replace(input, @"\*\*(.*?)\*\*", "<b>$1</b>");
            input = Regex.Replace(input, @"(?<!\*)\*(?!\*)(.*?)\*(?!\*)", "<i>$1</i>");
            return input;
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
        }

        public enum QuestType {
            MainQuest,
            SideQuest,
        }
    }
}