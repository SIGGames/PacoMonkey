using PlayerInput;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Rebinding_UI {
    public class SetGamepadIcons : MonoBehaviour {
        protected void OnEnable() {
            // Hook into all updateBindingUIEvents on all RebindActionUI components in our hierarchy.
            RebindActionUI[] rebindUIComponents = transform.GetComponentsInChildren<RebindActionUI>();
            foreach (var component in rebindUIComponents) {
                component.updateBindingUIEvent.AddListener(OnUpdateBindingDisplay);
                component.UpdateBindingDisplay();
            }
        }

        private static void OnUpdateBindingDisplay(RebindActionUI component, string bindingDisplayString, string deviceLayoutName,
            string controlPath) {
            if (string.IsNullOrEmpty(deviceLayoutName) || string.IsNullOrEmpty(controlPath))
                return;

            Sprite icon = PlayerInputManager.Instance.GetControllerSprite(controlPath);
            TextMeshProUGUI textComponent = component.BindingText;

            // Grab Image component.
            Transform imageGo = textComponent.transform.parent.Find("ActionBindingIcon");
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
}