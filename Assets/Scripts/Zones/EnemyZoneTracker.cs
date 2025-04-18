using System.Collections.Generic;
using Controllers;
using Gameplay;
using Managers;
using NaughtyAttributes;
using UnityEngine;

namespace Zones {
    [RequireComponent(typeof(Collider2D))]
    public class EnemyZoneTracker : MonoBehaviour {
        [SerializeField] private bool questRequired;
        [Tooltip("Active quest required to trigger action")]
        [ShowIf("questRequired")]
        [SerializeField] private string requiredQuestId;
        [Tooltip("Next quest to be activated after all enemies are defeated")]
        [SerializeField] private string nextQuestId;

        private readonly HashSet<EnemyController> _trackedEnemies = new();

        private void Awake() {
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

            EnemyDeath.OnEnemyDeath += OnEnemyDeath;
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
            if (questRequired && QuestManager.Instance.GetActiveQuest().id != requiredQuestId) {
                return;
            }
            if (!string.IsNullOrEmpty(nextQuestId)) {
                QuestManager.Instance.SetActiveQuest(nextQuestId);
            }
        }
    }
}