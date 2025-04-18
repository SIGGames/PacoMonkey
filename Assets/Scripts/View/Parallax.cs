using UnityEngine;

namespace View {
    public class Parallax : MonoBehaviour {
        [SerializeField, Range(0f, 1f)] private float lagAmount;

        private Vector3 _previousCameraPosition;
        private Transform _camera;
        private Vector3 _targetPosition;

        private float ParallaxAmount => 1f - lagAmount;

        private void Awake() {
            if (Camera.main != null) {
                _camera = Camera.main.transform;
            }

            _previousCameraPosition = _camera.position;
        }

        private void LateUpdate() {
            if (_camera == null) {
                return;
            }

            // + movement.y * ParallaxAmount
            Vector3 movement = CameraMovement;
            if (movement != Vector3.zero) {
                _targetPosition = new Vector3(transform.position.x + movement.x * ParallaxAmount, transform.position.y,
                    transform.position.z);
                transform.position = _targetPosition;
            }
        }

        private Vector3 CameraMovement {
            get {
                Vector3 movement = _camera.position - _previousCameraPosition;
                _previousCameraPosition = _camera.position;
                return movement;
            }
        }
    }
}