using UnityEngine;
using UnityEngine.Serialization;

namespace Configuration {
    public class GlobalConfiguration : MonoBehaviour {
        public static GlobalConfiguration Instance { get; private set; }

        // If any the fields wants to do not be modified in the inspector,
        // use the readonly keyword. In case the field is a constant, use the const keyword.

        [Header("Screen Configuration")]
        // These fields are hided in the inspector because they are constants
        public const int DefaultScreenWidth = 1920;
        public const int DefaultScreenHeight = 1080;
        public const bool DefaultFullScreen = true;

        [Header("Player Configuration")]
        public float playerSpeed = 5.0f;
        public float playerJumpForce = 12.0f;
        public int playerMaxHealth = 100;

        [Header("Enemy Configuration")]
        public float enemySpeed = 3.0f;
        public int enemyMaxHealth = 50;

        [FormerlySerializedAs("DefaultHp")] [Header("Health Configuration")]
        // TODO: Refactor this to determine player and enemy health
        public int defaultHp = 100;

        public int maxHp = 100;
        public int defaultLives = 1;
        public int maxLives = 10;
        public const int DefaultHpIncrement = 1;
        public const int DefaultHpDecrement = 1;

        [Header("Physics Configuration")]
        public const float GravityScale = 1.0f;

        [Header("Audio Configuration")]
        public float masterVolume = 1.0f;
        public float musicVolume = 0.8f;
        public float sfxVolume = 0.7f;

        [Header("Environment Configuration")]
        public float windSpeed = 2.0f;

        [Header("Debug Configuration")]
        public bool isDebugMode = false;
        public bool isGodMode = false;

        private void Awake() {
            SetGlobalConfigInstance();
        }

        private void SetGlobalConfigInstance() {
            if (Instance == null) {
                Instance = this;
                // To make the object persist between scenes
                DontDestroyOnLoad(gameObject);
            }
            else {
                Destroy(gameObject);
            }
        }
    }
}