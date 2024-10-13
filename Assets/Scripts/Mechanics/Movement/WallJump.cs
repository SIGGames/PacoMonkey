using System;
using Mechanics.Utils;
using UnityEngine;

namespace Mechanics.Movement {
    [RequireComponent(typeof(PlayerController))]
    public class WallJump : MonoBehaviour {
        [SerializeField] private bool isWallSliding;

        [Range(0, 10)]
        [SerializeField] private float wallSlideSpeed = 3f;

        [SerializeField] private bool isWallJumping;

        [Range(0.01f, 1)]
        [SerializeField] private float wallJumpingTime = 0.2f;

        [SerializeField] private float wallJumpingCounter;
        [SerializeField] private Vector2 wallJumpingPower = new Vector2(8f, 16f);

        [SerializeField] private Transform wallCheck;
        [SerializeField] private LayerMask wallLayer;

        [Range(0.01f, 1)]
        [SerializeField] private float wallCheckRadius = 0.2f;

        private int _wallJumpingDirection;
        private PlayerController _playerController;
        private Rigidbody2D _rb;

        private void Awake() {
            _playerController = GetComponent<PlayerController>();
            _rb = GetComponent<Rigidbody2D>();
        }

        private void Update() {
            WallSlide();
            WallJumping();
        }

        private bool IsWalled() {
            return Physics2D.OverlapCircle(wallCheck.position, wallCheckRadius, wallLayer);
        }

        private void WallSlide() {
            if (IsWalled() && !_playerController.IsGrounded && _playerController.move.x != 0f) {
                isWallSliding = true;
                var velocity = _rb.velocity;
                _rb.velocity = new Vector2(velocity.x, Mathf.Clamp(velocity.y, -wallSlideSpeed, float.MaxValue));
            }
            else {
                isWallSliding = false;
            }
        }

        private void WallJumping() {
            if (isWallSliding) {
                isWallJumping = false;
                _wallJumpingDirection = _playerController.IsFacingRight() ? -1 : 1;
                wallJumpingCounter = wallJumpingTime;
            }
            else {
                wallJumpingCounter -= Time.deltaTime;
            }

            if (Keybinds.GetJumpKey() && wallJumpingCounter > 0) {
                isWallJumping = true;
                _rb.velocity = new Vector2(wallJumpingPower.x * _wallJumpingDirection, wallJumpingPower.y);
                wallJumpingCounter = 0f;

                if (_playerController.IsFacingRight() != (_wallJumpingDirection == 1)) {
                    _playerController.Flip();
                }
            }

            if (wallJumpingCounter <= 0 && isWallJumping) {
                StopWallJumping();
            }
        }

        private void StopWallJumping() {
            isWallJumping = false;
        }
    }
}