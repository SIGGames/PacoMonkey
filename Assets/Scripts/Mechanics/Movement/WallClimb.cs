using Controllers;
using Enums;
using Gameplay;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using Utils;
using static PlayerInput.KeyBinds;

namespace Mechanics.Movement {
    [RequireComponent(typeof(PlayerController))]
    public class WallClimb : MonoBehaviour {
        [Header("Wall Climbing Settings")]
        [SerializeField] private float climbSpeed = 2f;

        [TagSelector] [SerializeField] private string wallTag = "ClimbableWall";

        [Range(0, 5)]
        [SerializeField] private float slideEffect = 5f;

        [Tooltip("Offset applied to the player when attached to the wall")]
        [SerializeField] private Vector2 attachOffset = new(0.2f, 0.3f);

        [Header("References")]
        [SerializeField] private LedgeDetection ledgeCheck;

        [SerializeField] private TilemapCollider2D climbableTilemap;

        [SerializeField] private PlayerController player;

        private bool _isClimbing;
        private bool _canAttach;
        private float _gravityModifier;

        private Animator _animator;
        private Collider2D _playerCollider;
        private Hold _hold;
        private static readonly int IsClimbing = Animator.StringToHash("isClimbing");
        private static readonly int IsTowardsUp = Animator.StringToHash("isTowardsUp");

        private void Awake() {
            if (player == null) {
                player = GetComponent<PlayerController>();
            }

            if (ledgeCheck == null) {
                ledgeCheck = GetComponentInChildren<LedgeDetection>();
            }

            if (_hold == null) {
                _hold = GetComponent<Hold>();
            }

            _animator = GetComponent<Animator>();
            _playerCollider = player.collider2d;

            if (climbableTilemap == null) {
                climbableTilemap = FindObjectOfType<TilemapCollider2D>();
            }

            _gravityModifier = player.gravityModifier;

            if (player == null || ledgeCheck == null || _playerCollider == null || climbableTilemap == null ||
                _hold == null) {
                Debugger.Log(("Player", player), ("LedgeCheck", ledgeCheck),
                    ("PlayerCollider", _playerCollider), ("ClimbableTilemap", climbableTilemap), ("Hold", _hold));
                enabled = false;
            }
        }

        private void Update() {
            if (ledgeCheck.isNearLedge) {
                _hold.StartHold();
            }

            if (!_isClimbing && _canAttach && ledgeCheck.isNearWall && GetUpKey()) {
                StartClimbing();
            } else if (_isClimbing) {
                HandleClimbing();
            }
        }

        private void StartClimbing() {
            _isClimbing = true;

            player.FreezeHorizontalPosition();

            // TODO: Flip the player when player prepares to jump
            //player.flipManager.Flip(!player.isFacingRight);

            SetClimbingState(true);

            float offsetX = player.isFacingRight ? attachOffset.x : -attachOffset.x;
            player.AddPosition(offsetX, attachOffset.y);

            player.SetMovementState(PlayerMovementState.WallClimb, 3);
            _animator.SetBool(IsClimbing, true);
        }

        private void HandleClimbing() {
            float verticalInput = GetVerticalAxis();

            if (verticalInput != 0) {
                player.AddPosition(0, climbSpeed * Time.deltaTime * verticalInput);
                player.velocity.y = climbSpeed * verticalInput;
            } else {
                player.velocity.y = 0f;
            }

            if (player.velocity.y <= 0f) {
                _animator.SetFloat(IsTowardsUp, -1f);
            } else {
                _animator.SetFloat(IsTowardsUp, 1f);
            }

            // In case there is a ledge or the player wants to stop climbing
            if (AnyStopClimbKeyIsPressed() || !ledgeCheck.isNearWall || ledgeCheck.isNearLedge) {
                StopClimbing();
            }
        }

        private bool AnyStopClimbKeyIsPressed() {
            return GetJumpKeyDown();
        }

        private void StopClimbing() {
            _isClimbing = false;
            _canAttach = false;

            player.FreezeHorizontalPosition(false);
            SetClimbingState(false);
            player.UnlockMovementState();

            _animator.SetBool(IsClimbing, false);
        }

        private void OnTriggerEnter2D(Collider2D collision) {
            if (collision.CompareTag(wallTag)) {
                _canAttach = true;
            }
        }

        private void OnTriggerExit2D(Collider2D collision) {
            if (collision.CompareTag(wallTag)) {
                _canAttach = false;
            }
        }


        private void SetClimbingState(bool isClimbing) {
            Physics2D.IgnoreLayerCollision(player.gameObject.layer, LayerMask.NameToLayer("Ground"), isClimbing);

            if (isClimbing) {
                SetColliderEnabledStatus(false);
                SetGravity(slideEffect);
            } else {
                SetColliderEnabledStatus(true);
                SetGravity(_gravityModifier * 100);
            }
        }

        private void SetColliderEnabledStatus(bool value) {
            climbableTilemap.isTrigger = !value;
        }

        private void SetGravity(float value) {
            if (value > 0) {
                player.gravityModifier = value / 100;
            } else {
                player.gravityModifier = 0;
            }

            player.rb.velocity = Vector2.zero;
            player.velocity = Vector2.zero;
        }
    }
}