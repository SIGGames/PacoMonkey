using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Utils.PlayerPrefsKeys;

namespace UI {
    [SuppressMessage("ReSharper", "HeuristicUnreachableCode")]
    public class VideoUIConfiguration : MonoBehaviour {
        [SerializeField] private Slider brightnessSlider;
        [SerializeField] private TMP_InputField brightnessInput;
        [SerializeField] private Toggle vSyncCheckbox;
        [SerializeField] private Toggle fullscreenCheckbox;
        [SerializeField] private TMP_Dropdown resolutionDropdown;
        [SerializeField] private bool setAllResolutions;

        private const float DefaultBrightness = 10f;
        private const bool DefaultVSyncCount = true;
        private const bool DefaultFullScreen = false;

        private void Awake() {
            if (brightnessSlider == null || vSyncCheckbox == null || fullscreenCheckbox == null || resolutionDropdown == null ||
                brightnessInput == null) {
                Debug.LogError("One or more UI elements are not assigned in the inspector");
                enabled = false;
            }
        }

        private void Start() {
            float savedBrightness = PlayerPrefs.GetFloat(BrightnessKey, DefaultBrightness);
            bool savedFullscreen = PlayerPrefs.GetInt(FullScreenKey, DefaultFullScreen ? 1 : 0) == 1;
            int savedVSync = PlayerPrefs.GetInt(VSyncCountKey, DefaultVSyncCount ? 1 : 0);

            InitializeSliders(savedBrightness);
            InitializeInputs(savedBrightness);
            InitializeDropdown();

            vSyncCheckbox.isOn = savedVSync != 0;
            fullscreenCheckbox.isOn = savedFullscreen;

            resolutionDropdown.onValueChanged.AddListener(OnResolutionDropdownChanged);
            vSyncCheckbox.onValueChanged.AddListener(OnVSyncChanged);
            fullscreenCheckbox.onValueChanged.AddListener(OnFullscreenChanged);

            ResolutionManager.Instance.SetBrightness(savedBrightness);
            ResolutionManager.Instance.SetVSyncCount(savedVSync);

            if (savedFullscreen) {
                int screenWidth = Screen.currentResolution.width;
                int screenHeight = Screen.currentResolution.height;
                ResolutionManager.SetResolution(screenWidth, screenHeight, true);
            } else {
                ResolutionManager.SetResolution(Screen.width, Screen.height, false);
            }
        }

        private void InitializeSliders(float brightness) {
            brightnessSlider.value = brightness;
            brightnessSlider.onValueChanged.AddListener(OnBrightnessSliderChanged);
            brightnessSlider.onValueChanged.AddListener(delegate { UpdateInputFromSlider(brightnessSlider, brightnessInput); });
        }

        private void InitializeInputs(float brightness) {
            brightnessInput.text = brightness.ToString(CultureInfo.InvariantCulture);
            brightnessInput.onValueChanged.AddListener(delegate { OnBrightnessInputChanged(); });
        }

        private void InitializeDropdown() {
            if (setAllResolutions) {
                resolutionDropdown.ClearOptions();
                foreach (Resolution res in Screen.resolutions) {
                    resolutionDropdown.options.Add(new TMP_Dropdown.OptionData(res.width + "x" + res.height));
                }
            }

            resolutionDropdown.value = GetCurrentResolutionIndex();
        }

        public void ResetVideoSettings() {
            PlayerPrefs.DeleteKey(BrightnessKey);
            PlayerPrefs.DeleteKey(VSyncCountKey);
            PlayerPrefs.DeleteKey(FullScreenKey);
            PlayerPrefs.DeleteKey(ScreenWidthKey);
            PlayerPrefs.DeleteKey(ScreenHeightKey);
            PlayerPrefs.Save();

            brightnessSlider.value = DefaultBrightness;
            brightnessInput.text = DefaultBrightness.ToString(CultureInfo.InvariantCulture);
            vSyncCheckbox.isOn = DefaultVSyncCount;
            fullscreenCheckbox.isOn = DefaultFullScreen;
            resolutionDropdown.value = GetCurrentResolutionIndex();
        }

        private static void OnBrightnessSliderChanged(float value) {
            ResolutionManager.Instance.SetBrightness(value);
        }

        private void OnBrightnessInputChanged() {
            if (float.TryParse(brightnessInput.text, NumberStyles.Float, CultureInfo.InvariantCulture, out float value)) {
                value = Mathf.Clamp(value, 0f, 10f);
                brightnessSlider.value = value;
            }
        }

        private static void OnVSyncChanged(bool state) {
            ResolutionManager.Instance.SetVSyncCount(state ? 1 : 0);
        }

        private void OnFullscreenChanged(bool state) {
            resolutionDropdown.interactable = !state;
            if (state) {
                int screenWidth = Screen.currentResolution.width;
                int screenHeight = Screen.currentResolution.height;
                ResolutionManager.SetResolution(screenWidth, screenHeight, true);
            } else {
                ResolutionManager.SetResolution(Screen.width, Screen.height, false);
            }
        }

        private void OnResolutionDropdownChanged(int index) {
            if (fullscreenCheckbox.isOn) {
                return;
            }

            string[] parts = resolutionDropdown.options[index].text.ToLower().Split('x');
            int width = int.Parse(parts[0]);
            int height = int.Parse(parts[1]);
            if (width <= 200 || height <= 200) {
                return;
            }

            ResolutionManager.SetResolution(width, height, false);
        }

        private static void UpdateInputFromSlider(Slider slider, TMP_InputField inputField) {
            inputField.text = slider.value.ToString(CultureInfo.InvariantCulture);
        }

        private static int GetCurrentResolutionIndex() {
            for (int i = 0; i < Screen.resolutions.Length; i++) {
                if (Screen.resolutions[i].width == Screen.width && Screen.resolutions[i].height == Screen.height) {
                    return i;
                }
            }

            return 0;
        }
    }
}