using System;
using System.Collections;
using System.Collections.Generic;
using Gameplay;
using TMPro;
using UnityEngine;
using static Utils.PlayerPrefsKeys;

namespace Managers {
    public class HiddenElementsManager : MonoBehaviour {
        public static HiddenElementsManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private float timeToClosePanel = 5f;

        [Header("Components")]
        [SerializeField] private GameObject hiddenElementsPanel;
        [SerializeField] private TextMeshProUGUI hiddenElementsText;

        private HashSet<string> _pickedHiddenElements = new();
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
        }

        private void Update() {
            if (Input.GetKeyDown(KeyCode.F2)) {
                RegisterHiddenElement(Guid.NewGuid().ToString());
            }

            if (Input.GetKeyDown(KeyCode.F3)) {
                ResetHiddenElements();
            }
        }

        public void RegisterHiddenElement(string id) {
            if (_pickedHiddenElements.Contains(id)) {
                return;
            }

            _pickedHiddenElements.Add(id);
            SavePickedHiddenElements();
            ShowPanel();

            // TODO: Al register si obtens totes fer un audio especial, si es nomes una fer un audio normal
        }

        public bool IsElementPicked(string id) {
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
            StopAllCoroutines();
            StartCoroutine(HidePanel());
        }

        private IEnumerator HidePanel() {
            yield return new WaitForSeconds(timeToClosePanel);
            hiddenElementsPanel.SetActive(false);
        }

        public void ResetHiddenElements() {
            _pickedHiddenElements.Clear();
            PlayerPrefs.DeleteKey(HiddenElementsKey);
            PlayerPrefs.Save();

            HiddenElement[] hiddenElements = FindObjectsOfType<HiddenElement>(true);
            foreach (HiddenElement hiddenElement in hiddenElements) {
                hiddenElement.ShowSprite(false);
            }
        }

        private void FindHiddenElementsCount() {
            _hiddenElementsCount = FindObjectsOfType<HiddenElement>(true).Length;
        }
    }
}