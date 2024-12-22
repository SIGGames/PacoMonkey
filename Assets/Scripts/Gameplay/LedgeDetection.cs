using Controllers;
using UnityEngine;

namespace Gameplay {
    public class LedgeDetection : MonoBehaviour {
        [Range(0, 1)]
        [SerializeField] private float radius;

        [SerializeField] private LayerMask whatIsGround;
        [SerializeField] private PlayerController player;

        [SerializeField] private GameObject ledgeCheck;

        public bool isNearLedge;

        private void Update() {
            isNearLedge = IsLedge();

            UpdateLedgeCheckPosition();
        }

        private bool IsLedge() {
            Vector2 spherePosition = ledgeCheck.transform.position;

            RaycastHit2D hitDown = Physics2D.Raycast(spherePosition, Vector2.down, radius, whatIsGround);
            RaycastHit2D hitUp = Physics2D.Raycast(spherePosition, Vector2.up, radius, whatIsGround);

            if (hitDown.collider == null) {
                return false;
            }

            if (hitUp.collider != null) {
                return false;
            }

            return true;
        }

        private void UpdateLedgeCheckPosition() {
            Vector3 ledgeCheckPosition = ledgeCheck.transform.localPosition;

            if (player.isFacingRight) {
                ledgeCheck.transform.localPosition = new Vector3(
                    Mathf.Abs(ledgeCheckPosition.x),
                    ledgeCheckPosition.y,
                    ledgeCheckPosition.z);
            } else {
                ledgeCheck.transform.localPosition = new Vector3(
                    -Mathf.Abs(ledgeCheckPosition.x),
                    ledgeCheckPosition.y,
                    ledgeCheckPosition.z);
            }
        }

        private void OnDrawGizmos() {
            Vector3 ledgeCheckPosition = ledgeCheck.transform.position;
            Gizmos.DrawWireSphere(ledgeCheckPosition, radius);

            Gizmos.color = Color.green;
            Gizmos.DrawLine(ledgeCheckPosition, ledgeCheckPosition + Vector3.down * radius);

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(ledgeCheckPosition, ledgeCheckPosition + Vector3.up * radius);
        }
    }
}