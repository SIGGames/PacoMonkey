using System.Collections;
using System.Linq;
using Controllers;
using Enums;
using Managers;
using TMPro;
using UnityEditor.ColorRangeDrawers;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using static PlayerInput.KeyBinds;
using Debug = System.Diagnostics.Debug;

namespace UI {
    public class Dialogue : MonoBehaviour {
        public DialogueType dialogueType = DialogueType.Fixed;

        // Components
        private GameObject _dialoguePanel;
        private TextMeshProUGUI _dialogueText;
        private TextMeshProUGUI _dialogueTitle;
        private Image _dialogueImage;

        [Header("Configuration")]
        [SerializeField]
        private string title;

        [SerializeField]
        private Sprite dialogueSprite;

        [SerializeField, Range(0.01f, 0.3f)]
        private float wordSpeed;

        [SerializeField, ColorRange(0.5f, 5)]
        private ColorRangeValue playerDistance = new(2, Color.black);

        [SerializeField]
        private bool freezePlayer;

        // Dialogues
        [SerializeField]
        private string[] dialogue;

        // Properties
        private static PlayerController PlayerController => CharacterManager.Instance.currentPlayerController;
        private static Vector3 PlayerPosition => PlayerController.transform.position;
        private bool PlayerIsClose => Vector3.Distance(transform.position, PlayerPosition) < playerDistance.value;
        private int _index;
        private Coroutine _typingCoroutine;

        // Components Titles
        private const string DialoguePanelTitle = "DialoguePanel";
        private const string DialogueTextTitle = "DialogueText";
        private const string DialogueTitleTitle = "DialogueTitle";
        private const string DialogueImageTitle = "DialogueImage";

        private void Awake() {
            _dialoguePanel = FindObjectsOfType<GameObject>(true)
                .FirstOrDefault(x => x.name == DialoguePanelTitle);

            Debug.Assert(_dialoguePanel != null, nameof(_dialoguePanel) + " != null");
            _dialogueText = _dialoguePanel.transform.Find(DialogueTextTitle)?.GetComponent<TextMeshProUGUI>();
            _dialogueTitle = _dialoguePanel.transform.Find(DialogueTitleTitle)?.GetComponent<TextMeshProUGUI>();
            _dialogueImage = _dialoguePanel.transform.Find(DialogueImageTitle)?.GetComponent<Image>();

            Debugger.LogIfNull((nameof(_dialoguePanel), _dialoguePanel), (nameof(_dialogueTitle), _dialogueTitle));
        }

        private void Start() {
            _dialoguePanel.SetActive(false);
            ResetText();
        }

        private void Update() {
            if (!PlayerIsClose) {
                ResetText();
                return;
            }

            if (!GetInteractKey()) {
                return;
            }

            if (_dialoguePanel.activeInHierarchy) {
                NextLine();
            } else {
                _dialoguePanel.SetActive(true);
                SetImage();
                SetTitle();
                if (_typingCoroutine != null) {
                    StopCoroutine(_typingCoroutine);
                }

                _typingCoroutine = StartCoroutine(Typing());
            }

            if (freezePlayer) {
                if (_dialoguePanel.activeInHierarchy) {
                    PlayerController.FreezePosition(true, true);
                } else {
                    PlayerController.FreezePosition(false);
                }
            }
        }

        private void SetImage() {
            _dialogueImage.sprite = dialogueSprite;
        }

        private void SetTitle() {
            _dialogueTitle.text = string.IsNullOrEmpty(title) ? name : title;
        }

        private void NextLine() {
            if (_index < dialogue.Length - 1) {
                _index++;
                _dialogueText.text = "";
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

            _dialogueText.text = "";
            _index = 0;
            _dialoguePanel.SetActive(false);
        }

        private IEnumerator Typing() {
            foreach (char letter in dialogue[_index]) {
                _dialogueText.text += letter;
                yield return new WaitForSeconds(wordSpeed);
            }
        }

        private void OnDrawGizmosSelected() {
            Gizmos.color = playerDistance.color;
            Gizmos.DrawWireSphere(transform.position, playerDistance.value);
        }
    }
}