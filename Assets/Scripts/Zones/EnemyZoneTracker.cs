using System;
using System.Collections.Generic;
using Controllers;
using Gameplay;
using Managers;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using static Utils.TagUtils;

namespace Zones {
    [RequireComponent(typeof(Collider2D))]
    public class EnemyZoneTracker : MonoBehaviour {
        [SerializeField] private bool questRequired;

        [Tooltip("Active quest required to trigger action")]
        [ShowIf("questRequired")]
        [SerializeField] private string requiredQuestId;

        [SerializeField] private bool setNextQuest;

        [ShowIf("setNextQuest"), Tooltip("Next quest to be activated after all enemies are defeated")]
        [SerializeField] private string nextQuestId;

        [SerializeField] private bool showEnemyCount;
        [SerializeField, ShowIf("showEnemyCount")] private GameObject enemyCountTextPrefab;
        [SerializeField, ShowIf("showEnemyCount")] private TextMeshProUGUI enemyCountText;

        private readonly HashSet<EnemyController> _trackedEnemies = new();
        private int _originalEnemyCount;

        private void Awake() {
            if (enemyCountTextPrefab != null) {
                enemyCountTextPrefab.SetActive(false);
            }

            TrackEnemies();
        }

        private void OnEnable() {
            TrackEnemies();
        }

        private void TrackEnemies() {
            Collider2D zoneCollider = GetComponent<Collider2D>();
            zoneCollider.isTrigger = true;

            List<Collider2D> results = new();
            ContactFilter2D filter = new ContactFilter2D { useTriggers = true };
            Physics2D.OverlapCollider(zoneCollider, filter, results);

            foreach (Collider2D col in results) {
                EnemyController enemy = col.GetComponent<EnemyController>();
                if (enemy != null) {
                    _trackedEnemies.Add(enemy);
                }
            }

            _originalEnemyCount = _trackedEnemies.Count;
            EnemyDeath.OnEnemyDeath += OnEnemyDeath;
        }

        private void OnTriggerEnter2D(Collider2D other) {
            if (!other.CompareTag(Player)) {
                return;
            }

            if (_trackedEnemies.Count == 0 || enemyCountTextPrefab == null || enemyCountText == null) {
                return;
            }

            enemyCountTextPrefab.SetActive(true);
            enemyCountText.text = $"[{_trackedEnemies.Count}/{_originalEnemyCount}]";
        }

        private void OnDestroy() {
            EnemyDeath.OnEnemyDeath -= OnEnemyDeath;
        }

        private void OnEnemyDeath(EnemyController enemy) {
            if (!_trackedEnemies.Contains(enemy)) {
                return;
            }

            _trackedEnemies.Remove(enemy);
            if (_trackedEnemies.Count == 0) {
                OnAllEnemiesDefeated();
            }
        }

        private void OnAllEnemiesDefeated() {
            if (questRequired) {
                QuestManager.Quest activeQuest = QuestManager.Instance.GetActiveQuest();
                if (activeQuest == null || activeQuest.id != requiredQuestId) {
                    // Quest not active or not the required quest
                    return;
                }
            }

            if (enemyCountTextPrefab != null) {
                enemyCountTextPrefab.SetActive(false);
                enemyCountText.text = string.Empty;
            }

            if (!string.IsNullOrEmpty(nextQuestId)) {
                QuestManager.Instance.SetQuestAvailable(nextQuestId);
            }

            if (setNextQuest && !string.IsNullOrEmpty(nextQuestId)) {
                QuestManager.Instance.SetActiveQuest(nextQuestId);
            }
        }
    }
}