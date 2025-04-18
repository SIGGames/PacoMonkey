using System;
using System.Collections;
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
        [SerializeField, Range(0f, 2f)] private float animationDuration = 0.25f;
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
            if (_activated && !canReopen) {
                return;
            }

            popupPrefab.SetActive(true);
            StartCoroutine(AnimatePopUp(true, animationDuration));
            _activated = true;

            SetUpTitleText();
            SetupIcons();
        }

        private void ClosePopUp() {
            if (popupPrefab == null || !_activated) {
                return;
            }

            StartCoroutine(AnimatePopUp(false, animationDuration));
        }

        private void SetUpTitleText() {
            TextMeshProUGUI titleText = popupPrefab.transform.Find("Title")?.GetComponent<TextMeshProUGUI>();
            if (titleText != null) {
                titleText.text = LocalizationManager.Instance.GetLocalizedText(titleTextKey);
            }
        }

        private IEnumerator AnimatePopUp(bool isOpening, float duration = 0.25f) {
            RectTransform rect = popupPrefab.GetComponent<RectTransform>();
            Vector3 startScale = rect.localScale;
            Vector3 endScale = isOpening ? Vector3.one : Vector3.zero;

            float timer = 0f;

            while (timer < duration) {
                float t = timer / duration;
                float scale = Mathf.SmoothStep(startScale.x, endScale.x, t);
                rect.localScale = new Vector3(scale, scale, 1f);
                timer += Time.deltaTime;
                yield return null;
            }

            rect.localScale = endScale;

            if (!isOpening) {
                ClearIcons();
                popupPrefab.SetActive(false);
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

        public void ResetZone() {
            _activated = false;
            ClearIcons();
            popupPrefab.SetActive(false);
        }
    }

    [Serializable]
    public struct TutorialActionBinding {
        public string keyboardInputActionName;
        public string gamepadInputActionName;
    }
}