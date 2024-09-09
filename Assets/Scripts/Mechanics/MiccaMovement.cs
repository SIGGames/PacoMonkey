using UnityEngine;

namespace Platformer.Mechanics {
    public class MiccaMovement : MonoBehaviour {
        private Rigidbody2D _rb;
        private Animator _animator;
        protected float moveSpeed = 5f;
        private const float WalkSpeed = 3f;
        private const float RunSpeed = 6f;
        private bool _isFacingRight = true;

        private PlayerMovementState _currentState = PlayerMovementState.Idle;

        protected virtual void Start() {
            _rb = GetComponent<Rigidbody2D>();
            _animator = GetComponent<Animator>();
        }

        protected virtual void Update() {
            HandleMovement();
            HandleJump();
            HandleCrouch();
            HandleIdle();
        }

        protected virtual void HandleMovement() {
            var move = Input.GetAxis("Horizontal");

            if (move == 0) return;
            if (Input.GetKey(KeyCode.LeftShift)) {
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
            if (Input.GetAxis("Horizontal") != 0 || Input.GetKey(KeyCode.S)) return;
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
            if (Input.GetKeyDown(KeyCode.Space)) {
                Jump();
            }
        }

        private void Jump() {
            if (_currentState == PlayerMovementState.Jump || !IsGrounded()) return;
            _currentState = PlayerMovementState.Jump;
            _rb.velocity = new Vector2(_rb.velocity.x, 7f);
            // _animator.Play("Jump");
        }

        protected virtual bool IsGrounded() {
            // TODO: Implement grounded check
            return true;
        }

        protected virtual void HandleCrouch() {
            if (Input.GetKey(KeyCode.S)) {
                Crouch();
            }
        }

        private void Crouch() {
            _currentState = PlayerMovementState.Crouch;
            _rb.velocity = new Vector2(0, _rb.velocity.y);
            _animator.Play("Crouch");
        }

        public void Climb() {
            // TODO: Implement climbing
        }
    }
}