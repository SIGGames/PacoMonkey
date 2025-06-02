using System;
using System.Collections.Generic;
using Enums;
using Managers;
using PlayerInput;
using UnityEngine;
using static Utils.TagUtils;

namespace Zones {
    public class ShowTutorialKeysZone : MonoBehaviour {
        [SerializeField] private bool canReopen;
        [SerializeField, Range(0f, 5f)] private float closeDelay = 2f;
        [SerializeField] private bool requireActiveQuestToShow;
        [SerializeField] private List<TutorialActionBinding> actionsToShow;

        [Header("Components")]
        [SerializeField] private string titleTextKey;

        private bool _activated;

        private void OnTriggerEnter2D(Collider2D col) {
            if (!col.CompareTag(Player)) {
                return;
            }

            TryOpenPopUp();
        }

        private void OnTriggerExit2D(Collider2D col) {
            if (!col.CompareTag(Player)) {
                return;
            }

            PopUpManager.Instance.ClosePopUp(closeDelay);
        }

        private void TryOpenPopUp() {
            if (_activated && !canReopen) {
                return;
            }

            if (requireActiveQuestToShow && QuestManager.Instance.GetActiveQuest() == null) {
                return;
            }

            _activated = true;

            List<Sprite> sprites = new();
            foreach (TutorialActionBinding action in actionsToShow) {
                string controlPath = GetControlPath(action);
                Sprite icon = PlayerInputManager.Instance.GetInputSprite(controlPath);
                if (icon != null) {
                    sprites.Add(icon);
                }
            }

            PopUpManager.Instance?.OpenPopUp(titleTextKey, sprites);
        }

        private static string GetControlPath(TutorialActionBinding action) {
            return PlayerInputManager.Instance.currentInputDevice == InputDeviceType.Controller
                ? action.gamepadInputActionName
                : action.keyboardInputActionName;
        }

        public void ResetZone() {
            _activated = false;
            if (PopUpManager.Instance != null) {
                PopUpManager.Instance.ResetPopUp();
            }
        }
    }

    [Serializable]
    public struct TutorialActionBinding {
        public string keyboardInputActionName;
        public string gamepadInputActionName;
    }
}