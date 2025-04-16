using System;
using System.Collections.Generic;
using Enums;
using Localization;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static Utils.TagUtils;
using PlayerInputManager = PlayerInput.PlayerInputManager;

namespace Zones {
    public class ShowTutorialKeysZone : MonoBehaviour {
        [SerializeField] private bool canReopen;
        [SerializeField] private List<TutorialActionBinding> actionsToShow;

        [Header("Components")]
        [SerializeField] private string titleTextKey;
        [SerializeField] private Transform keysContainer;
        [SerializeField] private GameObject popupPrefab;
        [SerializeField] private GameObject iconPrefab;

        private readonly List<InputAction> _listenedActions = new();
        private bool _active;
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
            _active = true;
            _activated = true;

            SetUpTitleText();
            SetupIcons();
            EnableActionListeners();
        }

        private void ClosePopUp() {
            if (popupPrefab == null) {
                return;
            }

            popupPrefab.SetActive(false);
            _active = false;

            DisableActionListeners();
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
                InputAction action = tutorialAction.actionReference.action;
                if (action == null) {
                    continue;
                }
                _listenedActions.Add(action);

                string controlPath = GetControlPath(tutorialAction);
                Sprite sprite = PlayerInputManager.Instance.GetInputSprite(controlPath);
                if (sprite != null) {
                    GameObject iconObj = Instantiate(iconPrefab, keysContainer);
                    iconObj.name = $"Icon_{action.name}_{controlPath}";
                    iconObj.GetComponentInChildren<Image>().sprite = sprite;
                }
            }
        }

        private static string GetControlPath(TutorialActionBinding tutorialAction) {
            return PlayerInputManager.Instance.currentInputDevice == InputDeviceType.Controller
                ? tutorialAction.gamepadInputActionName
                : tutorialAction.keyBoardInputActionName;
        }

        private void ClearIcons() {
            foreach (Transform child in keysContainer) {
                Destroy(child.gameObject);
            }

            _listenedActions.Clear();
        }

        private void EnableActionListeners() {
            foreach (var action in _listenedActions) {
                action.performed += OnAnyActionPerformed;
            }
        }

        private void DisableActionListeners() {
            foreach (var action in _listenedActions) {
                action.performed -= OnAnyActionPerformed;
            }
        }

        private void OnAnyActionPerformed(InputAction.CallbackContext ctx) {
            if (!_active) return;

            popupPrefab.SetActive(false);
            DisableActionListeners();
        }
    }

    [Serializable]
    public struct TutorialActionBinding {
        public InputActionReference actionReference;
        public string keyBoardInputActionName;
        public string gamepadInputActionName;
    }
}