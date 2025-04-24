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
        private bool requireQuestToStart;

        [SerializeField, ShowIf("requireQuestToStart")]
        private string questIdToStart;

        private void OnTriggerEnter2D(Collider2D col) {
            if (!col.CompareTag(Player)) {
                return;
            }

            if (requireQuestToStart && !QuestManager.Instance.IsActiveQuest(questIdToStart)) {
                return;
            }

            CinematicManager.Instance.StartCinematic(cinematicToStart);
        }
    }
}