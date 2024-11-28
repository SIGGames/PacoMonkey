using UnityEngine;
using UnityEngine.UI;


namespace Health.UI {
    public class HealthHeart : MonoBehaviour {
        public Sprite fullHeart, halfHeart, emptyHeart;
        private Image _heartImage;

        private void Awake() {
            _heartImage = GetComponent<Image>();
        }

        public void SetHeartImage(HeartState state) {
            switch (state) {
                case HeartState.Empty:
                    _heartImage.sprite = emptyHeart;
                    break;
                case HeartState.Half:
                    _heartImage.sprite = halfHeart;
                    break;
                case HeartState.Full:
                    _heartImage.sprite = fullHeart;
                    break;
            }
        }
    }

    public enum HeartState {
        Empty = 0,
        Half = 1,
        Full = 2
    }
}