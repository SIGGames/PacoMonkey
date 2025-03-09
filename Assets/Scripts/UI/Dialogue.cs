using System;
using System.Collections;
using Controllers;
using Enums;
using Managers;
using TMPro;
using UnityEditor.ColorRangeDrawers;
using UnityEngine;
using UnityEngine.UI;
using static PlayerInput.KeyBinds;

namespace UI {
    public class Dialogue : MonoBehaviour {
        public DialogueType dialogueType = DialogueType.Fixed;

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

        [SerializeField, Range(0.01f, 0.3f)]
        private float wordSpeed;

        [SerializeField, ColorRange(0.5f, 5)]
        private ColorRangeValue playerDistance = new(2, Color.black);

        private bool FreezePlayer => dialogueType == DialogueType.Fixed;

        [SerializeField]
        private bool ensureMultipleLanguagesDialoguesLength = true;

        [Header("Dialogues")]
        [SerializeField] private string[] dialogueCa;
        [SerializeField] private string[] dialogueES;
        [SerializeField] private string[] dialogueEn;
        private string[] _dialogue;

        // Properties
        private static PlayerController PlayerController => CharacterManager.Instance.currentPlayerController;
        private static Vector3 PlayerPosition => PlayerController.transform.position;
        private bool PlayerIsClose => Vector3.Distance(transform.position, PlayerPosition) < playerDistance.value;
        private int _index;

        // Unique identifier for each NPC
        private string _npcGuid;
        private static string _currentNpcGuid = "";
        private Coroutine _typingCoroutine;

        private void Awake() {
            _npcGuid = Guid.NewGuid().ToString();
        }

        private void Start() {
            DialoguePanel.SetActive(false);
            ResetText();
            _dialogue = GetCurrentDialogue();
            CheckDialoguesLength();
        }

        private void Update() {
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
            _dialogue = GetCurrentDialogue();

            if (DialoguePanel.activeInHierarchy) {
                NextLine();
            } else {
                DialoguePanel.SetActive(true);
                SetImage();
                SetTitle();
                if (_typingCoroutine != null) {
                    StopCoroutine(_typingCoroutine);
                }

                _typingCoroutine = StartCoroutine(Typing());
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
                if (d == this) continue;
                float candidateDistance = Vector3.Distance(d.transform.position, PlayerPosition);
                if (candidateDistance < npcDistance) {
                    return false;
                }
            }

            return true;
        }

        private void SetImage() {
            DialogueImage.sprite = dialogueSprite;
        }

        private void SetTitle() {
            DialogueTitle.text = string.IsNullOrEmpty(title) ? name : title;
        }

        private void NextLine() {
            if (_dialogue.Length == 0) {
                return;
            }

            if (_index < _dialogue.Length - 1) {
                _index++;
                DialogueText.text = "";
                if (_typingCoroutine != null) {
                    StopCoroutine(_typingCoroutine);
                }

                _typingCoroutine = StartCoroutine(Typing());
            } else {
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

        private IEnumerator Typing() {
            foreach (char letter in _dialogue[_index]) {
                DialogueText.text += letter;
                yield return new WaitForSeconds(wordSpeed);
            }
        }

        private string[] GetCurrentDialogue() {
            string[] selectedDialogue = GameController.Instance.currentLanguage switch {
                Languages.Catalan => dialogueCa,
                Languages.Spanish => dialogueES,
                Languages.English => dialogueEn,
                _ => throw new ArgumentOutOfRangeException()
            };

            if (selectedDialogue == null || selectedDialogue.Length == 0) {
                string[] defaultDialogue = { "No dialogue" };
                dialogueCa = defaultDialogue;
                dialogueES = defaultDialogue;
                dialogueEn = defaultDialogue;
                selectedDialogue = defaultDialogue;
            }

            return selectedDialogue;
        }

        private void CheckDialoguesLength() {
            if (!ensureMultipleLanguagesDialoguesLength) {
                return;
            }

            int targetLength = _dialogue.Length;
            if (dialogueCa.Length != targetLength || dialogueES.Length != targetLength ||
                dialogueEn.Length != targetLength) {
                throw new Exception("Dialogues length must be the same");
            }
        }

        private void OnDrawGizmosSelected() {
            Gizmos.color = playerDistance.color;
            Gizmos.DrawWireSphere(transform.position, playerDistance.value);
        }
    }
}