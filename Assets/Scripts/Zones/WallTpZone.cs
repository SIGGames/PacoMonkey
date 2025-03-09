using Controllers;
using Managers;
using UnityEngine;
using static Utils.TagUtils;
using static Utils.LayerUtils;

namespace Zones {
    [RequireComponent(typeof(Collider2D))]
    public class WallTpZone : MonoBehaviour {
        [SerializeField] private float maxCheckDistance = 10f;
        [SerializeField] private float offset = 0.5f;

        private void OnTriggerEnter2D(Collider2D col) {
            if (!col.gameObject.CompareTag(Player)) {
                return;
            }

            PlayerController pc = CharacterManager.Instance.currentPlayerController;
            Vector2 exitPos = FindNearestExit(pc.transform.position);
            pc.SetPosition(exitPos);
        }

        private Vector2 FindNearestExit(Vector2 playerPos) {
            Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
            float minDist = float.MaxValue;
            Vector2 bestPos = playerPos;

            foreach (Vector2 dir in directions) {
                RaycastHit2D hit = Physics2D.Raycast(playerPos, dir, maxCheckDistance, Ground.value);
                if (hit.collider != null) {
                    if (hit.distance < minDist) {
                        minDist = hit.distance;
                        bestPos = hit.point + dir * offset;
                    }
                }
            }

            return bestPos;
        }
    }
}