using Controllers;
using UnityEngine;
using static Utils.TagUtils;
using static Utils.LayerUtils;

namespace Gameplay {
    [RequireComponent(typeof(Collider2D))]
    public class LedgeDetection : MonoBehaviour {
        [SerializeField] private PlayerController player;
        [SerializeField] private GameObject ledgeCheck;

        [Header("Debug")]
        [SerializeField] private bool drawObjects;

        public bool isNearLedge;
        public bool isNearWall;
        public bool isCloseToClimbableWall;
        private Collider2D _detectedLedge;

        private void Awake() {
            if (player == null) {
                player = GetComponentInParent<PlayerController>();
            }

            if (ledgeCheck == null) {
                Debug.LogError("LedgeDetection script requires a ledgeCheck object");
                enabled = false;
            }
        }

        private void Update() {
            UpdateLedgeCheckPosition();
            UpdateWallCheck();
        }

        private void UpdateWallCheck() {
            Vector2 direction = player.isFacingRight ? Vector2.right : Vector2.left;
            Vector2 origin = ledgeCheck.transform.position;

            RaycastHit2D hit = Physics2D.Raycast(origin, direction, 0.5f, Ground);
            isNearWall = hit.collider != null;

            Debug.DrawRay(origin, direction * 0.5f, isNearWall ? Color.green : Color.red);
        }

        private void UpdateLedgeCheckPosition() {
            Vector3 ledgeCheckPosition = ledgeCheck.transform.localPosition;
            ledgeCheck.transform.localPosition = new Vector3(
                player.isFacingRight ? Mathf.Abs(ledgeCheckPosition.x) : -Mathf.Abs(ledgeCheckPosition.x),
                ledgeCheckPosition.y,
                ledgeCheckPosition.z
            );
        }

        private void OnTriggerEnter2D(Collider2D collision) {
            if ((GetBitMask(collision.gameObject.layer) & Ground.value) != 0) {
                if (IsValidLedge()) {
                    _detectedLedge = collision;
                    isNearLedge = true;
                }
            }

            if (collision.CompareTag(ClimbableWall)) {
                isCloseToClimbableWall = true;
            }
        }

        private void OnTriggerExit2D(Collider2D collision) {
            if (collision == _detectedLedge) {
                isNearLedge = false;
                _detectedLedge = null;
            }

            if (collision.CompareTag(ClimbableWall)) {
                isCloseToClimbableWall = false;
            }
        }

        private bool IsValidLedge() {
            const float rayLength = 0.4f;
            Vector3 ledgePosition = ledgeCheck.transform.position;
            Vector2 checkAbove = ledgePosition + Vector3.up * rayLength;
            Collider2D hitAbove = Physics2D.OverlapCircle(checkAbove, transform.localScale.x, Ground);

            // Ledge candidate
            if (hitAbove == null) {
                // This is a ledge if there is no ground above and there is ground on the side (depending on the player direction)
                if (!IsGroundAbove() && IsGroundOnSide()) {
                    return true;
                }
            }

            return false;
        }

        private bool IsGroundOnSide(float rayLength = 0.4f) {
            float radius = transform.localScale.x;
            Vector2 floorPoint = player.transform.position +
                                 (player.isFacingRight ? Vector3.right : Vector3.left) * rayLength;
            return Physics2D.OverlapCircle(floorPoint, radius, Ground) != null;
        }

        public bool IsGroundAbove(float rayLength = 0.4f) {
            Vector3 ledgePosition = ledgeCheck.transform.position;
            Vector2 ceilingPoint = ledgePosition + Vector3.up * rayLength;
            return Physics2D.OverlapCircle(ceilingPoint, transform.localScale.x, Ground) != null;
        }

        private void OnDrawGizmosSelected() {
            if (!drawObjects || ledgeCheck == null) {
                return;
            }

            Vector3 ledgePosition = ledgeCheck.transform.position;
            const float rayLength = 0.4f;
            float radius = transform.localScale.x;

            Vector2 checkAbove = ledgePosition + Vector3.up * rayLength;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(checkAbove, radius);

            Vector2 checkSide = player.transform.position +
                                (player.isFacingRight ? Vector3.right : Vector3.left) * transform.localScale.x;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(checkSide, radius);
        }
    }
}