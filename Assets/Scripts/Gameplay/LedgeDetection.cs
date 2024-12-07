using Controllers;
using UnityEngine;

namespace Gameplay {
    public class LedgeDetection : MonoBehaviour {
        [Range(0, 1)]
        [SerializeField] private float radius;

        [SerializeField] private LayerMask whatIsGround;
        [SerializeField] private PlayerController player;
        [SerializeField] private KeyCode getUpKey = KeyCode.W;

        // Offset detection ledge on flip
        private const float LedgeDetectionOffsetOnFlip = 0.4f;

        [SerializeField] private GameObject ledgeCheck;

        private bool _isNearLedge;

        private void Update() {
            _isNearLedge = IsLedge();

            if (_isNearLedge && Input.GetKeyDown(getUpKey)) {
                ClimbLedge();
            }

            UpdateLedgeCheckPosition();
        }

        private bool IsLedge() {
            Vector2 spherePosition = ledgeCheck.transform.position;

            RaycastHit2D hitDown = Physics2D.Raycast(spherePosition, Vector2.down, radius, whatIsGround);

            RaycastHit2D hitUp = Physics2D.Raycast(spherePosition, Vector2.up, radius, whatIsGround);

            return hitDown.collider != null && hitUp.collider == null;
        }

        private void ClimbLedge() {
            Vector3 ledgePosition = ledgeCheck.transform.position;
            player.transform.position = new Vector3(ledgePosition.x, ledgePosition.y + 1f, player.transform.position.z);
        }

        private void UpdateLedgeCheckPosition() {
            var ledgeCheckPosition = ledgeCheck.transform.localPosition;

            if (player.isFacingRight) {
                ledgeCheck.transform.localPosition = new Vector3(
                    Mathf.Abs(ledgeCheckPosition.x),
                    ledgeCheckPosition.y,
                    ledgeCheckPosition.z);
            }
            else {
                ledgeCheck.transform.localPosition = new Vector3(
                    -Mathf.Abs(ledgeCheckPosition.x),
                    ledgeCheckPosition.y,
                    ledgeCheckPosition.z);
            }
        }

        private void OnDrawGizmos() {
            Gizmos.DrawWireSphere(ledgeCheck.transform.position, radius);

            Gizmos.color = Color.green;
            Gizmos.DrawLine(ledgeCheck.transform.position,
                ledgeCheck.transform.position + Vector3.down * radius);

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(ledgeCheck.transform.position,
                ledgeCheck.transform.position + Vector3.up * radius);
        }
    }
}