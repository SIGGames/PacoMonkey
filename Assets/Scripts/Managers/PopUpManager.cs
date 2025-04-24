using System.Collections;
using System.Collections.Generic;
using Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace Managers {
    public class PopUpManager : MonoBehaviour {
        public static PopUpManager Instance { get; private set; }

        [Header("Components")]
        [SerializeField] private GameObject popupGameObject;
        [SerializeField] private Transform keysContainer;
        [SerializeField] private GameObject keyIconPrefab;

        private TextMeshProUGUI _titleText;
        private RectTransform _popupRectTransform;

        [Header("Settings")]
        [SerializeField, Range(0f, 2f)] private float animationDuration = 0.25f;

        private Coroutine _closeRoutine;
        private bool _isOpen;

        private void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            _titleText = popupGameObject.transform.Find("Title")?.GetComponent<TextMeshProUGUI>();
            _popupRectTransform = popupGameObject.GetComponent<RectTransform>();

            if (popupGameObject == null || keysContainer == null || _titleText == null || keyIconPrefab == null) {
                Debugger.LogIfNull((nameof(popupGameObject), popupGameObject), (nameof(keysContainer),
                    keysContainer), (nameof(_titleText), _titleText), (nameof(keyIconPrefab), keyIconPrefab));
                enabled = false;
            }

            popupGameObject.SetActive(false);
        }

        public void OpenPopUp(string titleKey, List<Sprite> icons) {
            if (_isOpen) {
                ResetPopUp();
            }

            popupGameObject.SetActive(true);
            _isOpen = true;
            SetTitle(titleKey);
            SetIcons(titleKey, icons);
            StartCoroutine(AnimatePopUp(true, animationDuration));
        }

        public void ClosePopUp(float closeDelay = 0f) {
            if (!_isOpen || popupGameObject == null) {
                return;
            }

            if (_closeRoutine != null) {
                StopCoroutine(_closeRoutine);
            }

            _closeRoutine = StartCoroutine(ClosePopUpRoutine(closeDelay));
        }

        public void ResetPopUp() {
            _isOpen = false;
            ClearIcons();
            popupGameObject.SetActive(false);
            if (_closeRoutine != null) {
                StopCoroutine(_closeRoutine);
            }
        }

        private void SetTitle(string key) {
            _titleText.text = LocalizationManager.Instance.GetLocalizedText(key);
        }

        private void SetIcons(string titleKey, List<Sprite> icons) {
            ClearIcons();
            foreach (Sprite sprite in icons) {
                GameObject icon = Instantiate(keyIconPrefab, keysContainer);
                icon.name = $"Icon_{titleKey}";
                icon.transform.SetParent(keysContainer, false);
                icon.GetComponent<Image>().sprite = sprite;
            }
        }

        private void ClearIcons() {
            foreach (Transform icon in keysContainer) {
                Destroy(icon.gameObject);
            }
        }

        private IEnumerator ClosePopUpRoutine(float closeDelay) {
            yield return new WaitForSeconds(closeDelay);


            yield return AnimatePopUp(false, animationDuration);
            ResetPopUp();
        }

        private IEnumerator AnimatePopUp(bool opening, float duration) {
            if (_popupRectTransform == null) {
                yield break;
            }

            Vector3 start = _popupRectTransform.localScale;
            Vector3 end = opening ? Vector3.one : Vector3.zero;
            float timer = 0f;

            while (timer < duration) {
                float t = timer / duration;
                float scale = Mathf.SmoothStep(start.x, end.x, t);
                _popupRectTransform.localScale = new Vector3(scale, scale, 1f);
                timer += Time.deltaTime;
                yield return null;
            }

            _popupRectTransform.localScale = end;
        }
    }
}