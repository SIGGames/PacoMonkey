using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Controllers;
using Enums;
using Gameplay;
using Health;
using Health.UI;
using UnityEngine;
using Utils;
using static Utils.AnimatorUtils;

namespace Managers {
    public class CharacterManager : MonoBehaviour {
        public static CharacterManager Instance { get; private set; }

        [Header("Character")]
        [SerializeField] private Character initialCharacter;

        [SerializeField] private Character currentCharacter;
        public PlayerController currentPlayerController;

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

            if (Instance == null) {
                Instance = this;
            } else {
                Destroy(gameObject);
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

            currentCharacter = character;
            currentPlayerController = GetCurrentPlayerController();

            selectedConfig.characterGameObject.transform.position = previousPosition;
            selectedConfig.characterGameObject.SetActive(true);
            currentPlayerController.FreezePosition(false);
            UpdateAnimator(selectedConfig);

            // Update the Cinemachine camera to follow the new character
            cinemachineCamera.Follow = selectedConfig.characterGameObject.transform;
            cinemachineCamera.LookAt = selectedConfig.characterGameObject.transform;

            Lives playerLives = currentPlayerController.lives;
            if (playerLives != null) {
                healthBar.SetPlayerLives(playerLives);
            }
        }

        public void RespawnCharacter() {
            var selectedConfig = Array.Find(characters, c => c.characterType == currentCharacter);
            StartCoroutine(RespawnRoutine(selectedConfig));
        }

        private IEnumerator RespawnRoutine(CharacterConfiguration characterConfig) {
            currentPlayerController.FreezePosition();
            currentPlayerController.SetVelocity(Vector2.zero);
            currentPlayerController.SetColliderOnDeath();
            yield return new WaitForSeconds(characterConfig.respawnTime);
            currentPlayerController.FreezePosition(false);
            currentPlayerController.ResetState();
            currentPlayerController.SetColliderOnDeath();
            InstanceEnemies();

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

        private PlayerController GetCurrentPlayerController() {
            var selectedConfig = Array.Find(characters, c => c.characterType == currentCharacter);
            return selectedConfig.characterGameObject.GetComponent<PlayerController>();
        }

        private static void InstanceEnemies() {
            List<EnemyController> currentEnemies = new(FindObjectsOfType<EnemyController>(true));

            foreach (EnemySpawnData spawnData in EnemySpawnManager.EnemySpawnList) {
                EnemyController enemyController = currentEnemies.Find(e => e != null
                                                                           && e.enemyType == spawnData.enemyType);

                if (enemyController != null) {
                    currentEnemies.Remove(enemyController);

                    enemyController.transform.position = spawnData.spawnPosition;
                    enemyController.transform.rotation = spawnData.spawnRotation;
                    enemyController.ResetEnemy();
                }
            }
        }
    }
}