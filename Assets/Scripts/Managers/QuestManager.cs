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
using Random = UnityEngine.Random;

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
            _activeQuestId = PlayerPrefs.GetString(ActiveQuestKey, quests.FirstOrDefault()?.id ?? string.Empty);
            UpdateQuestPanelTexts();
        }

        private void UpdateQuestPanelTexts() {
            Quest activeQuest = GetActiveQuest();

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

        public void ResetQuests() {
            _activeQuestId = quests.FirstOrDefault()?.id ?? string.Empty;
            UpdateQuestPanelTexts();
        }

        private void SetActiveQuest(Quest quest) {
            _activeQuestId = quest.id;

            PlayerPrefs.SetString(ActiveQuestKey, _activeQuestId);
            PlayerPrefs.Save();

            UpdateQuestPanelTexts();
        }

        public Quest GetActiveQuest() {
            // In case there is no active quest, return the first one
            return FindQuest(_activeQuestId) ?? quests.FirstOrDefault();
        }

        private Quest FindQuest(string id) {
            Quest quest = quests.FirstOrDefault(q => q.id == id);
            if (quest == null) {
                Debug.LogWarning($"Quest with ID {id} not found");
            }

            return quest;
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
        }

        public enum QuestType {
            MainQuest,
            SideQuest,
        }
    }
}