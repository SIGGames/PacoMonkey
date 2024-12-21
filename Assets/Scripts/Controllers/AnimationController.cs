using Mechanics;
using UnityEngine;

namespace Controllers {
    [RequireComponent(typeof(Animator))]
    public class AnimationController : KinematicObject {
        public float maxSpeed = 7;

        public float jumpTakeOffSpeed = 7;

        public Vector2 move;

        public bool jump;

        public bool stopJump;

        Animator animator;
        private PlayerController player;

        protected virtual void Awake() {
            animator = GetComponent<Animator>();
            player = GetComponent<PlayerController>();
        }

        protected override void ComputeVelocity() {
            if (jump && IsGrounded) {
                velocity.y = jumpTakeOffSpeed * player.jumpModifier;
                jump = false;
            }
            else if (stopJump) {
                stopJump = false;
                if (velocity.y > 0) {
                    velocity.y *= player.jumpDeceleration;
                }
            }

            animator.SetBool("grounded", IsGrounded);
            animator.SetFloat("velocityX", Mathf.Abs(velocity.x) / maxSpeed);

            targetVelocity = move * maxSpeed;
        }
    }
}