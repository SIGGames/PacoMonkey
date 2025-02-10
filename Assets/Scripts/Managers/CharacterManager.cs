using System;
using System.Collections;
using Cinemachine;
using Controllers;
using Enums;
using Health;
using Health.UI;
using UnityEngine;
using Utils;
using static Utils.AnimatorUtils;

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

            [Range(0, 10)]
            public float respawnTime = 2f;
        }

        [Header("Character Configurations")]
        [SerializeField] private CharacterConfiguration[] characters;

        [Header("Cinemachine")]
        [SerializeField] private CinemachineVirtualCamera cinemachineCamera;

        [Header("UI")]
        [SerializeField] private HealthBar healthBar;

        private int _currentCharacterIndex;

        private void Awake() {
            if (cinemachineCamera == null) {
                cinemachineCamera = FindObjectOfType<CinemachineVirtualCamera>();
            }

            if (healthBar == null) {
                healthBar = FindObjectOfType<HealthBar>();
            }

            if (cinemachineCamera == null || healthBar == null) {
                Debugger.Log(("CinemachineCamera", cinemachineCamera), ("HealthBar", healthBar));
                enabled = false;
            }
        }

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
                NextCharacter();
            }
        }

        private void NextCharacter() {
            _currentCharacterIndex = (_currentCharacterIndex + 1) % characters.Length;
            SetCharacter(characters[_currentCharacterIndex].characterType);
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
            PlayerController currentCharacterPC = selectedConfig.characterGameObject.GetComponent<PlayerController>();
            currentCharacterPC.FreezePosition(false);
            UpdateAnimator(selectedConfig);

            currentCharacter = character;

            // Update the Cinemachine camera to follow the new character
            cinemachineCamera.Follow = selectedConfig.characterGameObject.transform;
            cinemachineCamera.LookAt = selectedConfig.characterGameObject.transform;

            Lives playerLives = currentCharacterPC.lives;
            if (playerLives != null) {
                healthBar.SetPlayerLives(playerLives);
            }
        }

        public void RespawnCharacter() {
            var selectedConfig = Array.Find(characters, c => c.characterType == currentCharacter);
            StartCoroutine(RespawnRoutine(selectedConfig));
        }

        private IEnumerator RespawnRoutine(CharacterConfiguration characterConfig) {
            PlayerController player = characterConfig.characterGameObject.GetComponent<PlayerController>();
            player.FreezePosition();
            player.SetVelocity(Vector2.zero);
            yield return new WaitForSeconds(characterConfig.respawnTime);
            player.FreezePosition(false);
            player.ResetState();

            cinemachineCamera.Follow = characterConfig.characterGameObject.transform;
            cinemachineCamera.LookAt = characterConfig.characterGameObject.transform;
        }

        private void UpdateAnimator(CharacterConfiguration characterConfig) {
            Animator animator = characterConfig.characterGameObject.GetComponent<Animator>();
            if (animator == null) {
                Debug.LogError($"Character {characterConfig.characterType} is missing its Animator component");
                return;
            }

            animator.runtimeAnimatorController = characterConfig.animatorOverrideController;

            if (characterConfig.characterGameObject.name.Contains("Micca1")) {
                animator.SetBool(IsMicca1, true);
            } else {
                animator.SetBool(IsMicca1, false);
            }
        }

        public Character GetCurrentCharacter() {
            return currentCharacter;
        }

        public PlayerController GetCurrentCharacterController() {
            var selectedConfig = Array.Find(characters, c => c.characterType == currentCharacter);
            return selectedConfig.characterGameObject.GetComponent<PlayerController>();
        }
    }
}