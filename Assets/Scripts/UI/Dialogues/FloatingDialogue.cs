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
using Utils;
using static PlayerInput.KeyBinds;

namespace UI.Dialogues {
    public class FloatingDialogue : MonoBehaviour {
        // Components
        private GameObject _dialoguePanel;
        private TextMeshProUGUI _dialogueText;
        private TextMeshProUGUI _dialogueTitle;

        [Header("Configuration")]
        [SerializeField]
        private string title;

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

        private int _index;
        private DialogueCharacter _dialogueCharacter;
        private Coroutine _typingCoroutine;
        private GameObject _interactButtonInstance;
        public bool mustShowAlternativeDialogue;

        private static PlayerController PlayerController => CharacterManager.Instance.currentPlayerController;
        private static Vector3 PlayerPosition => PlayerController.transform.position;
        private bool PlayerIsClose => TargetExists && Vector3.Distance(transform.position, PlayerPosition) < playerDistance.value;
        private static bool TargetExists => PlayerController != null;

        private const string DialoguePanelIdentifier = "FloatingDialoguePanel";
        private const string DialogueTextIdentifier = "FloatingDialogueText";
        private const string DialogueTitleIdentifier = "FloatingDialogueTitle";
        private const string InteractButtonIdentifier = "FloatingDialogueBeforeInteract";

        private void Awake() {
            _dialoguePanel = transform.Find(DialoguePanelIdentifier)?.gameObject;
            if (_dialoguePanel == null) {
                Debug.LogError("DialoguePanel not found as a child of " + gameObject.name);
                return;
            }

            _dialogueText = _dialoguePanel.transform.Find(DialogueTextIdentifier)?.GetComponent<TextMeshProUGUI>();
            _dialogueTitle = _dialoguePanel.transform.Find(DialogueTitleIdentifier)?.GetComponent<TextMeshProUGUI>();

            Debugger.LogIfNull((nameof(_dialoguePanel), _dialoguePanel), (nameof(_dialogueText), _dialogueText),
                (nameof(_dialogueTitle), _dialogueTitle));
        }

        private void Start() {
            _dialoguePanel.SetActive(false);
            ResetText();
            _dialogue = GetCurrentDialogue();
            _dialogueCharacter = _dialogue[0].character;
            CheckDialoguesLength();
        }

        private void Update() {
            if (MetaGameController.IsMenuOpen) {
                return;
            }

            DisableDialogueIfOutOfCameraBounds();

            if (showInteractButtonBeforeInteract) {
                HandleInteractButtonBeforeInteract();
            }

            if (!PlayerIsClose) {
                return;
            }

            if (!GetInteractKey()) {
                return;
            }

            // Check if there is any important Dialogue open
            if (DialogueManager.Instance.DialoguePanel.activeInHierarchy) {
                ResetText();
                return;
            }

            // If there are multiple NPCs overlapping, only the closest one will interact
            if (!IsClosestNpc()) {
                ResetText();
                return;
            }

            ShowDialogue();
        }

        public void ShowDialogue(bool deactivate = true) {
            if (IsDialogueActive()) {
                NextLine();
            } else {
                if (deactivate) {
                    DeactivateAllActiveFloatingDialogues();
                }

                if (mustShowAlternativeDialogue) {
                    ShowAlternativeDialogue();
                    return;
                }

                _dialoguePanel.SetActive(true);
                SetTitle();
                if (_typingCoroutine != null) {
                    StopCoroutine(_typingCoroutine);
                }

                _typingCoroutine = StartTyping();
                mustShowAlternativeDialogue = HasAlternativeDialogue();
            }
        }

        public bool IsDialogueActive() {
            return _dialoguePanel.activeSelf;
        }

        private void SetTitle() {
            if (_dialogueTitle != null) {
                if (_dialogueCharacter == DialogueCharacter.Npc) {
                    _dialogueTitle.text = string.IsNullOrEmpty(title) ? gameObject.name : title;
                } else {
                    _dialogueTitle.text = _dialogueCharacter.ToString();
                }
            }
        }

        public void NextLine() {
            if (_dialogue.Length == 0) {
                return;
            }

            if (_index < _dialogue.Length - 1 && _dialogue[_index].text != "") {
                _index++;
                _dialogueText.text = "";
                if (_typingCoroutine != null) {
                    StopCoroutine(_typingCoroutine);
                }

                _typingCoroutine = StartTyping();
            } else {
                ResetText();
            }
        }

        private void ResetText() {
            if (_typingCoroutine != null) {
                StopCoroutine(_typingCoroutine);
            }

            if (_dialogueText != null) {
                _dialogueText.text = "";
            }

            _index = 0;
            if (_dialoguePanel != null) {
                _dialoguePanel.SetActive(false);
            }
        }

        private Coroutine StartTyping() {
            return StartCoroutine(Typing());
        }

        private IEnumerator Typing() {
            // If the current character changed update the title
            if (_dialogueCharacter != _dialogue[_index].character) {
                _dialogueCharacter = _dialogue[_index].character;
                SetTitle();
            }

            if (!progressiveTyping) {
                _dialogueText.text = _dialogue[_index].text;
                yield break;
            }

            foreach (char letter in _dialogue[_index].text) {
                _dialogueText.text += letter;
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
                DialogueLine[] defaultDialogue = {
                    new() { text = "No alternative dialogue", character = DialogueCharacter.Micca }
                };
                alternativeDialogueCa = defaultDialogue;
                alternativeDialogueEs = defaultDialogue;
                alternativeDialogueEn = defaultDialogue;
                alt = defaultDialogue;
            }

            return alt;
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
            _dialoguePanel.SetActive(true);

            if (resetAfterShowAlternativeDialogue) {
                StartCoroutine(ResetAfterDelay());
            }
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
            if (dialogueCa.Length != targetLength || dialogueEs.Length != targetLength || dialogueEn.Length != targetLength) {
                throw new Exception("The dialogues must have the same length for all languages.");
            }
        }

        private IEnumerator ResetAfterDelay(float delay = -1) {
            float effectiveDelay = delay < 0 ? resetDelay : delay;
            yield return new WaitForSeconds(effectiveDelay);
            // If it's still active, wait until it's not
            if (IsDialogueActive()) {
                StartCoroutine(ResetAfterDelay(0.1f));
                yield break;
            }

            ResetDialogue();
        }

        private void DisableDialogueIfOutOfCameraBounds() {
            if (IsDialogueActive()) {
                Vector3 viewportPos = Camera.main!.WorldToViewportPoint(_dialoguePanel.transform.position);
                if (viewportPos.x < 0 || viewportPos.x > 1 || viewportPos.y < 0 || viewportPos.y > 1) {
                    ResetText();
                }
            }
        }

        private bool IsClosestNpc() {
            FloatingDialogue[] floatingDialogues = FindObjectsOfType<FloatingDialogue>();
            float npcDistance = Vector3.Distance(transform.position, PlayerPosition);
            foreach (FloatingDialogue d in floatingDialogues) {
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

        private void DeactivateAllActiveFloatingDialogues() {
            FloatingDialogue[] dialogues = FindObjectsOfType<FloatingDialogue>();
            foreach (FloatingDialogue dialogue in dialogues) {
                if (dialogue != this && dialogue.IsDialogueActive()) {
                    dialogue._dialoguePanel.SetActive(false);
                }
            }
        }

        private void HandleInteractButtonBeforeInteract() {
            if (PlayerIsClose && !IsDialogueActive()) {
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
            mustShowAlternativeDialogue = false;
        }

        private void OnDrawGizmosSelected() {
            Gizmos.color = playerDistance.color;
            Gizmos.DrawWireSphere(transform.position, playerDistance.value);
        }
    }
}