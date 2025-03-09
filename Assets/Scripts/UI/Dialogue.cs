using System.Collections;
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

        [Header("Components")]
        [SerializeField]
        private GameObject dialoguePanel;

        [SerializeField]
        private TextMeshProUGUI dialogueText;

        [SerializeField]
        private TextMeshProUGUI dialogueTitle;

        [SerializeField]
        private Image dialogueImage;

        [Header("Configuration")]
        [SerializeField]
        private string title;

        [SerializeField]
        private Sprite dialogueSprite;

        [SerializeField, Range(0.01f, 0.3f)]
        private float wordSpeed;

        [SerializeField]
        private string[] dialogue;

        [SerializeField, ColorRange(0.5f, 5)]
        private ColorRangeValue playerDistance = new(2, Color.black);

        [SerializeField]
        private bool freezePlayer;

        private static Vector3 PlayerPosition => CharacterManager.Instance.currentPlayerController.transform.position;
        private bool PlayerIsClose => Vector3.Distance(transform.position, PlayerPosition) < playerDistance.value;
        private int _index;
        private Coroutine _typingCoroutine;

        private void Start() {
            dialoguePanel.SetActive(false);
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

            if (dialoguePanel.activeInHierarchy) {
                NextLine();
            } else {
                dialoguePanel.SetActive(true);
                SetImage();
                SetTitle();
                if (_typingCoroutine != null) {
                    StopCoroutine(_typingCoroutine);
                }

                _typingCoroutine = StartCoroutine(Typing());
            }

            if (freezePlayer) {
                if (dialoguePanel.activeInHierarchy) {
                    CharacterManager.Instance.currentPlayerController.FreezePosition(true, true);
                } else {
                    CharacterManager.Instance.currentPlayerController.FreezePosition(false);
                }
            }
        }

        private void SetImage() {
            dialogueImage.sprite = dialogueSprite;
        }

        private void SetTitle() {
            dialogueTitle.text = string.IsNullOrEmpty(title) ? name : title;
        }

        private void NextLine() {
            if (_index < dialogue.Length - 1) {
                _index++;
                dialogueText.text = "";
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

            dialogueText.text = "";
            _index = 0;
            dialoguePanel.SetActive(false);
        }

        private IEnumerator Typing() {
            foreach (char letter in dialogue[_index]) {
                dialogueText.text += letter;
                yield return new WaitForSeconds(wordSpeed);
            }
        }

        private void OnDrawGizmosSelected() {
            Gizmos.color = playerDistance.color;
            Gizmos.DrawWireSphere(transform.position, playerDistance.value);
        }
    }
}