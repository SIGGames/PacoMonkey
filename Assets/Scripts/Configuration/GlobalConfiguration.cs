using UnityEngine;
using UnityEngine.Serialization;

namespace Configuration {
    public class GlobalConfiguration : MonoBehaviour {
        [System.Serializable]
        public struct AudioConfig {
            [Range(0, 100)]
            [Tooltip("Master volume control")]
            public int masterVolume;
            [Range(0, 100)]
            [Tooltip("Music volume control")]
            public int musicVolume;
            [Range(0, 100)]
            [Tooltip("Sound effects volume control")]
            public int sfxVolume;
        }

        [System.Serializable]
        public struct PlayerConfig {
            [Tooltip("Player running speed")]
            public const float MaxRunSpeed = 4f;
            [Tooltip("Player running acceleration")]
            public const float RunAcceleration = 75f;
            [Tooltip("Player running deceleration")]
            public const float RunDeceleration = 70f;
            [Tooltip("Player walking speed")]
            public float speedWalk;
            [Tooltip("Player crouching speed")]
            public float speedCrouchWalk;
            [Tooltip("Player jump force")]
            public float jumpForce;
            [Tooltip("Small jump force")]
            public float smallJumpForce;
            [Tooltip("Big jump force")]
            public float bigJumpForce;
            [Tooltip("Double jump force")]
            public float doubleJumpForce;
            [Tooltip(("Climbing speed"))]
            public const float ClimbingSpeed = 8f;
        }

        [System.Serializable]
        public struct HealthConfig {
            [Tooltip("Default player HP")]
            public int defaultHp;
            [Tooltip("Maximum player HP")]
            public int maxHp;
            [Tooltip("Default player lives")]
            public int defaultLives;
            [Tooltip("Maximum player lives")]
            public int maxLives;
            [Tooltip("HP increment value")]
            public const int DefaultHpIncrement = 1;
            [Tooltip("HP decrement value")]
            public const int DefaultHpDecrement = 1;
        }

        public static GlobalConfiguration Instance { get; private set; }

        [Header("Debug Configuration")]
        public bool isDebugMode;
        public bool isGodMode;

        [Header("Screen Configuration")]
        public const int DefaultScreenWidth = 640;
        public const int DefaultScreenHeight = 360;
        public const bool DefaultFullScreen = true;
        public const int VSyncCount = 0;
        public const int FrameRate = 12;

        [Header("Physics Configuration")]
        public const float GravityScale = 1.0f;

        [Header("Audio Configuration")]
        public AudioConfig audioSettings;

        [Header("Player Configuration")]
        public PlayerConfig playerSettings;

        [FormerlySerializedAs("healthSettings")] [Header("Health Configuration")]
        public HealthConfig playerHealthSettings;

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