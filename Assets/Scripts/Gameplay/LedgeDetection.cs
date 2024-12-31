using Controllers;
using UnityEngine;

namespace Gameplay {
    [RequireComponent(typeof(Collider2D))]
    public class LedgeDetection : MonoBehaviour {
        [SerializeField] private LayerMask whatIsGround;
        [SerializeField] private PlayerController player;

        [SerializeField] private GameObject ledgeCheck;

        public bool isNearLedge;
        public bool isNearWall;
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

            RaycastHit2D hit = Physics2D.Raycast(origin, direction, 0.5f, whatIsGround);
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
            if (((1 << collision.gameObject.layer) & whatIsGround) != 0) {
                _detectedLedge = collision;
                if (IsValidLedge()) {
                    isNearLedge = true;
                }
            }
        }

        private void OnTriggerExit2D(Collider2D collision) {
            if (collision == _detectedLedge) {
                isNearLedge = false;
                _detectedLedge = null;
            }
        }

        private bool IsValidLedge() {
            Vector3 ledgePosition = ledgeCheck.transform.position;
            Vector2 topPoint = ledgePosition + Vector3.up * 0.1f;
            Collider2D hitUp = Physics2D.OverlapCircle(topPoint, 0.1f, whatIsGround);

            return hitUp == null;
        }
    }
}