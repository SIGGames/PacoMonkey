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

        [SerializeField, Range(0, 1)]
        private float redXOffset, redYOffset, redXSize, redYSize, greenXOffset, greenYOffset, greenXSize, greenYSize;

        [SerializeField]
        private bool displayGizmos;

        private Rigidbody2D _rb;
        private float _startingGrav;
        private bool _greenBox, _redBox;

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

            if (_greenBox && !_redBox && !_isGrabbing && !_player.IsGrounded) {
                StartHold();
            }
        }

        private void StartHold() {
            if (_isGrabbing) {
                return;
            }
            _isGrabbing = true;

            // To ensure that map is not colliding with player during the ledge climb
            _player.boxCollider.isTrigger = true;
            _rb.bodyType = RigidbodyType2D.Kinematic;
            _rb.gravityScale = 0f;
            _rb.velocity = Vector2.zero;

            _player.AddPosition(GetXOffset(onHoldXOffset), onHoldYOffset);
            _animator.SetBool(IsHolding, true);
            _player.SetMovementState(PlayerMovementState.Hold, 2);
            _player.FreezePosition();
        }

        private void ClimbLedge() {
            _player.UnlockMovementState();
            _player.SetMovementState(PlayerMovementState.Climb, 2);

            // This starts the ledge climb animation
            _animator.SetBool(IsHolding, false);
        }

        public void OnFinishClimbLedge() {
            // Set player grounded since it will be grounded after the ledge climb, this is to prevent the player falling animation
            _player.IsGrounded = true;
            _isGrabbing = false;
            _rb.bodyType = RigidbodyType2D.Kinematic;
            _rb.gravityScale = _startingGrav;
            _rb.velocity = Vector2.zero;
            _player.boxCollider.isTrigger = false;

            _player.UnlockMovementState();
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