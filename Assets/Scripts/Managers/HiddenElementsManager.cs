using System.Collections;
using System.Collections.Generic;
using Controllers;
using Gameplay;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using static Utils.PlayerPrefsKeys;

namespace Managers {
    public class HiddenElementsManager : MonoBehaviour {
        public static HiddenElementsManager Instance { get; private set; }

        [Header("Settings"), Range(1, 10)]
        [SerializeField] private float timeToClosePanel = 5f;

        [SerializeField] private bool paintPlayerOnFinish = true;
        [SerializeField, ShowIf("paintPlayerOnFinish")] private Color playerColor;
        private bool _playerHasBeenPainted;

        [Header("Audio")]
        [SerializeField] private AudioClip onFinishAudio;
        public AudioClip onPickAudio;

        [Header("Components")]
        [SerializeField] private GameObject hiddenElementsPanel;
        [SerializeField] private TextMeshProUGUI hiddenElementsText;

        private readonly HashSet<string> _pickedHiddenElements = new();
        private int _hiddenElementsCount;

        private void Awake() {
            if (Instance == null) {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            } else {
                Destroy(gameObject);
            }

            FindHiddenElementsCount();
        }

        private void Start() {
            LoadPickedHiddenElements();

            if (HasAllHiddenElements()) {
                ShowPanel();
            }
        }

        public void RegisterHiddenElement(string id) {
            if (_pickedHiddenElements.Contains(id)) {
                return;
            }

            _pickedHiddenElements.Add(id);
            SavePickedHiddenElements();
            ShowPanel();
            PlayAudio();
        }

        public bool IsElementPicked(string id) {
            LoadPickedHiddenElements();
            return _pickedHiddenElements.Contains(id);
        }

        private void LoadPickedHiddenElements() {
            string saved = PlayerPrefs.GetString(HiddenElementsKey, "");
            if (!string.IsNullOrEmpty(saved)) {
                string[] ids = saved.Split(',');
                foreach (string id in ids) {
                    _pickedHiddenElements.Add(id);
                }
            }
        }

        private void SavePickedHiddenElements() {
            string data = string.Join(",", _pickedHiddenElements);
            PlayerPrefs.SetString(HiddenElementsKey, data);
            PlayerPrefs.Save();
        }

        private void ShowPanel() {
            hiddenElementsText.text = "[" + _pickedHiddenElements.Count + "/" + _hiddenElementsCount + "]";
            hiddenElementsPanel.SetActive(true);

            if (!HasAllHiddenElements()) {
                if (_playerHasBeenPainted) {
                    PaintPlayers(Color.white);
                    _playerHasBeenPainted = false;
                }

                StopAllCoroutines();
                StartCoroutine(HidePanel());
            } else {
                OnFinish();
            }
        }

        private IEnumerator HidePanel() {
            yield return new WaitForSeconds(timeToClosePanel);
            hiddenElementsPanel.SetActive(false);
        }

        public bool HasAllHiddenElements() {
            return _pickedHiddenElements.Count == _hiddenElementsCount;
        }

        private void OnFinish() {
            if (paintPlayerOnFinish) {
                PaintPlayers(playerColor);
            }
            CharacterManager.Instance.currentPlayerController.lives.ResetLives();
        }

        public void ResetHiddenElements() {
            _pickedHiddenElements.Clear();
            PlayerPrefs.DeleteKey(HiddenElementsKey);
            PlayerPrefs.Save();

            HiddenElement[] hiddenElements = FindObjectsOfType<HiddenElement>(true);
            foreach (HiddenElement hiddenElement in hiddenElements) {
                hiddenElement.ShowSprite(false);
            }
            ShowPanel();
            hiddenElementsPanel.SetActive(false);
        }

        private void FindHiddenElementsCount() {
            _hiddenElementsCount = FindObjectsOfType<HiddenElement>(true).Length;
        }

        private void PaintPlayers(Color color) {
            PlayerController[] players = FindObjectsOfType<PlayerController>(true);
            foreach (PlayerController player in players) {
                if (player == null) {
                    continue;
                }

                SpriteRenderer spriteRenderer = player.gameObject.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null) {
                    spriteRenderer.color = color;
                    _playerHasBeenPainted = true;
                }
            }
        }

        private void PlayAudio() {
            if (HasAllHiddenElements()) {
                if (onFinishAudio != null) {
                    AudioManager.Instance.PlayMusic(MusicType.Game, MusicSoundType.Victory);
                }
            }
        }
    }
}