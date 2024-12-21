using Mechanics;
using UnityEngine;

namespace Controllers {
    [RequireComponent(typeof(Animator))]
    public sealed class AnimationController : KinematicObject {
        public float maxSpeed = 7;

        public float jumpTakeOffSpeed = 7;

        public float jumpDeceleration = 0.5f;

        public float jumpModifier = 1f;

        public Vector2 move;

        public bool jump;

        public bool stopJump;

        Animator _animator;

        private void Awake() {
            _animator = GetComponent<Animator>();
        }

        protected override void ComputeVelocity() {
            if (jump && IsGrounded) {
                velocity.y = jumpTakeOffSpeed * jumpModifier;
                jump = false;
            } else if (stopJump) {
                stopJump = false;
                if (velocity.y > 0) {
                    velocity.y *= jumpDeceleration;
                }
            }

            _animator.SetBool("grounded", IsGrounded);
            _animator.SetFloat("velocityX", Mathf.Abs(velocity.x) / maxSpeed);

            targetVelocity = move * maxSpeed;
        }
    }
}