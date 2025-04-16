using System;
using System.Collections.Generic;
using Enums;
using Localization;
using PlayerInput;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Utils.TagUtils;

namespace Zones {
    public class ShowTutorialKeysZone : MonoBehaviour {
        [SerializeField] private bool canReopen;
        [SerializeField] private List<TutorialActionBinding> actionsToShow;

        [Header("Components")]
        [SerializeField] private string titleTextKey;
        [SerializeField] private Transform keysContainer;
        [SerializeField] private GameObject popupPrefab;
        [SerializeField] private GameObject iconPrefab;

        private bool _activated;

        private void OnTriggerEnter2D(Collider2D col) {
            if (!col.CompareTag(Player)) {
                return;
            }

            OpenPopUp();
        }

        private void OnTriggerExit2D(Collider2D col) {
            if (!col.CompareTag(Player)) {
                return;
            }

            ClosePopUp();
        }

        private void OpenPopUp() {
            if (_activated && !canReopen) return;

            popupPrefab.SetActive(true);
            _activated = true;

            SetUpTitleText();
            SetupIcons();
        }

        private void ClosePopUp() {
            if (popupPrefab == null) {
                return;
            }

            popupPrefab.SetActive(false);
            ClearIcons();
        }

        private void SetUpTitleText() {
            TextMeshProUGUI titleText = popupPrefab.transform.Find("Title")?.GetComponent<TextMeshProUGUI>();
            if (titleText != null) {
                titleText.text = LocalizationManager.Instance.GetLocalizedText(titleTextKey);
            }
        }

        private void SetupIcons() {
            ClearIcons();

            foreach (TutorialActionBinding tutorialAction in actionsToShow) {
                string controlPath = GetControlPath(tutorialAction);
                Sprite sprite = PlayerInputManager.Instance.GetInputSprite(controlPath);
                if (sprite != null) {
                    GameObject iconObj = Instantiate(iconPrefab, keysContainer);
                    iconObj.name = $"Icon_{titleTextKey}_{controlPath}";
                    iconObj.GetComponentInChildren<Image>().sprite = sprite;
                }
            }
        }

        private static string GetControlPath(TutorialActionBinding tutorialAction) {
            return PlayerInputManager.Instance.currentInputDevice == InputDeviceType.Controller
                ? tutorialAction.gamepadInputActionName
                : tutorialAction.keyboardInputActionName;
        }

        private void ClearIcons() {
            foreach (Transform child in keysContainer) {
                Destroy(child.gameObject);
            }
        }
    }

    [Serializable]
    public struct TutorialActionBinding {
        public string keyboardInputActionName;
        public string gamepadInputActionName;
    }
}