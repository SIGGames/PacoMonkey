using Controllers;
using Enums;
using NaughtyAttributes;
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
        private float redXOffset, redXSize, redYOffset, redYSize;

        [SerializeField, Range(0, 1)]
        private float greenXOffset, greenXSize, greenYOffset, greenYSize;

        [SerializeField, Range(0, 1)]
        private float ledgeCeilingCheckHeight = 0.3f;

        private Rigidbody2D _rb;
        private float _startingGrav;
        private bool _greenBox, _redBox;
        private bool _playerWasWallClimbing;
        private bool _isGrabbing;
        private bool _hasStartedClimb;

        private Animator _animator;
        private WallClimb _wallClimb;
        private PlayerController _player;

        public void Start() {
            _rb = GetComponent<Rigidbody2D>();
            _animator = GetComponent<Animator>();
            _startingGrav = _rb.gravityScale;
            _player = GetComponent<PlayerController>();
            _wallClimb = GetComponent<WallClimb>();
        }

        private void Update() {
            if (_player.movementState == PlayerMovementState.Jump || !_player.lives.IsAlive) {
                _isGrabbing = false;
            }

            if (_isGrabbing && (GetJumpKeyDown() || GetUpKeyDown())) {
                if (!_hasStartedClimb) {
                    _hasStartedClimb = true;
                    ClimbLedge();
                }
            }
        }

        private void FixedUpdate() {
            if (_isGrabbing || _player.movementState == PlayerMovementState.Climb || _player.IsGrounded) {
                return;
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
                if (_wallClimb != null) {
                    _wallClimb.StopClimbing();
                }
            }

            // To ensure that map is not colliding with player during the ledge climb
            _player.boxCollider.isTrigger = true;

            _rb.gravityScale = 0f;
            _player.SetVelocity(Vector2.zero);
            _player.SetBodyType(RigidbodyType2D.Static);
            _player.SetPosition(GetSnapHoldPoint());
            CorrectVisualHoldOffset();
            _animator.SetBool(IsHolding, true);
            _player.SetMovementState(PlayerMovementState.Hold, 5);
            _player.FreezePosition();
        }

        private void ClimbLedge() {
            _player.UnlockMovementState();
            _player.SetMovementState(PlayerMovementState.Climb, 2);

            // This is to avoid displaying the falling animation
            _player.IsGrounded = true;

            // This starts the ledge climb animation (anti intuitive, I know)
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
            _player.SetVelocity(Vector2.zero);

            // When the player was wall climbing, we need to add the extra offset to the player position since the detection
            // was made a bit earlier and the player is not in on the exact same position
            if (_playerWasWallClimbing) {
                _player.AddPosition(GetXOffset(extraOffsetOnWallClimb.x), extraOffsetOnWallClimb.y);
            } else {
                _player.AddPosition(GetXOffset(onFinishClimbLedgeOffset.x), onFinishClimbLedgeOffset.y);
            }

            _isGrabbing = false;
            _hasStartedClimb = false;
            _playerWasWallClimbing = false;
            _player.UnlockMovementState();
            _player.boxCollider.isTrigger = false;
            _player.FreezePosition(false);
        }

        private Vector2 GetSnapHoldPoint() {
            float snapX = transform.position.x + GetXOffset(onHoldXOffset);
            float snapY = transform.position.y + onHoldYOffset;
            return new Vector2(snapX, snapY);
        }

        private void CorrectVisualHoldOffset() {
            // This method is intended to fix visual misalignment of the player when holding with the wall
            const float maxRaycastDistance = 0.5f;
            Vector2 direction = _player.isFacingRight ? Vector2.right : Vector2.left;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, maxRaycastDistance, Ground.value);

            if (hit.collider != null) {
                // Its multiplied by 2 since the hit distance and the goal correction position are not the same
                float correction = hit.distance * 2f;
                float signedCorrection = _player.isFacingRight ? correction : -correction;
                // If it's greater than this, it means that it's a false positive and we don't want to correct it
                if (correction < 0.3f) {
                    _player.AddPosition(signedCorrection);
                }
            }
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

            return _player.isFacingRight ? offset : -offset;
        }

        public void ResetHold() {
            // This method is used to reset the hold state when the player has ben reset
            _isGrabbing = false;
            _hasStartedClimb = false;
            _playerWasWallClimbing = false;
            _rb.gravityScale = _startingGrav;
        }
    }
}