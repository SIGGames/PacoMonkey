using System.Collections.Generic;
using Enums;
using Managers;
using NaughtyAttributes;
using UnityEngine;
using static Utils.TagUtils;

namespace Zones {
    public class StartCinematicZone : MonoBehaviour {
        [SerializeField]
        private Cinematic cinematicToStart;

        [SerializeField]
        private bool overrideCurrentCinematic;

        [SerializeField]
        private bool requireQuestToStart;

        [SerializeField, ShowIf("requireQuestToStart")]
        private List<string> questIdsToStart;

        [SerializeField]
        private bool triggerQuestOnStart;

        [SerializeField, ShowIf("triggerQuestOnStart")]
        private string questIdToTrigger;

        private void OnTriggerEnter2D(Collider2D col) {
            if (!col.CompareTag(Player)) {
                return;
            }

            if (requireQuestToStart && !QuestManager.Instance.IsActiveQuest(questIdsToStart)) {
                return;
            }

            if (triggerQuestOnStart) {
                QuestManager.Instance.SetActiveQuest(questIdToTrigger);
            }

            CinematicManager.Instance.StartCinematic(cinematicToStart, overrideCurrentCinematic);
        }
    }
}