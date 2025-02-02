using UnityEngine;

namespace Utils {
    public static class AnimatorUtils {
        public static readonly int Grounded = Animator.StringToHash("grounded");
        public static readonly int IsClimbing = Animator.StringToHash("isClimbing");
        public static readonly int IsCrouching = Animator.StringToHash("isCrouching");
        public static readonly int IsFlipping = Animator.StringToHash("isFlipping");
        public static readonly int IsHolding = Animator.StringToHash("isHolding");
        public static readonly int IsHoldingJumpOnClimb = Animator.StringToHash("isHoldingJumpOnClimb");
        public static readonly int IsJumping = Animator.StringToHash("isJumping");
        public static readonly int IsMicca1 = Animator.StringToHash("isMicca1");
        public static readonly int IsTowardsUp = Animator.StringToHash("isTowardsUp");
        public static readonly int MeleeAttack = Animator.StringToHash("meleeAttack");
        public static readonly int RangedAttack = Animator.StringToHash("rangedAttack");
        public static readonly int StartProjectile = Animator.StringToHash("startProjectile");
        public static readonly int VelocityX = Animator.StringToHash("velocityX");
        public static readonly int VelocityY = Animator.StringToHash("velocityY");
    }
}