using Enums;
using Platformer.Mechanics;
using static Mechanics.Utils.Keybinds;
using UnityEngine;

namespace Mechanics {
    public class PacoSmallMovement : MonoBehaviour {
        private Rigidbody2D _rb;
        private Animator _animator;
        private const float WalkSpeed = 2f;
        private const float RunSpeed = 6f;
        private bool _isFacingRight = true;

        private PlayerMovementState _currentState = PlayerMovementState.Idle;

        protected void Start() {
            _rb = GetComponent<Rigidbody2D>();
            _animator = GetComponent<Animator>();
        }

        protected void Update() {
            HandleMovement();
            HandleJump();
            HandleCrouch();
            HandleIdle();
        }

        protected virtual void HandleMovement() {
            var move = Input.GetAxis("Horizontal");

            if (move == 0) return;
            if (GetWalkKey()) {
                Walk(move);
            }
            else {
                Run(move);
            }

            switch (move) {
                case > 0 when !_isFacingRight:
                case < 0 when _isFacingRight:
                    Flip();
                    break;
            }
        }

        protected virtual void HandleIdle() {
            if (Input.GetAxis("Horizontal") != 0 || GetIdleKey()) return;
            _currentState = PlayerMovementState.Idle;
            // _animator.Play("Idle");
        }

        protected virtual void Walk(float direction) {
            _currentState = PlayerMovementState.Walk;
            _rb.velocity = new Vector2(direction * WalkSpeed, _rb.velocity.y);
            // _animator.Play("Walk");
        }

        protected virtual void Run(float direction) {
            _currentState = PlayerMovementState.Run;
            _rb.velocity = new Vector2(direction * RunSpeed, _rb.velocity.y);
            // _animator.Play("Run");
        }

        protected virtual void Flip() {
            _isFacingRight = !_isFacingRight;
            var scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }

        protected virtual void HandleJump() {
            if (GetJumpKey()) {
                Jump();
            }
        }

        private void Jump() {
            if (_currentState != PlayerMovementState.Jump) {
                _currentState = PlayerMovementState.Jump;
                _rb.velocity = new Vector2(_rb.velocity.x, 5f);
                // _animator.Play("Jump");
            }
            else if (_currentState == PlayerMovementState.Jump) {
                _currentState = PlayerMovementState.DoubleJump;
                _rb.velocity = new Vector2(_rb.velocity.x, 4f);
                // _animator.Play("DoubleJump");
            }
        }

        protected virtual void HandleCrouch() {
            if (GetCrouchKey()) {
                Crouch();
            }
        }

        private void Crouch() {
            _currentState = PlayerMovementState.Crouch;
            _rb.velocity = new Vector2(0, _rb.velocity.y);
            // _animator.Play("Crouch");
        }

        public void Climb() {
            _currentState = PlayerMovementState.Climb;
            // TODO: Implement climbing
        }

        public void Stealth() {
            _currentState = PlayerMovementState.Hold;
            // TODO: Implement stealth
        }
    }
}
