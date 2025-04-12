using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using PlayerInputManager = PlayerInput.PlayerInputManager;

namespace UI.Rebinding_UI {
    public class SetKeyboardIcons : MonoBehaviour {
        protected void OnEnable() {
            RebindActionUI[] rebindUIComponents = transform.GetComponentsInChildren<RebindActionUI>();
            foreach (RebindActionUI component in rebindUIComponents) {
                component.updateBindingUIEvent.AddListener(OnUpdateBindingDisplay);
                component.UpdateBindingDisplay();
            }
        }

        private static void OnUpdateBindingDisplay(RebindActionUI component, string bindingDisplayString, string deviceLayoutName,
            string controlPath) {
            if (string.IsNullOrEmpty(deviceLayoutName) || string.IsNullOrEmpty(controlPath)) {
                UpdateCompositeKeyboardIcons(component);
                return;
            }

            if (PlayerInputManager.Instance == null) {
                return;
            }

            Sprite icon = PlayerInputManager.Instance.GetKeyboardSprite(controlPath);
            TextMeshProUGUI textComponent = component.BindingText;

            Transform container = textComponent.transform.parent.Find("ActionRebindIcons");
            if (container != null) {
                Transform imageGo = container.Find("ActionBindingIcon");
                if (imageGo == null) {
                    return;
                }

                Image imageComponent = imageGo.GetComponent<Image>();

                if (icon != null) {
                    textComponent.gameObject.SetActive(false);
                    imageComponent.sprite = icon;
                    imageComponent.gameObject.SetActive(true);
                } else {
                    textComponent.gameObject.SetActive(true);
                    imageComponent.gameObject.SetActive(false);
                }
            }
        }

        private static void UpdateCompositeKeyboardIcons(RebindActionUI component) {
            InputAction action = component.ActionReference.action;
            if (action == null) {
                return;
            }

            int compositeIndex = action.bindings.IndexOf(x => x.id.ToString() == component.BindingId);
            if (compositeIndex == -1) {
                return;
            }

            List<Image> icons = new();
            int iconIndex = 1;
            for (int i = compositeIndex + 1; i < action.bindings.Count && action.bindings[i].isPartOfComposite; i++) {
                // Getting the key of the composite part
                InputBinding part = action.bindings[i];
                string partKey = part.effectivePath;

                Sprite icon = null;
                if (PlayerInputManager.Instance != null) {
                    partKey = PlayerInputManager.GetCleanControlPath(partKey).ToLower();
                    icon = PlayerInputManager.Instance.GetKeyboardSprite(partKey);
                }

                // Getting the key image
                Transform parentTransform = component.BindingText.transform.parent;
                Transform container = parentTransform.Find("ActionRebindIcons");
                if (container != null) {
                    Transform childIconTransform = container.Find("ActionBindingIcon_" + iconIndex);
                    if (childIconTransform != null) {
                        Image childIcon = childIconTransform.GetComponent<Image>();
                        if (icon != null) {
                            childIcon.sprite = icon;
                            childIcon.gameObject.SetActive(true);
                            // Store if the icon is showing, so in case all icons are showing, we can hide the text
                            icons.Add(childIcon);
                        } else {
                            childIcon.gameObject.SetActive(false);
                        }
                    }
                }

                iconIndex++;
            }

            // Hide the text if all icons are showing
            if (icons.Count == iconIndex - 1) {
                component.BindingText.gameObject.SetActive(false);
            } else {
                // Hide all icons if the text is showing
                foreach (Image childIcon in icons) {
                    childIcon.gameObject.SetActive(false);
                }
            }
        }
    }
}