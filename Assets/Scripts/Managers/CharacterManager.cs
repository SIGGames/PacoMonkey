﻿using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Controllers;
using Enums;
using Gameplay;
using Health;
using Health.UI;
using UI.TextSetters;
using UnityEngine;
using Utils;
using Zones;
using static Utils.AnimatorUtils;
using static Utils.PlayerPrefsKeys;

namespace Managers {
    public class CharacterManager : MonoBehaviour {
        public static CharacterManager Instance { get; private set; }

        [Header("Character")]
        [SerializeField] private Character initialCharacter;

        [SerializeField] public Character currentCharacter;
        [HideInInspector] public PlayerController currentPlayerController;
        [HideInInspector] public Sprite currentCharacterFaceSprite;
        [HideInInspector] public float currentCharacterRespawnTime;

        [Serializable]
        public class CharacterConfiguration {
            public Character characterType;
            public GameObject characterGameObject;
            public AnimatorOverrideController animatorOverrideController;
            public Sprite characterFaceSprite;

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
            foreach (CharacterConfiguration config in characters) {
                if (config.characterGameObject == null) {
                    Debug.LogError($"Character {config.characterType} is missing its GameObject");
                    enabled = false;
                    return;
                }
            }

            LoadCharacter();
        }

        private void NextCharacter() {
            _currentCharacterIndex = (_currentCharacterIndex + 1) % characters.Length;
            SetCharacter(characters[_currentCharacterIndex].characterType);
        }

        public void SetCharacter(Character character) {
            Vector3 previousPosition = Vector3.zero;

            foreach (CharacterConfiguration config in characters) {
                if (config.characterType == currentCharacter) {
                    previousPosition = config.characterGameObject.transform.position;
                }

                config.characterGameObject.SetActive(false);
            }

            CharacterConfiguration selectedConfig = Array.Find(characters, c => c.characterType == character);
            if (selectedConfig == null) {
                return;
            }

            currentCharacter = character;
            SaveCharacter();
            currentPlayerController = GetCurrentPlayerController();
            currentCharacterFaceSprite = selectedConfig.characterFaceSprite;
            currentCharacterRespawnTime = selectedConfig.respawnTime;
            UpdatePlayerFaceSprite();

            currentPlayerController.SetPosition(previousPosition);
            selectedConfig.characterGameObject.SetActive(true);
            currentPlayerController.FreezePosition(false);
            DifficultyManager.Instance.SetPlayerDifficultyMultiplier();
            UpdateAnimator(selectedConfig);

            // Update the Cinemachine camera to follow the new character
            CameraManager.Instance.FollowAndLookAt(selectedConfig.characterGameObject.transform);

            Lives playerLives = currentPlayerController.lives;
            if (playerLives != null) {
                healthBar.SetPlayerLives(playerLives);
            }
        }

        public void RespawnCharacter() {
            CharacterConfiguration selectedConfig = Array.Find(characters, c => c.characterType == currentCharacter);
            StartCoroutine(RespawnRoutine(selectedConfig, false));
        }

        public void RespawnCharacter(bool showCinematic) {
            CharacterConfiguration selectedConfig = Array.Find(characters, c => c.characterType == currentCharacter);
            StartCoroutine(RespawnRoutine(selectedConfig, showCinematic));
        }

        private IEnumerator RespawnRoutine(CharacterConfiguration characterConfig, bool showCinematic) {
            currentPlayerController.FreezePosition();
            currentPlayerController.SetVelocity(Vector2.zero);
            currentPlayerController.SetColliderOnDeath();
            if (showCinematic) {
                CinematicManager.Instance.StartCinematic(Cinematic.Death, true);
            }
            yield return new WaitForSeconds(showCinematic ? characterConfig.respawnTime : 0);
            currentPlayerController.FreezePosition(false);
            currentPlayerController.ResetState(false);
            currentPlayerController.SetColliderOnDeath();
            InstanceEnemies();
            ResetEnemyCount();
            CameraManager.Instance.FollowAndLookAt(characterConfig.characterGameObject.transform);
        }

        private static void UpdateAnimator(CharacterConfiguration characterConfig) {
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
            CharacterConfiguration selectedConfig = Array.Find(characters, c => c.characterType == currentCharacter);
            return selectedConfig.characterGameObject.GetComponent<PlayerController>();
        }

        public static void InstanceEnemies() {
            List<EnemyController> currentEnemies = new(FindObjectsOfType<EnemyController>(true));

            foreach (EnemySpawnData spawnData in EnemySpawnManager.EnemySpawnList) {
                // Filter enemy by type and name since it's guaranteed that the enemy type and name are unique
                EnemyController enemyController = currentEnemies.Find(e =>
                    e != null &&
                    e.enemyType == spawnData.enemyType &&
                    e.name.ToLower().Contains(spawnData.name.ToLower())
                );

                if (enemyController != null) {
                    currentEnemies.Remove(enemyController);

                    enemyController.transform.position = spawnData.spawnPosition;
                    enemyController.transform.rotation = spawnData.spawnRotation;
                    enemyController.ResetEnemy();
                }
            }

            SetBossOnPosition();
        }

        private static void SetBossOnPosition() {
            EnemyController[] enemies = FindObjectsOfType<EnemyController>(true);

            foreach (EnemyController enemy in enemies) {
                // Hardcoded position for boss to ensure it's always on the correct spot
                if (enemy != null && enemy.name.ToLower().Contains("boss")) {
                    enemy.transform.position = new Vector3(141.7f, -10.8f, 0f);
                }
            }
        }

        private void SaveCharacter() {
            PlayerPrefs.SetString(CurrentCharacterKey, currentCharacter.ToString());
            PlayerPrefs.Save();
        }

        private void LoadCharacter() {
            string characterString = PlayerPrefs.GetString(CurrentCharacterKey, initialCharacter.ToString());
            SetCharacter(Enum.TryParse(characterString, out Character loadedCharacter) ? loadedCharacter : initialCharacter);
        }

        public static void ResetState() {
            List<PlayerController> playerControllers = new(FindObjectsOfType<PlayerController>(true));
            foreach (PlayerController playerController in playerControllers) {
                playerController.ResetState(true);
            }

            InstanceEnemies();
        }

        private void UpdatePlayerFaceSprite() {
            // Since there is only one player face, we can use the first one
            SetPlayerFace setPlayerFaceSetters = FindObjectOfType<SetPlayerFace>(true);
            if (setPlayerFaceSetters != null) {
                setPlayerFaceSetters.UpdatePlayerFace(currentCharacterFaceSprite);
            }
        }

        private static void ResetEnemyCount() {
            EnemyZoneTracker[] enemyZoneTrackers = FindObjectsOfType<EnemyZoneTracker>(true);
            foreach (EnemyZoneTracker enemyZoneTracker in enemyZoneTrackers) {
                if (enemyZoneTracker == null) {
                    continue;
                }

                enemyZoneTracker.ResetZone();
            }
        }
    }
}