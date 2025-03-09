using System;
using System.Collections;
using Controllers;
using Enums;
using Managers;
using TMPro;
using UnityEditor.ColorRangeDrawers;
using UnityEngine;
using Utils;
using static PlayerInput.KeyBinds;

namespace UI {
    public class FloatingDialogue : MonoBehaviour {
        // Components
        private GameObject _dialoguePanel;
        private TextMeshProUGUI _dialogueText;
        private TextMeshProUGUI _dialogueTitle;

        [Header("Configuration")]
        [SerializeField]
        private string title;

        [SerializeField, Range(0.01f, 0.3f)]
        private float wordSpeed = 0.1f;

        [SerializeField, ColorRange(0.5f, 5)]
        private ColorRangeValue playerDistance = new(2, Color.black);

        [Header("Dialogues")]
        [SerializeField] private string[] dialogueCa;

        [SerializeField] private string[] dialogueEs;
        [SerializeField] private string[] dialogueEn;
        private string[] _dialogue;

        private int _index;
        private Coroutine _typingCoroutine;

        private static PlayerController PlayerController => CharacterManager.Instance.currentPlayerController;
        private static Vector3 PlayerPosition => PlayerController.transform.position;
        private bool PlayerIsClose => TargetExists && Vector3.Distance(transform.position, PlayerPosition) < playerDistance.value;

        private static bool TargetExists => PlayerController != null;

        private const string DialoguePanelIdentifier = "FloatingDialoguePanel";
        private const string DialogueTextIdentifier = "FloatingDialogueText";
        private const string DialogueTitleIdentifier = "FloatingDialogueTitle";

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
            CheckDialoguesLength();
        }

        private void Update() {
            if (!PlayerIsClose) {
                ResetText();
                return;
            }

            if (!GetInteractKey()) {
                return;
            }

            // If there are multiple NPCs overlapping, only the closest one will interact
            if (!IsClosestNpc()) {
                ResetText();
                return;
            }

            if (_dialoguePanel.activeSelf) {
                NextLine();
            } else {
                _dialoguePanel.SetActive(true);
                SetTitle();
                if (_typingCoroutine != null)
                    StopCoroutine(_typingCoroutine);
                _typingCoroutine = StartCoroutine(Typing());
            }
        }

        private bool IsClosestNpc() {
            FloatingDialogue[] dialogues = FindObjectsOfType<FloatingDialogue>();
            float npcDistance = Vector3.Distance(transform.position, PlayerPosition);
            foreach (FloatingDialogue d in dialogues) {
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

        private void SetTitle() {
            if (_dialogueTitle != null)
                _dialogueTitle.text = string.IsNullOrEmpty(title) ? gameObject.name : title;
        }

        private void NextLine() {
            if (_dialogue.Length == 0)
                return;

            if (_index < _dialogue.Length - 1) {
                _index++;
                _dialogueText.text = "";
                if (_typingCoroutine != null)
                    StopCoroutine(_typingCoroutine);
                _typingCoroutine = StartCoroutine(Typing());
            } else {
                ResetText();
            }
        }

        private void ResetText() {
            if (_typingCoroutine != null)
                StopCoroutine(_typingCoroutine);
            if (_dialogueText != null)
                _dialogueText.text = "";
            _index = 0;
            _dialoguePanel.SetActive(false);
        }

        private IEnumerator Typing() {
            foreach (char letter in _dialogue[_index]) {
                _dialogueText.text += letter;
                yield return new WaitForSeconds(wordSpeed);
            }
        }

        private string[] GetCurrentDialogue() {
            string[] selectedDialogue = GameController.Instance.currentLanguage switch {
                Languages.Catalan => dialogueCa,
                Languages.Spanish => dialogueEs,
                Languages.English => dialogueEn,
                _ => throw new ArgumentOutOfRangeException()
            };

            if (selectedDialogue == null || selectedDialogue.Length == 0) {
                string[] defaultDialogue = { "No dialogue" };
                dialogueCa = defaultDialogue;
                dialogueEs = defaultDialogue;
                dialogueEn = defaultDialogue;
                selectedDialogue = defaultDialogue;
            }

            return selectedDialogue;
        }

        private void CheckDialoguesLength() {
            int targetLength = _dialogue.Length;
            if (dialogueCa.Length != targetLength || dialogueEs.Length != targetLength || dialogueEn.Length != targetLength) {
                throw new Exception("The dialogues must have the same length for all languages.");
            }
        }

        private void OnDrawGizmosSelected() {
            Gizmos.color = playerDistance.color;
            Gizmos.DrawWireSphere(transform.position, playerDistance.value);
        }
    }
}