using Controllers;
using Enums;
using UnityEngine;
using static Utils.AnimatorUtils;
using static PlayerInput.KeyBinds;
using static Utils.LayerUtils;

namespace Mechanics.Movement {
    public class HoldV2 : MonoBehaviour {
        [SerializeField, Range(0, 1)]
        private float onHoldXOffset, onHoldYOffset;

        [Header("On finish climb ledge")]
        [SerializeField]
        private Vector2 onFinishClimbLedgeOffset = new(0f, 0f);

        [SerializeField]
        private Vector2 extraOffsetOnWallClimb = new(0f, 0f);

        [Header("Ledge Detection")]
        [SerializeField]
        private bool displayGizmos;

        [SerializeField, Range(0, 1)]
        private float redXOffset, redYOffset, redXSize, redYSize, greenXOffset, greenYOffset, greenXSize, greenYSize;

        [SerializeField, Range(0, 1)]
        private float ledgeCeilingCheckHeight = 0.3f;

        private Rigidbody2D _rb;
        private float _startingGrav;
        private bool _greenBox, _redBox;
        private bool _playerWasWallClimbing;

        private bool _isGrabbing;
        private Animator _animator;
        private PlayerController _player;

        public void Start() {
            _rb = GetComponent<Rigidbody2D>();
            _animator = GetComponent<Animator>();
            _startingGrav = _rb.gravityScale;
            _player = GetComponent<PlayerController>();
        }

        private void Update() {
            if (_isGrabbing && (GetJumpKeyDown() || GetUpKeyDown())) {
                ClimbLedge();
            }

            _greenBox = Physics2D.OverlapBox(new Vector2(transform.position.x + GetXOffset(greenXOffset),
                transform.position.y + greenYOffset), new Vector2(greenXSize, greenYSize), 0f, Ground.value);

            _redBox = Physics2D.OverlapBox(new Vector2(transform.position.x + GetXOffset(redXOffset),
                transform.position.y + redYOffset), new Vector2(redXSize, redYSize), 0f, Ground.value);

            if (_greenBox && !_redBox && !_isGrabbing && !_player.IsGrounded && !IsCeilingAbove()) {
                StartHold();
            }
        }

        private void StartHold() {
            if (_isGrabbing) {
                return;
            }
            _isGrabbing = true;

            if (_player.movementState == PlayerMovementState.WallClimb) {
                _playerWasWallClimbing = true;
            }

            // To ensure that map is not colliding with player during the ledge climb
            _player.boxCollider.isTrigger = true;
            _rb.gravityScale = 0f;
            _rb.velocity = Vector2.zero;
            _player.AddPosition(GetXOffset(onHoldXOffset), onHoldYOffset);
            _animator.SetBool(IsHolding, true);
            _player.SetMovementState(PlayerMovementState.Hold, 5);
            _player.FreezePosition();
        }

        private void ClimbLedge() {
            _player.UnlockMovementState();
            _player.SetMovementState(PlayerMovementState.Climb, 2);

            // This is to avoid displaying the falling animation
            _player.IsGrounded = true;

            // This starts the ledge climb animation
            _animator.SetBool(IsHolding, false);
        }

        private bool IsCeilingAbove() {
            Vector2 origin = new Vector2(transform.position.x, transform.position.y + _player.boxCollider.bounds.extents.y);
            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.up, ledgeCeilingCheckHeight, Ground.value);
            return hit.collider != null;
        }

        public void OnFinishClimbLedge() {
            // Set player grounded since it will be grounded after the ledge climb, this is to prevent the player falling animation
            _player.IsGrounded = true;
            _isGrabbing = false;
            _rb.gravityScale = _startingGrav;
            _rb.velocity = Vector2.zero;

            // When the player was wall climbing, we need to add the extra offset to the player position since the detection
            // was made a bit earlier and the player is not in on the exact same position
            if (_playerWasWallClimbing) {
                _player.AddPosition(GetXOffset(extraOffsetOnWallClimb.x), extraOffsetOnWallClimb.y);
            } else {
                _player.AddPosition(GetXOffset(onFinishClimbLedgeOffset.x), onFinishClimbLedgeOffset.y);
            }

            _playerWasWallClimbing = false;
            _player.UnlockMovementState();
            _player.boxCollider.isTrigger = false;
            _player.FreezePosition(false);
        }

        private void OnDrawGizmosSelected() {
            if (!displayGizmos) {
                return;
            }

            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(new Vector2(transform.position.x + GetXOffset(redXOffset),
                transform.position.y + redYOffset), new Vector2(redXSize, redYSize));
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(new Vector2(transform.position.x + GetXOffset(greenXOffset),
                transform.position.y + greenYOffset), new Vector2(greenXSize, greenYSize));
        }

        private float GetXOffset(float offset) {
            if (_player == null) {
                _player = GetComponent<PlayerController>();
            }

            bool isFacingRight = _player.isFacingRight;
            return isFacingRight ? offset : -offset;
        }
    }
}