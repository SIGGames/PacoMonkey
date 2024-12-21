using System;
using Cinemachine;
using Enums;
using UnityEngine;

namespace Managers {
    public class CharacterManager : MonoBehaviour {
        [Header("Character")]
        [SerializeField] private Character initialCharacter;

        [SerializeField] private Character currentCharacter;

        [Serializable]
        public class CharacterConfiguration {
            public Character characterType;
            public GameObject characterGameObject;
            public AnimatorOverrideController animatorOverrideController;
        }

        [Header("Character Configurations")]
        [SerializeField] private CharacterConfiguration[] characters;

        [Header("Cinemachine")]
        [SerializeField] private CinemachineVirtualCamera cinemachineCamera;

        private int _currentCharacterIndex;

        private void Start() {
            foreach (var config in characters) {
                if (config.characterGameObject == null) {
                    Debug.LogError($"Character {config.characterType} is missing its GameObject");
                    enabled = false;
                    return;
                }
            }

            _currentCharacterIndex = Array.FindIndex(characters, c => c.characterType == initialCharacter);
            if (_currentCharacterIndex < 0) {
                _currentCharacterIndex = 0;
            }

            SetCharacter(characters[_currentCharacterIndex].characterType);
        }

        private void Update() {
            // TODO: This bind is just for testing purposes, this will be removed
            if (Input.GetKeyDown(KeyCode.F5)) {
                _currentCharacterIndex = (_currentCharacterIndex + 1) % characters.Length;
                SetCharacter(characters[_currentCharacterIndex].characterType);
            }
        }

        public void SetCharacter(Character character) {
            Vector3 previousPosition = Vector3.zero;

            foreach (var config in characters) {
                if (config.characterType == currentCharacter) {
                    previousPosition = config.characterGameObject.transform.position;
                }

                config.characterGameObject.SetActive(false);
            }

            var selectedConfig = Array.Find(characters, c => c.characterType == character);
            if (selectedConfig == null) {
                return;
            }

            selectedConfig.characterGameObject.transform.position = previousPosition;
            selectedConfig.characterGameObject.SetActive(true);

            var animator = selectedConfig.characterGameObject.GetComponent<Animator>();
            if (animator != null && selectedConfig.animatorOverrideController != null) {
                animator.runtimeAnimatorController = selectedConfig.animatorOverrideController;
            }

            currentCharacter = character;

            // Update the Cinemachine camera to follow the new character
            if (cinemachineCamera != null) {
                cinemachineCamera.Follow = selectedConfig.characterGameObject.transform;
                cinemachineCamera.LookAt = selectedConfig.characterGameObject.transform;
            }
        }

        public Character GetCurrentCharacter() {
            return currentCharacter;
        }
    }
}