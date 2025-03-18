using UnityEngine;
using static Utils.PlayerPrefsKeys;

namespace Managers {
    public class PlayTimeManager : MonoBehaviour {
        public static PlayTimeManager Instance { get; private set; }
        public float PlayTime { get; private set; }

        [SerializeField, Range(1, 10)]
        private float saveInterval = 5f;

        [SerializeField]
        private bool keepIncrementingOnPause;

        private float _timeSinceLastSave;

        private void Awake() {
            if (Instance == null) {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            } else {
                Destroy(gameObject);
            }
        }

        private void Start() {
            PlayTime = PlayerPrefs.GetFloat(CurrentPlayTimeKey, 0);
            _timeSinceLastSave = 0f;
        }

        private void Update() {
            float deltaTime = keepIncrementingOnPause ? Time.unscaledDeltaTime : Time.deltaTime;
            PlayTime += deltaTime;
            _timeSinceLastSave += deltaTime;

            if (_timeSinceLastSave >= saveInterval) {
                PlayerPrefs.SetFloat(CurrentPlayTimeKey, PlayTime);
                PlayerPrefs.Save();
                _timeSinceLastSave = 0f;
            }
        }

        private void OnApplicationQuit() {
            PlayerPrefs.SetFloat(CurrentPlayTimeKey, PlayTime);
            PlayerPrefs.Save();
        }

        public void ResetPlayTime() {
            PlayTime = 0;
            PlayerPrefs.SetFloat(CurrentPlayTimeKey, PlayTime);
            PlayerPrefs.Save();
        }
    }
}