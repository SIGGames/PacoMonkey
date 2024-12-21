using System;
using Cinemachine;
using Controllers;
using Enums;
using Mechanics.Movement;
using UnityEngine;

namespace Managers {
    public class CharacterManager : MonoBehaviour {
        [Serializable]
        public class CharacterConfiguration {
            public Character characterType;
            public Sprite characterSprite;
            public Vector2 colliderOffset;
            public Vector2 colliderSize;
            public float crouchSpeedMultiplier;
            public float maxRunSpeed;
            public float jumpModifier;
            public GameObject characterGameObject;
        }

        [Header("Character Configurations")]
        public CharacterConfiguration[] characters;

        [Header("Starting Character")]
        public Character initialCharacter;

        [Header("Cinemachine")]
        public CinemachineVirtualCamera cinemachineCamera;

        private Character _currentCharacter;
        private int currentCharacterIndex;

        private void Start() {
            currentCharacterIndex = Array.FindIndex(characters, c => c.characterType == initialCharacter);
            if (currentCharacterIndex < 0) {
                currentCharacterIndex = 0;
            }

            SetCharacter(characters[currentCharacterIndex].characterType);
        }

        private void Update() {
            if (Input.GetKeyDown(KeyCode.F5)) {
                currentCharacterIndex = (currentCharacterIndex + 1) % characters.Length;
                SetCharacter(characters[currentCharacterIndex].characterType);
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

            ApplyCharacterConfiguration(selectedConfig);

            _currentCharacter = character;

            if (cinemachineCamera != null && selectedConfig.characterGameObject != null) {
                cinemachineCamera.Follow = selectedConfig.characterGameObject.transform;
            }
        }

        private void ApplyCharacterConfiguration(CharacterConfiguration config) {
            if (config.characterGameObject != null) {
                var spriteRenderer = config.characterGameObject.GetComponent<SpriteRenderer>();
                var boxCollider = config.characterGameObject.GetComponent<BoxCollider2D>();
                var playerController = config.characterGameObject.GetComponent<PlayerController>();
                var crouch = config.characterGameObject.GetComponent<Crouch>();

                if (spriteRenderer != null) {
                    spriteRenderer.sprite = config.characterSprite;
                }

                if (boxCollider != null) {
                    boxCollider.offset = config.colliderOffset;
                    boxCollider.size = config.colliderSize;
                }

                if (playerController != null) {
                    playerController.maxRunSpeed = config.maxRunSpeed;
                    playerController.jumpModifier = config.jumpModifier;
                }

                if (crouch != null) {
                    crouch.crouchSpeedMultiplier = config.crouchSpeedMultiplier;
                }
            }
        }

        public Character GetCurrentCharacter() {
            return _currentCharacter;
        }
    }
}