using UnityEngine;
using UnityEngine.Serialization;

namespace Configuration {
    public class GlobalConfiguration : MonoBehaviour {
        public static GlobalConfiguration Instance { get; private set; }

        // If any the fields wants to do not be modified in the inspector,
        // use the readonly keyword. In case the field is a constant, use the const keyword.

        [Header("Debug Configuration")]
        public bool isDebugMode = false;
        public bool isGodMode = false;

        [Header("Screen Configuration")]
        // These fields are hided in the inspector because they are constants
        public const int DefaultScreenWidth = 1920;
        public const int DefaultScreenHeight = 1080;
        public const bool DefaultFullScreen = true;

        [Header("Physics Configuration")]
        public const float GravityScale = 1.0f;

        [Header("Audio Configuration")]
        public float masterVolume = 1.0f;
        public float musicVolume = 0.8f;
        public float sfxVolume = 0.7f;

        [Header("Player Configuration")]
        public float playerSpeedRun = 7.0f;
        public float playerSpeedWalk = 2.33f;
        public float playerSpeedCrouchWalk = 1.5f;
        public float playerJumpForce = 12.0f;
        public float playerSmallJumpForce = 6.0f;
        public float playerBigJumpForce = 12.0f;
        public float playerDoubleJumpForce = 6.0f;

        [Header("Player Health Configuration")]
        public int defaultHp = 100;
        public int maxHp = 100;
        public int defaultLives = 1;
        public int maxLives = 10;
        public const int DefaultHpIncrement = 1;
        public const int DefaultHpDecrement = 1;

        [Header("Character Sizes")]
        public float pacoSmallJumpHeightMultiplier = 1.5f;
        public float pacoAdultMiccaJumpHeightMultiplier = 2.0f;

        [Header("Enemy Configuration")]
        public float enemySpeed = 3.0f;
        public int enemyMaxHealth = 50;

        [Header("Idle Animations")]
        public float idleExtendedDelay1 = 6.0f;
        public float idleExtendedDuration1 = 3.0f;
        public float idleExtendedDelay2 = 18.0f;

        [Header("Crouch Configuration")]
        public float crouchSlideDuration = 1.5f;
        public float crouchSlideSpeed = 5.0f;

        [Header("Environment Configuration")]
        public float windSpeed = 2.0f;

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