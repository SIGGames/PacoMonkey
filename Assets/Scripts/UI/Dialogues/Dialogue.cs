using System;
using System.Collections;
using Controllers;
using Enums;
using Localization;
using Managers;
using NaughtyAttributes;
using TMPro;
using UnityEditor.ColorRangeDrawers;
using UnityEngine;
using UnityEngine.UI;
using static PlayerInput.KeyBinds;

namespace UI.Dialogues {
    public class Dialogue : MonoBehaviour {
        // Components
        private static GameObject DialoguePanel => DialogueManager.Instance.DialoguePanel;
        private static TextMeshProUGUI DialogueText => DialogueManager.Instance.DialogueText;
        private static TextMeshProUGUI DialogueTitle => DialogueManager.Instance.DialogueTitle;
        private static Image DialogueImage => DialogueManager.Instance.DialogueImage;

        [Header("Configuration")]
        [SerializeField]
        private string title;

        [SerializeField]
        private Sprite dialogueSprite;

        [SerializeField]
        private bool progressiveTyping = true;

        [SerializeField, ShowIf("progressiveTyping"), Range(0.01f, 0.3f)]
        private float wordSpeed = 0.01f;

        [SerializeField, ColorRange(0.5f, 5)]
        private ColorRangeValue playerDistance = new(2, Color.black);

        [Header("Interact Button Before Interact")]
        [SerializeField]
        private bool showInteractButtonBeforeInteract = true;

        [SerializeField, ShowIf("showInteractButtonBeforeInteract")]
        private Vector2 interactButtonOffset = new(0, 1f);

        [SerializeField]
        private bool triggerQuestOnFinish;

        [SerializeField, ShowIf("triggerQuestOnFinish")]
        private string questIdToTrigger = "";

        [SerializeField]
        private bool triggerCinematicOnFinish;

        [SerializeField, ShowIf("triggerCinematicOnFinish")] private Cinematic cinematicToTrigger;

        [Header("Dialogues")]
        [SerializeField]
        private bool ensureMultipleLanguagesDialoguesLength = true;

        [SerializeField] private DialogueLine[] dialogueCa;
        [SerializeField] private DialogueLine[] dialogueEs;
        [SerializeField] private DialogueLine[] dialogueEn;
        private DialogueLine[] _dialogue;

        [Header("Alternative Dialogue")]
        [SerializeField,
         Tooltip("This dialogue will be shown once the default dialogue has finished and the player interacts again")]
        private bool hasAlternativeDialogue = true;

        [SerializeField, ShowIf("hasAlternativeDialogue")]
        private bool resetAfterShowAlternativeDialogue;

        [SerializeField, ShowIf("resetAfterShowAlternativeDialogue"), Range(0.1f, 240),
         Tooltip("Time in seconds to reset the dialogue and use the default text")]
        private float resetDelay = 10f;

        [SerializeField, ShowIf("hasAlternativeDialogue")]
        private DialogueLine[] alternativeDialogueCa;

        [SerializeField, ShowIf("hasAlternativeDialogue")]
        private DialogueLine[] alternativeDialogueEs;

        [SerializeField, ShowIf("hasAlternativeDialogue")]
        private DialogueLine[] alternativeDialogueEn;

        // Properties
        private static PlayerController PlayerController => CharacterManager.Instance.currentPlayerController;
        private static Vector3 PlayerPosition => PlayerController.transform.position;
        private bool PlayerIsClose => Vector3.Distance(transform.position, PlayerPosition) < playerDistance.value;
        private int _index;
        private DialogueCharacter _dialogueCharacter;

        // Unique identifier for each NPC
        private string _npcGuid;
        private static string _currentNpcGuid = "";
        private Coroutine _typingCoroutine;
        private GameObject _interactButtonInstance;
        private const bool FreezePlayer = true;
        private bool _mustShowAlternativeDialogue;
        private bool _hasActivated;
        private const string InteractButtonIdentifier = "FloatingDialogueBeforeInteract";

        private void Awake() {
            _npcGuid = Guid.NewGuid().ToString();
        }

        private void Start() {
            DialoguePanel.SetActive(false);
            ResetText();
            _dialogue = GetCurrentDialogue();
            _dialogueCharacter = _dialogue[0].character;
            CheckDialoguesLength();
        }

        private void Update() {
            if (showInteractButtonBeforeInteract) {
                HandleInteractButtonBeforeInteract();
            }

            if (MetaGameController.IsMenuOpen) {
                return;
            }

            if (!PlayerIsClose) {
                return;
            }

            if (!GetInteractKey()) {
                return;
            }

            // If there are multiple NPCs overlapping, only the closest one will interact
            if (!IsClosestNpc()) {
                return;
            }

            // If the panel is open and comes from another NPC, reset the dialogue
            if (DialoguePanel.activeInHierarchy && _currentNpcGuid != _npcGuid) {
                ResetText();
            }

            _currentNpcGuid = _npcGuid;

            if (!_mustShowAlternativeDialogue) {
                _dialogue = GetCurrentDialogue();
            }

            if (DialoguePanel.activeInHierarchy) {
                NextLine();
            } else {
                if (_mustShowAlternativeDialogue) {
                    ShowAlternativeDialogue();
                } else {
                    DialoguePanel.SetActive(true);
                    SetImage();
                    SetTitle();
                    if (_typingCoroutine != null) {
                        StopCoroutine(_typingCoroutine);
                    }

                    _typingCoroutine = StartTyping();
                    _mustShowAlternativeDialogue = HasAlternativeDialogue();
                }
            }

            if (FreezePlayer) {
                if (DialoguePanel.activeInHierarchy) {
                    PlayerController.FreezePosition(true, true);
                } else {
                    PlayerController.FreezePosition(false);
                }
            }
        }

        private bool IsClosestNpc() {
            Dialogue[] dialogues = FindObjectsOfType<Dialogue>();
            float npcDistance = Vector3.Distance(transform.position, PlayerPosition);
            foreach (Dialogue d in dialogues) {
                if (d == this) {
                    continue;
                }

                float candidateDistance = Vector3.Distance(d.transform.position, PlayerPosition);
                if (candidateDistance < npcDistance) {
                    return false;
                }
            }

            return true;
        }

        private void SetImage() {
            DialogueImage.sprite = _dialogueCharacter == DialogueCharacter.Npc
                ? dialogueSprite
                : CharacterManager.Instance.currentCharacterFaceSprite;
        }

        private void SetTitle() {
            if (_dialogueCharacter == DialogueCharacter.Npc) {
                DialogueTitle.text = string.IsNullOrEmpty(title) ? name : title;
            } else {
                DialogueTitle.text = _dialogueCharacter.ToString();
            }
        }

        private void NextLine() {
            if (_dialogue.Length == 0) {
                return;
            }

            if (_index < _dialogue.Length - 1 && _dialogue[_index].text != "") {
                _index++;
                DialogueText.text = "";
                if (_typingCoroutine != null) {
                    StopCoroutine(_typingCoroutine);
                }

                _typingCoroutine = StartTyping();
            } else {
                if (!_hasActivated) {
                    TriggerQuestIfNeeded();
                    TriggerCinematicIfNeeded();
                    _hasActivated = true;
                }
                ResetText();
            }
        }

        private void ResetText() {
            if (_typingCoroutine != null) {
                StopCoroutine(_typingCoroutine);
            }

            DialogueText.text = "";
            _index = 0;
            DialoguePanel.SetActive(false);
        }

        private Coroutine StartTyping() {
            return StartCoroutine(Typing());
        }

        private IEnumerator Typing() {
            // If the current character changed update the title and the image
            if (_dialogueCharacter != _dialogue[_index].character) {
                _dialogueCharacter = _dialogue[_index].character;
                SetTitle();
                SetImage();
            }

            if (!progressiveTyping) {
                DialogueText.text = _dialogue[_index].text;
                yield break;
            }

            foreach (char letter in _dialogue[_index].text) {
                DialogueText.text += letter;
                yield return new WaitForSeconds(wordSpeed);
            }
        }

        private DialogueLine[] GetCurrentDialogue() {
            DialogueLine[] selectedDialogue = LocalizationManager.Instance.currentLanguage switch {
                Language.Catalan => dialogueCa,
                Language.Spanish => dialogueEs,
                Language.English => dialogueEn,
                _ => throw new ArgumentOutOfRangeException()
            };

            if (selectedDialogue == null || selectedDialogue.Length == 0) {
                DialogueLine[] defaultDialogue = { new() { text = "No dialogue", character = DialogueCharacter.Micca } };
                dialogueCa = defaultDialogue;
                dialogueEs = defaultDialogue;
                dialogueEn = defaultDialogue;
                selectedDialogue = defaultDialogue;
            }

            return selectedDialogue;
        }

        private DialogueLine[] GetAlternativeDialogue() {
            DialogueLine[] alt = LocalizationManager.Instance.currentLanguage switch {
                Language.Catalan => alternativeDialogueCa,
                Language.Spanish => alternativeDialogueEs,
                Language.English => alternativeDialogueEn,
                _ => throw new ArgumentOutOfRangeException()
            };
            if (alt == null || alt.Length == 0) {
                DialogueLine[] defaultDialogue =
                    { new() { text = "No alternative dialogue", character = DialogueCharacter.Micca } };
                alternativeDialogueCa = defaultDialogue;
                alternativeDialogueEs = defaultDialogue;
                alternativeDialogueEn = defaultDialogue;
                alt = defaultDialogue;
            }

            return alt;
        }

        private bool HasAlternativeDialogue() {
            if (!hasAlternativeDialogue) {
                return false;
            }

            DialogueLine[] alt = GetAlternativeDialogue();
            return alt is { Length: > 0 };
        }

        private void CheckDialoguesLength() {
            if (!ensureMultipleLanguagesDialoguesLength) {
                return;
            }

            int targetLength = _dialogue.Length;
            if (dialogueCa.Length != targetLength || dialogueEs.Length != targetLength ||
                dialogueEn.Length != targetLength) {
                throw new Exception("Dialogues length must be the same");
            }
        }

        private void ShowAlternativeDialogue() {
            ResetText();
            _dialogue = GetAlternativeDialogue();

            // Stop the coroutine if it's still running and start the new one
            if (_typingCoroutine != null) {
                StopCoroutine(_typingCoroutine);
            }

            if (_dialogue[_index].text == "") {
                ResetText();
                return;
            }

            _typingCoroutine = StartTyping();
            DialoguePanel.SetActive(true);

            if (resetAfterShowAlternativeDialogue) {
                StartCoroutine(ResetAfterDelay());
            }
        }

        private IEnumerator ResetAfterDelay(float delay = -1) {
            float effectiveDelay = delay < 0 ? resetDelay : delay;
            yield return new WaitForSeconds(effectiveDelay);
            // If it's still active, wait until it's not
            if (DialoguePanel.activeSelf) {
                StartCoroutine(ResetAfterDelay(0.1f));
                yield break;
            }

            ResetDialogue();
        }

        private void HandleInteractButtonBeforeInteract() {
            if (PlayerIsClose && !DialoguePanel.activeSelf) {
                ShowInteractButton();
            } else {
                HideInteractButton();
            }
        }

        private void ShowInteractButton() {
            if (_interactButtonInstance == null) {
                _interactButtonInstance = Instantiate(DialogueManager.Instance.beforeInteractButtonPrefab, transform);
                _interactButtonInstance.name = InteractButtonIdentifier;
            }

            _interactButtonInstance.transform.position = transform.position + (Vector3)interactButtonOffset;
            _interactButtonInstance.SetActive(true);
        }

        private void HideInteractButton() {
            if (_interactButtonInstance != null) {
                _interactButtonInstance.SetActive(false);
            }
        }

        public void ResetDialogue() {
            ResetText();
            _dialogue = GetCurrentDialogue();
            _mustShowAlternativeDialogue = false;
        }

        private void OnDrawGizmosSelected() {
            Gizmos.color = playerDistance.color;
            Gizmos.DrawWireSphere(transform.position, playerDistance.value);
        }

        private void TriggerQuestIfNeeded() {
            if (triggerQuestOnFinish && !string.IsNullOrEmpty(questIdToTrigger)) {
                QuestManager.Instance.SetActiveQuest(questIdToTrigger);
            }
        }

        private void TriggerCinematicIfNeeded() {
            if (triggerCinematicOnFinish) {
                CinematicManager.Instance.StartCinematic(cinematicToTrigger);
            }
        }
    }
}