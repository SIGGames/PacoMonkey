using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Enums;
using Managers;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;
using static Utils.PlayerPrefsKeys;

namespace PlayerInput {
    public class PlayerInputManager : MonoBehaviour {
        public static PlayerInputManager Instance { get; private set; }
        public PlayerInputActions InputActions { get; private set; }

        public InputDeviceType currentInputDevice = InputDeviceType.Unknown;

        [SerializeField, ShowIf("currentInputDevice", InputDeviceType.Controller)]
        private ControllerType currentControllerType = ControllerType.Unknown;

        [SerializeField, Range(0.1f, 3)]
        private float inputDeviceCheckInterval = 0.5f;

        [Header("Controller Input Images")]
        public GamepadIcons playStationIcons;

        public GamepadIcons xboxIcons;

        [Header("Keyboard Input Images")]
        [SerializeField]
        private KeyboardIcons keyboardIcons;

        private Coroutine _checkInputDeviceCoroutine;

        private void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            InputActions = new PlayerInputActions();
            InputActions.Enable();
            UpdateBindingKeys();
        }

        private void Start() {
            if (Gamepad.current != null) {
                SetControllerType();
                currentInputDevice = InputDeviceType.Controller;
            } else if (Keyboard.current != null) {
                currentControllerType = ControllerType.Unknown;
                currentInputDevice = InputDeviceType.Keyboard;
            } else {
                currentControllerType = ControllerType.Unknown;
                currentInputDevice = InputDeviceType.Unknown;
            }

            UpdateInteractKeyImages();
        }

        private void OnEnable() {
            InputActions.PlayerControls.Enable();
            _checkInputDeviceCoroutine = StartCoroutine(CheckInputDeviceCoroutine());
        }

        private void OnDisable() {
            InputActions.PlayerControls.Disable();
            StopCoroutine(_checkInputDeviceCoroutine);
        }

        public void UpdateBindingKeys() {
            if (PlayerPrefs.HasKey(BindingOverridesKey)) {
                string overrides = PlayerPrefs.GetString(BindingOverridesKey);
                InputActions.LoadBindingOverridesFromJson(overrides);
            }
        }

        [SuppressMessage("ReSharper", "IteratorNeverReturns")]
        private IEnumerator CheckInputDeviceCoroutine() {
            while (true) {
                InputDeviceType newInputDevice = GetCurrentInputDevice();
                // Only update if new input device is not Unknown and different from the current one
                if (newInputDevice != InputDeviceType.Unknown && newInputDevice != currentInputDevice) {
                    currentInputDevice = newInputDevice;
                    OnInputTypeChange();
                }

                // If the input device is still unknown, do not wait for the interval (exhaustive check)
                if (currentInputDevice == InputDeviceType.Unknown) {
                    yield return new WaitForSeconds(0.1f);
                } else {
                    yield return new WaitForSeconds(inputDeviceCheckInterval);
                }
            }
        }

        public void OnInputTypeChange() {
            UpdateInteractKeyImages();
        }

        private static InputDeviceType GetCurrentInputDevice() {
            // Check if there is input from the gamepad
            if (Gamepad.current != null) {
                if (Gamepad.current.allControls.Any(control => control is ButtonControl { isPressed: true })
                    || Gamepad.current.leftStick.ReadValue().sqrMagnitude > 0.001f) {
                    SetControllerType();
                    return InputDeviceType.Controller;
                }
            }

            // Check if any key is currently pressed
            if (Keyboard.current != null) {
                if (Keyboard.current.allKeys.Any(key => key.isPressed)) {
                    return InputDeviceType.Keyboard;
                }
            }

            return InputDeviceType.Unknown;
        }

        private static void SetControllerType() {
            if (Gamepad.current is not null) {
                string name = Gamepad.current.device.name.ToLower();

                if (name.Contains("sony") || name.Contains("playstation") || name.Contains("dualshock")) {
                    Instance.currentControllerType = ControllerType.PlayStation;
                    return;
                }

                if (name.Contains("microsoft") || name.Contains("xbox") || name.Contains("controllerwindows")) {
                    Instance.currentControllerType = ControllerType.Xbox;
                    return;
                }
            }

            Instance.currentControllerType = ControllerType.Unknown;
        }

        private void UpdateInteractKeyImages() {
            Sprite interactSprite = GetInputSprite(InputActions.PlayerControls.Interact);

            // Update the interact key image in the floating dialogue
            IEnumerable<Image> keyImages = FindObjectsOfType<Image>(true)
                .Where(img => img.gameObject.name == "FloatingDialogueNextStep");
            foreach (Image keyImage in keyImages) {
                keyImage.sprite = interactSprite;
            }

            // Update the interact key image before interacting with the NPC
            IEnumerable<Image> beforeInteractImages = FindObjectsOfType<Image>(true)
                .Where(img => img.gameObject.name == "FloatingDialogueBeforeInteract");
            foreach (Image image in beforeInteractImages) {
                image.sprite = interactSprite;
            }

            DialogueManager.Instance.beforeInteractButtonPrefab.GetComponent<Image>().sprite = interactSprite;
            // Update the next step image in the fixed dialogue panel
            DialogueManager.Instance.DialogueNextStepImage.sprite = interactSprite;
        }

        public string GetActiveBindingControlPath(InputAction action) {
            // This gets the control path of the first binding that is not a composite depending on the current input device
            if (action.bindings.Count == 0) {
                return "";
            }

            if (currentInputDevice == InputDeviceType.Controller) {
                foreach (var binding in action.bindings) {
                    if (binding.isComposite || binding.isPartOfComposite)
                        continue;

                    if (binding.effectivePath.Contains("Gamepad")) {
                        return binding.effectivePath;
                    }
                }
            }

            if (currentInputDevice == InputDeviceType.Keyboard) {
                foreach (var binding in action.bindings) {
                    if (binding.isComposite || binding.isPartOfComposite)
                        continue;

                    if (binding.effectivePath.Contains("Keyboard")) {
                        return binding.effectivePath;
                    }
                }
            }

            return action.bindings[0].effectivePath;
        }

        private Sprite GetInputSprite(InputAction action) {
            return GetInputSprite(GetActiveBindingControlPath(action));
        }

        public Sprite GetInputSprite(string controlPath) {
            return currentInputDevice switch {
                InputDeviceType.Controller => GetControllerSprite(controlPath),
                InputDeviceType.Keyboard => GetKeyboardSprite(controlPath),
                _ => keyboardIcons.GetSprite(controlPath) // Keyboard is the fallback for input devices
            };
        }

        public Sprite GetKeyboardSprite(string controlPath) {
            return keyboardIcons.GetSprite(controlPath);
        }

        public Sprite GetControllerSprite(string controlPath) {
            return currentControllerType switch {
                ControllerType.PlayStation => playStationIcons.GetSprite(controlPath),
                ControllerType.Xbox => xboxIcons.GetSprite(controlPath),
                _ => playStationIcons.GetSprite(controlPath) // PlayStation is the fallback for controller types
            };
        }

        public static string GetCleanControlPath(string controlPath, bool cleanPath = true) {
            // Clearing the path, this can not be done using .ToHumanReadableString since it UpperCases the first letter
            if (controlPath.Contains("<Keyboard>/")) {
                return controlPath.Replace("<Keyboard>/", "");
            }

            if (controlPath.Contains("<Gamepad>")) {
                return controlPath.Replace("<Gamepad>/", "");
            }

            // Just in case the path is not as expected
            if (cleanPath) {
                return controlPath.Contains("/") ? controlPath[(controlPath.LastIndexOf('/') + 1)..] : controlPath;
            }

            return controlPath;
        }
    }

    [Serializable]
    public struct GamepadIcons {
        public Sprite buttonSouth;
        public Sprite buttonNorth;
        public Sprite buttonEast;
        public Sprite buttonWest;
        public Sprite startButton;
        public Sprite selectButton;
        public Sprite touchpad;
        public Sprite leftTrigger;
        public Sprite rightTrigger;
        public Sprite leftShoulder;
        public Sprite rightShoulder;
        public Sprite dpad;
        public Sprite dpadUp;
        public Sprite dpadDown;
        public Sprite dpadLeft;
        public Sprite dpadRight;
        public Sprite leftStick;
        public Sprite rightStick;
        public Sprite leftStickUp;
        public Sprite leftStickDown;
        public Sprite leftStickLeft;
        public Sprite leftStickRight;
        public Sprite rightStickUp;
        public Sprite rightStickDown;
        public Sprite rightStickLeft;
        public Sprite rightStickRight;
        public Sprite leftStickPress;
        public Sprite rightStickPress;

        public Sprite GetSprite(string controlPath) {
            return PlayerInputManager.GetCleanControlPath(controlPath, false) switch {
                "buttonSouth" => buttonSouth,
                "buttonNorth" => buttonNorth,
                "buttonEast" => buttonEast,
                "buttonWest" => buttonWest,
                "start" => startButton,
                "select" => selectButton,
                "touchpadButton" => touchpad,
                "leftTrigger" => leftTrigger,
                "rightTrigger" => rightTrigger,
                "leftShoulder" => leftShoulder,
                "rightShoulder" => rightShoulder,
                "dpad" => dpad,
                "dpad/up" => dpadUp,
                "dpad/down" => dpadDown,
                "dpad/left" => dpadLeft,
                "dpad/right" => dpadRight,
                "leftStick" => leftStick,
                "leftStick/down" => leftStickDown,
                "leftStick/up" => leftStickUp,
                "leftStick/left" => leftStickLeft,
                "leftStick/right" => leftStickRight,
                "rightStick" => rightStick,
                "rightStick/down" => rightStickDown,
                "rightStick/up" => rightStickUp,
                "rightStick/left" => rightStickLeft,
                "rightStick/right" => rightStickRight,
                "leftStickPress" => leftStickPress,
                "rightStickPress" => rightStickPress,
                _ => null
            };
        }
    }

    [Serializable]
    public struct KeyboardIcons {
        public Sprite mouseLeftButton;
        public Sprite mouseRightButton;
        public Sprite mouseMiddleButton;
        public Sprite keyW;
        public Sprite keyA;
        public Sprite keyS;
        public Sprite keyD;
        public Sprite keyE;
        public Sprite keyQ;
        public Sprite keyR;
        public Sprite keyF;
        public Sprite keyT;
        public Sprite keyG;
        public Sprite keyY;
        public Sprite keyH;
        public Sprite keyU;
        public Sprite keyJ;
        public Sprite keyI;
        public Sprite keyK;
        public Sprite keyO;
        public Sprite keyL;
        public Sprite keyP;
        public Sprite keyZ;
        public Sprite keyX;
        public Sprite keyC;
        public Sprite keyV;
        public Sprite keyB;
        public Sprite keyN;
        public Sprite keyM;
        public Sprite keySpace;
        public Sprite keyShift;
        public Sprite keyCtrl;
        public Sprite keyAlt;
        public Sprite keyTab;
        public Sprite keyCapsLock;
        public Sprite keyEsc;
        public Sprite keyF1;
        public Sprite keyF2;
        public Sprite keyF3;
        public Sprite keyF4;
        public Sprite keyF5;
        public Sprite keyF6;
        public Sprite keyF7;
        public Sprite keyF8;
        public Sprite keyF9;
        public Sprite keyF10;
        public Sprite keyF11;
        public Sprite keyF12;
        public Sprite keyPrintScreen;
        public Sprite keyInsert;
        public Sprite keyHome;
        public Sprite keyPageUp;
        public Sprite keyDelete;
        public Sprite keyEnter;
        public Sprite keyEnd;
        public Sprite keyPageDown;
        public Sprite keyArrowUp;
        public Sprite keyArrowDown;
        public Sprite keyArrowLeft;
        public Sprite keyArrowRight;
        public Sprite keyNumpadMinus;
        public Sprite keyNumpadPlus;
        public Sprite keyPlus;
        public Sprite keyNumpadEnter;
        public Sprite keyNumpad1;
        public Sprite keyNumpad2;
        public Sprite keyNumpad3;
        public Sprite keyNumpad4;
        public Sprite keyNumpad5;
        public Sprite keyNumpad6;
        public Sprite keyNumpad7;
        public Sprite keyNumpad8;
        public Sprite keyNumpad9;
        public Sprite keyNumpad0;

        public Sprite GetSprite(string controlPath) {
            return PlayerInputManager.GetCleanControlPath(controlPath) switch {
                "leftButton" => mouseLeftButton,
                "rightButton" => mouseRightButton,
                "middleButton" => mouseMiddleButton,
                "w" => keyW,
                "a" => keyA,
                "s" => keyS,
                "d" => keyD,
                "e" => keyE,
                "q" => keyQ,
                "r" => keyR,
                "f" => keyF,
                "t" => keyT,
                "g" => keyG,
                "y" => keyY,
                "h" => keyH,
                "u" => keyU,
                "j" => keyJ,
                "i" => keyI,
                "k" => keyK,
                "o" => keyO,
                "l" => keyL,
                "p" => keyP,
                "z" => keyZ,
                "x" => keyX,
                "c" => keyC,
                "v" => keyV,
                "b" => keyB,
                "n" => keyN,
                "m" => keyM,
                "space" => keySpace,
                "leftShift" => keyShift,
                "leftCtrl" => keyCtrl,
                "leftAlt" => keyAlt,
                "tab" => keyTab,
                "capsLock" => keyCapsLock,
                "escape" => keyEsc,
                "f1" => keyF1,
                "f2" => keyF2,
                "f3" => keyF3,
                "f4" => keyF4,
                "f5" => keyF5,
                "f6" => keyF6,
                "f7" => keyF7,
                "f8" => keyF8,
                "f9" => keyF9,
                "f10" => keyF10,
                "f11" => keyF11,
                "f12" => keyF12,
                "printScreen" => keyPrintScreen,
                "insert" => keyInsert,
                "home" => keyHome,
                "pageUp" => keyPageUp,
                "delete" => keyDelete,
                "enter" => keyEnter,
                "end" => keyEnd,
                "pageDown" => keyPageDown,
                "uparrow" => keyArrowUp,
                "downarrow" => keyArrowDown,
                "leftarrow" => keyArrowLeft,
                "rightarrow" => keyArrowRight,
                "numpadMinus" => keyNumpadMinus,
                "slash" => keyNumpadMinus,
                "numpadPlus" => keyNumpadPlus,
                "rightBracket" => keyPlus,
                "numpadEnter" => keyNumpadEnter,
                "numpad1" => keyNumpad1,
                "1" => keyNumpad1,
                "numpad2" => keyNumpad2,
                "2" => keyNumpad2,
                "numpad3" => keyNumpad3,
                "3" => keyNumpad3,
                "numpad4" => keyNumpad4,
                "4" => keyNumpad4,
                "numpad5" => keyNumpad5,
                "5" => keyNumpad5,
                "numpad6" => keyNumpad6,
                "6" => keyNumpad6,
                "numpad7" => keyNumpad7,
                "7" => keyNumpad7,
                "numpad8" => keyNumpad8,
                "8" => keyNumpad8,
                "numpad9" => keyNumpad9,
                "9" => keyNumpad9,
                "numpad0" => keyNumpad0,
                "0" => keyNumpad0,
                _ => null
            };
        }
    }
}