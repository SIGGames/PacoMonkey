using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Platformer.Gameplay;
using static Platformer.Core.Simulation;
using Platformer.Model;
using Platformer.Core;
using Platformer.Mechanics.Enums;

namespace Platformer.Mechanics {
    /// <summary>
    /// This is the main class used to implement control of the player.
    /// It is a superset of the AnimationController class, but is inlined to allow for any kind of customisation.
    /// </summary>
    public class PlayerController : KinematicObject, IMechanics {
        // TODO This will be removed when the mechanics are implemented
        public AudioClip jumpAudio;
        public AudioClip respawnAudio;
        public AudioClip ouchAudio;

        /// <summary>
        /// Max horizontal speed of the player.
        /// </summary>
        public float maxSpeed = 7;

        /// <summary>
        /// Initial jump velocity at the start of a jump.
        /// </summary>
        public float jumpTakeOffSpeed = 7;

        public JumpState jumpState = JumpState.Grounded;

        private bool _stopJump;

        /*internal new*/
        public Collider2D collider2d;

        /*internal new*/
        public AudioSource audioSource;
        public Health health;
        public bool controlEnabled = true;

        private bool _jump;
        private Vector2 _move;
        private SpriteRenderer _spriteRenderer;
        internal Animator animator;
        private readonly PlatformerModel _model = Simulation.GetModel<PlatformerModel>();

        public Bounds Bounds => collider2d.bounds;

        void Awake() {
            health = GetComponent<Health>();
            audioSource = GetComponent<AudioSource>();
            collider2d = GetComponent<Collider2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
        }

        protected override void Update() {
            if (controlEnabled) {
                _move.x = Input.GetAxis("Horizontal");
                if (jumpState == JumpState.Grounded && Input.GetButtonDown("Jump"))
                    jumpState = JumpState.PrepareToJump;
                else if (Input.GetButtonUp("Jump")) {
                    _stopJump = true;
                    Schedule<PlayerStopJump>().player = this;
                }
            }
            else {
                _move.x = 0;
            }

            UpdateJumpState();
            base.Update();
        }

        void UpdateJumpState() {
            _jump = false;
            switch (jumpState) {
                case JumpState.PrepareToJump:
                    jumpState = JumpState.Jumping;
                    _jump = true;
                    _stopJump = false;
                    break;
                case JumpState.Jumping:
                    if (!IsGrounded) {
                        Schedule<PlayerJumped>().player = this;
                        jumpState = JumpState.InFlight;
                    }

                    break;
                case JumpState.InFlight:
                    if (IsGrounded) {
                        Schedule<PlayerLanded>().player = this;
                        jumpState = JumpState.Landed;
                    }

                    break;
                case JumpState.Landed:
                    jumpState = JumpState.Grounded;
                    break;
            }
        }

        protected override void ComputeVelocity() {
            if (_jump && IsGrounded) {
                velocity.y = jumpTakeOffSpeed * _model.jumpModifier;
                _jump = false;
            }
            else if (_stopJump) {
                _stopJump = false;
                if (velocity.y > 0) {
                    velocity.y = velocity.y * _model.jumpDeceleration;
                }
            }

            if (_move.x > 0.01f)
                _spriteRenderer.flipX = false;
            else if (_move.x < -0.01f)
                _spriteRenderer.flipX = true;

            animator.SetBool("grounded", IsGrounded);
            animator.SetFloat("velocityX", Mathf.Abs(velocity.x) / maxSpeed);

            targetVelocity = _move * maxSpeed;
        }

        public void Jump() {
            throw new System.NotImplementedException();
        }

        public void Idle() {
            throw new System.NotImplementedException();
        }

        public void Walk() {
            throw new System.NotImplementedException();
        }

        public void Run() {
            throw new System.NotImplementedException();
        }

        public void Crouch() {
            throw new System.NotImplementedException();
        }

        public void Climb() {
            throw new System.NotImplementedException();
        }
    }
}