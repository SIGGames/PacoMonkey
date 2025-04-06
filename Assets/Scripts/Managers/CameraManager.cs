using System.Collections;
using Cinemachine;
using UnityEngine;

namespace Managers {
    public class CameraManager : MonoBehaviour {
        public static CameraManager Instance { get; private set; }

        private CinemachineVirtualCamera _virtualCamera;
        [SerializeField] private GameObject camera1;
        [SerializeField] private GameObject camera2;
        private CinemachineFramingTransposer _framingTransposer;
        private float _initialOrthographicSize;
        private Vector3 _initialOffset;
        private Vector3 _initialCameraPosition;
        private bool _isFramingTransposerNotNull;
        private bool _isVirtualCameraNotNull;

        private void Awake() {
            InitializeSingleton();
            InitializeCameraComponents();
            SaveInitialCameraSettings();
        }

        private void Start() {
            _isVirtualCameraNotNull = _virtualCamera != null;
            _isFramingTransposerNotNull = _framingTransposer != null;

            // This is the main one
            camera1.SetActive(true);
        }

        private void InitializeSingleton() {
            if (Instance == null) {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            } else {
                Destroy(gameObject);
            }
        }

        private void InitializeCameraComponents() {
            _virtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
            if (_virtualCamera == null) {
                Debug.LogError("No virtual camera found in the scene");
                return;
            }

            _framingTransposer = _virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
            if (_framingTransposer == null) {
                Debug.LogError("No framing transposer found in the virtual camera");
            }
        }

        private void SaveInitialCameraSettings() {
            if (_virtualCamera != null && _framingTransposer != null) {
                _initialOrthographicSize = _virtualCamera.m_Lens.OrthographicSize;
                _initialOffset = _framingTransposer.m_TrackedObjectOffset;
                _initialCameraPosition = _virtualCamera.transform.position;
            }
        }

        public void FollowAndLookAt(Transform target) {
            CinemachineVirtualCamera virtualCamera1 = camera1.GetComponent<CinemachineVirtualCamera>();
            if (virtualCamera1 != null) {
                virtualCamera1.Follow = target;
                virtualCamera1.LookAt = target;
            }

            CinemachineVirtualCamera virtualCamera2 = camera2.GetComponent<CinemachineVirtualCamera>();
            if (virtualCamera2 != null) {
                virtualCamera2.Follow = target;
                virtualCamera2.LookAt = target;
            }
        }

        public void SetProgressiveZoom(float duration) {
            camera1.SetActive(false);
            camera2.SetActive(true);

            Invoke(nameof(ResetZoom), duration);
        }

        private void ResetZoom() {
            camera1.SetActive(true);
            camera2.SetActive(false);
        }

        public void SetOffset(Vector2 offset) {
            if (_isFramingTransposerNotNull) {
                _framingTransposer.m_TrackedObjectOffset =
                    new Vector3(offset.x, offset.y, _framingTransposer.m_TrackedObjectOffset.z);
            }
        }

        public void ResetCamera() {
            if (_isVirtualCameraNotNull && _isFramingTransposerNotNull) {
                _virtualCamera.m_Lens.OrthographicSize = _initialOrthographicSize;
                _framingTransposer.m_TrackedObjectOffset = _initialOffset;
                _virtualCamera.transform.position = _initialCameraPosition;
            }
        }

        public void ShakeCamera(float amplitude, float frequency, float duration) {
            var noise = _virtualCamera?.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            if (noise != null) {
                noise.m_AmplitudeGain = amplitude;
                noise.m_FrequencyGain = frequency;
                Invoke(nameof(StopShake), duration);
            }
        }

        private void StopShake() {
            var noise = _virtualCamera?.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            if (noise != null) {
                noise.m_AmplitudeGain = 0;
                noise.m_FrequencyGain = 0;
            }
        }
    }
}