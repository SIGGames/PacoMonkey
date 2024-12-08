using UnityEngine;

namespace PlayerInput {
    public class PlayerInputManager : MonoBehaviour {
        public static PlayerInputManager Instance { get; private set; }
        public PlayerInputActions InputActions { get; private set; }

        private void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            InputActions = new PlayerInputActions();
            InputActions.Enable();
        }

        private void OnEnable() {
            InputActions.PlayerControls.Enable();
        }

        private void OnDisable() {
            InputActions.PlayerControls.Disable();
        }
    }
}