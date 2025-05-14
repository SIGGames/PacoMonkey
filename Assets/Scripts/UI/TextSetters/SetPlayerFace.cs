using UnityEngine;
using UnityEngine.UI;

namespace UI.TextSetters {
    public class SetPlayerFace : MonoBehaviour {
        public void UpdatePlayerFace(Sprite sprite) {
            Image image = GetComponent<Image>();

            if (sprite == null) {
                // If we dont have sprite for that character set the image to be invisible
                image.color = new Color(0, 0, 0, 0);
            }

            image.sprite = sprite;

            // Ensure the image do not gets deformed on different resolutions
            image.preserveAspect = true;
        }
    }
}