using Managers;
using UnityEngine;

namespace UI {
    public class AudioUIConfiguration : MonoBehaviour {
        private AudioManager _audioManager;
        [SerializeField] private bool disableCarrets = true;

        private static bool _firstTime = true;

        private void Awake() {
            _audioManager = FindObjectOfType<AudioManager>();
        }

        private void OnEnable() {
            // On first time of load the scene, don't disable carrets since nothing has been set yet
            if (_firstTime) {
                _firstTime = false;
                return;
            }

            if (disableCarrets) {
                _audioManager.DisableCarrets();
            }
        }
    }
}