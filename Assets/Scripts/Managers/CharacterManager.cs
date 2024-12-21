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
        }

        [Header("Character Configurations")]
        [SerializeField] private CharacterConfiguration[] characters;

        [Header("Cinemachine")]
        private CinemachineVirtualCamera _cinemachineCamera;

        private int _currentCharacterIndex;

        private void Start() {
            _currentCharacterIndex = Array.FindIndex(characters, c => c.characterType == initialCharacter);
            if (_currentCharacterIndex < 0) {
                _currentCharacterIndex = 0;
            }

            SetCharacter(characters[_currentCharacterIndex].characterType);
        }

        private void Update() {
            if (Input.GetKeyDown(KeyCode.F5)) {
                Debug.Log("[INFO]: This bind is just for testing purposes, this will be removed");
                _currentCharacterIndex = (_currentCharacterIndex + 1) % characters.Length;
                SetCharacter(characters[_currentCharacterIndex].characterType);
            }
        }

        public void SetCharacter(Character character) {
            CharacterConfiguration selectedConfig = null;

            foreach (var config in characters) {
                if (config.characterGameObject != null) {
                    config.characterGameObject.SetActive(false);
                }

                if (config.characterType == character) {
                    selectedConfig = config;
                }
            }

            if (selectedConfig == null) {
                return;
            }

            if (selectedConfig.characterGameObject != null) {
                selectedConfig.characterGameObject.SetActive(true);
            }

            currentCharacter = character;

            // Update the Cinemachine camera to follow the new character
            if (_cinemachineCamera != null && selectedConfig.characterGameObject != null) {
                _cinemachineCamera.Follow = selectedConfig.characterGameObject.transform;
                _cinemachineCamera.LookAt = selectedConfig.characterGameObject.transform;
            }
        }

        public Character GetCurrentCharacter() {
            return currentCharacter;
        }
    }
}