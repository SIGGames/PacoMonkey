using PlayerInput;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
                return;
            }

            Sprite icon = PlayerInputManager.Instance.GetInputSprite(controlPath);
            TextMeshProUGUI textComponent = component.BindingText;

            Transform imageGo = textComponent.transform.parent.Find("ActionBindingIcon");
            if (imageGo == null)
                return;
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