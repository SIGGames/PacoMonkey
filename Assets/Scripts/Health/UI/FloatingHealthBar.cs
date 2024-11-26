using UnityEngine;
using UnityEngine.UI;

namespace Health.UI {
    public class FloatingHealthBar : MonoBehaviour {
        [SerializeField] private Slider slider;
        [SerializeField] private Image fillImage;
        [SerializeField] private Image background;
        [SerializeField] private bool showOnStart;
        [SerializeField] private bool visible = true;
        [SerializeField] private bool useColorStates = true;

        private HealthBarState _currentState;

        private struct HealthBarState {
            public readonly float minPercent;
            public readonly float maxPercent;
            public readonly Color color;

            public HealthBarState(float min, float max, Color stateColor) {
                minPercent = min;
                maxPercent = max;
                color = stateColor;
            }
        }

        private readonly HealthBarState[] _states = {
            new(0f, 0.25f, GetColor(248, 113, 104)), // Red
            new(0.25f, 0.75f, GetColor(248, 230, 160)), // Yellow
            new(0.5f, 1f, GetColor(75, 206, 151)) // Green
        };

        private static Color GetColor(float r, float g, float b) {
            return new Color(r / 255f, g / 255f, b / 255f);
        }

        void Start() {
            EnsureComponents();
            visible = showOnStart;
            SetVisibility(visible);
            if (useColorStates) {
                UpdateColor();
            }
        }

        private void EnsureComponents() {
            if (slider == null) {
                slider = GetComponent<Slider>();
            }

            if (slider == null) {
                Debug.LogError("No slider component found on " + gameObject.name);
            }

            if (fillImage == null) {
                fillImage = slider.fillRect.GetComponent<Image>();
            }

            if (fillImage == null) {
                Debug.LogError("No fill image component found on " + gameObject.name);
            }

            if (background == null) {
                Debug.LogError("No background image component found on " + gameObject.name);
            }
        }

        public void UpdateHealthBar(float currentValue, float maxValue) {
            slider.value = currentValue / maxValue;
            if (useColorStates) {
                UpdateColor();
            }
        }

        public void HideFloatingHealthBar() {
            SetVisibility(false);
        }

        public void ShowFloatingHealthBar() {
            SetVisibility(true);
        }

        private void SetVisibility(bool isVisible) {
            visible = isVisible;
            slider.enabled = isVisible;
            fillImage.enabled = isVisible;
            background.enabled = isVisible;
        }

        private void UpdateColor() {
            float value = slider.value;

            foreach (var state in _states) {
                if (value >= state.minPercent && value <= state.maxPercent) {
                    if (_currentState.color != state.color) {
                        fillImage.color = state.color;
                        _currentState = state;
                    }

                    break;
                }
            }
        }
    }
}