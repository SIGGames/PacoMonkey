using System;
using System.Collections.Generic;
using System.Linq;
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
        [SerializeField] private List<InGameInputAction> actionsToListen;

        [Header("Components")]
        [SerializeField] private string titleTextKey;
        [SerializeField] private Transform keysContainer;
        [SerializeField] private GameObject popupPrefab;
        [SerializeField] private GameObject iconPrefab;

        private readonly Dictionary<InGameInputAction, List<InputAction>> _listenedActions = new();
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
            if (_activated && !canReopen) {
                return;
            }

            popupPrefab.SetActive(true);
            _active = true;
            _activated = true;

            SetUpTitleText();
            SetupIcons();
            EnableActionListeners();
        }

        private void SetUpTitleText() {
            TextMeshProUGUI titleText = popupPrefab.transform.Find("Title").GetComponent<TextMeshProUGUI>();
            if (titleText != null) {
                titleText.text = LocalizationManager.Instance.GetLocalizedText(titleTextKey);
            }
        }

        private void ClosePopUp() {
            popupPrefab.SetActive(false);
            _active = false;

            DisableActionListeners();
            ClearIcons();
        }

        private void SetupIcons() {
            ClearIcons();
            foreach (InGameInputAction inGameInputAction in actionsToListen) {
                List<(InputAction action, string part)> actionBindings = GetActions(inGameInputAction);
                if (actionBindings == null || actionBindings.Count == 0) continue;

                _listenedActions[inGameInputAction] = actionBindings.Select(ab => ab.action).ToList();

                foreach ((var action, string part) in actionBindings) {
                    GameObject iconObj = Instantiate(iconPrefab, keysContainer);
                    iconObj.name = $"Icon_{inGameInputAction}_{part ?? "Main"}";

                    string controlPath = part == null
                        ? PlayerInputManager.Instance.GetActiveBindingControlPath(action)
                        : GetCompositeBindingControlPath(action, part);

                    Sprite sprite = PlayerInputManager.Instance.GetInputSprite(controlPath);
                    if (sprite != null) {
                        iconObj.GetComponentInChildren<Image>().sprite = sprite;
                    }
                }
            }
        }

        private static string GetCompositeBindingControlPath(InputAction action, string partName) {
            foreach (var binding in action.bindings.Where(binding =>
                         binding.isPartOfComposite &&
                         string.Equals(binding.name, partName, StringComparison.CurrentCultureIgnoreCase))) {
                return binding.effectivePath;
            }

            return "";
        }

        private static List<(InputAction action, string part)> GetActions(InGameInputAction input) {
            List<(InputAction, string)> result = new();

            string name = input.ToString();

            if (name.StartsWith("Secondary")) {
                string raw = name.Replace("Secondary", "");
                string[] directions = { "Left", "Right", "Up", "Down" };

                foreach (string dir in directions) {
                    if (raw.EndsWith(dir)) {
                        string baseActionName = raw[..^dir.Length];
                        InputAction baseAction = PlayerInputManager.Instance.InputActions.FindAction(baseActionName);
                        if (baseAction != null) {
                            result.Add((baseAction, dir.ToLower()));
                        }

                        break;
                    }
                }
            } else {
                InputAction action = PlayerInputManager.Instance.InputActions.FindAction(name);
                if (action != null) {
                    result.Add((action, null));
                }
            }

            return result;
        }

        private void ClearIcons() {
            foreach (Transform child in keysContainer.transform) {
                Destroy(child.gameObject);
            }

            _listenedActions.Clear();
        }

        private void EnableActionListeners() {
            foreach (InputAction action in _listenedActions.SelectMany(pair => pair.Value)) {
                action.performed += OnAnyActionPerformed;
            }
        }

        private void DisableActionListeners() {
            foreach (InputAction action in _listenedActions.SelectMany(pair => pair.Value)) {
                action.performed -= OnAnyActionPerformed;
            }
        }

        private void OnAnyActionPerformed(InputAction.CallbackContext ctx) {
            if (!_active) {
                return;
            }

            popupPrefab.SetActive(false);
            DisableActionListeners();
        }
    }
}