using UnityEngine;

namespace View {
    [ExecuteInEditMode]
    public class ParallaxCamera : MonoBehaviour {
        public delegate void ParallaxCameraDelegate(float deltaMovement);

        public ParallaxCameraDelegate onCameraTranslate;
        private float _oldPosition;

        private void Start() {
            _oldPosition = transform.position.x;
        }

        private void Update() {
            if (transform.position.x != _oldPosition) {
                if (onCameraTranslate != null) {
                    float delta = _oldPosition - transform.position.x;
                    onCameraTranslate(delta);
                }

                _oldPosition = transform.position.x;
            }
        }
    }
}