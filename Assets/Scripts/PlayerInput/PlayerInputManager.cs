﻿using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Enums;
using Managers;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;

namespace PlayerInput {
    public class PlayerInputManager : MonoBehaviour {
        public static PlayerInputManager Instance { get; private set; }
        public PlayerInputActions InputActions { get; private set; }

        public InputDeviceType currentInputDevice = InputDeviceType.Unknown;

        [SerializeField, Range(0.1f, 3)]
        private float inputDeviceCheckInterval = 0.5f;

        [Header("Controller Input Images")]
        [SerializeField]
        private Sprite controllerInteractImage;

        [Header("Keyboard Input Images")]
        [SerializeField]
        private Sprite keyboardInteractImage;

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
        }

        private void OnEnable() {
            InputActions.PlayerControls.Enable();
            _checkInputDeviceCoroutine = StartCoroutine(CheckInputDeviceCoroutine());
        }

        private void OnDisable() {
            InputActions.PlayerControls.Disable();
            StopCoroutine(_checkInputDeviceCoroutine);
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

        private void OnInputTypeChange() {
            UpdateKeyImages();
        }

        private static InputDeviceType GetCurrentInputDevice() {
            // Check if there is input from the gamepad
            if (Gamepad.current != null) {
                if (Gamepad.current.allControls.Any(control => control is ButtonControl { isPressed: true })
                    || Gamepad.current.leftStick.ReadValue().sqrMagnitude > 0.001f) {
                    return InputDeviceType.Controller;
                }
            }

            // Check if any key is currently pressed
            if (Keyboard.current != null) {
                if (Keyboard.current.allKeys.Any(key => key.isPressed))
                    return InputDeviceType.Keyboard;
            }

            return InputDeviceType.Unknown;
        }

        private void UpdateKeyImages() {
            // Update the interact key image in the floating dialogue
            IEnumerable<Image> keyImages = FindObjectsOfType<Image>(true)
                .Where(img => img.gameObject.name == "FloatingDialogueNextStep");
            foreach (Image keyImage in keyImages) {
                keyImage.sprite = GetInputSprite(keyImage);
            }

            // Update the interact key image before interacting with the NPC
            IEnumerable<Image> beforeInteractImages = FindObjectsOfType<Image>(true)
                .Where(img => img.gameObject.name == "FloatingDialogueBeforeInteract");
            foreach (Image image in beforeInteractImages) {
                image.sprite = GetInputSprite(image);
            }

            // Update the next step image in the fixed dialogue panel
            DialogueManager.Instance.DialogueNextStepImage.sprite =
                GetInputSprite(DialogueManager.Instance.DialogueNextStepImage);
        }

        private Sprite GetInputSprite(Image image) {
            return currentInputDevice switch {
                InputDeviceType.Controller => controllerInteractImage,
                InputDeviceType.Keyboard => keyboardInteractImage,
                _ => image.sprite
            };
        }
    }
}