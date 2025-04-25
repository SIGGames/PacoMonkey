using System.Collections.Generic;
using Controllers;
using Gameplay;
using Managers;
using NaughtyAttributes;
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

        private readonly HashSet<EnemyController> _trackedEnemies = new();
        private int _originalEnemyCount;

        private void Awake() {
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

            if (_trackedEnemies.Count == 0) {
                return;
            }

            if (showEnemyCount) {
                QuestManager.Instance.ShowEnemyCountText(true, _trackedEnemies.Count, _originalEnemyCount);
            }
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

            if (showEnemyCount) {
                QuestManager.Instance.ShowEnemyCountText(true, _trackedEnemies.Count, _originalEnemyCount);
            }
        }

        private void OnAllEnemiesDefeated() {
            if (questRequired) {
                if (QuestManager.Instance.IsActiveQuest(requiredQuestId)) {
                    // Quest not active or not the required quest
                    return;
                }
            }

            if (!string.IsNullOrEmpty(nextQuestId)) {
                QuestManager.Instance.SetQuestAvailable(nextQuestId);
            }

            if (setNextQuest && !string.IsNullOrEmpty(nextQuestId)) {
                QuestManager.Instance.SetActiveQuest(nextQuestId);
            }
        }

        public void ResetZone() {
            _trackedEnemies.Clear();
            _originalEnemyCount = 0;
            QuestManager.Instance.ShowEnemyCountText(false);
            TrackEnemies();
        }
    }
}