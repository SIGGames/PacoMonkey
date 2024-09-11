using Configuration;
using Enums;
using Model;
using Platformer.Core;
using Platformer.Gameplay;
using Platformer.Mechanics;
using UnityEngine;
using UnityEngine.Serialization;
using static Platformer.Core.Simulation;
using static Configuration.GlobalConfiguration;

namespace Mechanics {
    public class PlayerController : KinematicObject, IMechanics {
        public AudioClip jumpAudio;
        public AudioClip respawnAudio;
        public AudioClip ouchAudio;

        public float maxSpeed = 7;
        public float walkSpeedMultiplier = 0.33f;
        public float crouchSpeedMultiplier = 0.5f;
        public float jumpTakeOffSpeed = 7;

        public JumpState jumpState = JumpState.Grounded;
        private bool _stopJump;

        private const float MovementThreshold = 0.01f;

        public Collider2D collider2d;
        public AudioSource audioSource;
        public Health health;
        public bool controlEnabled = true;
        public bool canClimb;

        private bool _jump;
        private float _jumpTimeCounter;
        private const float JumpTimeMax = 1.0f;
        private bool _isCrouching;
        private bool _isWalking;
        private bool _isClimbing;
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

                // Handle Jump
                if (jumpState == JumpState.Grounded && Input.GetButtonDown("Jump"))
                    jumpState = JumpState.PrepareToJump;
                else if (Input.GetButtonUp("Jump")) {
                    _stopJump = true;
                    Schedule<PlayerStopJump>().player = this;
                }

                // Handle Walk
                if (Input.GetKey(KeyCode.LeftShift)) {
                    Walk();
                }
                else {
                    Walk(false);
                }

                // Handle Crouch
                if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) {
                    Crouch();
                }
                else {
                    Crouch(false);
                }

                if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) && canClimb) {
                    Climb();
                }
                else {
                    Climb(false);
                }
            }
            else {
                _move.x = 0;
            }

            UpdateJumpState();
            base.Update();
        }

        void UpdateJumpState() {
            Jump(false);
            switch (jumpState) {
                case JumpState.PrepareToJump:
                    jumpState = JumpState.Jumping;
                    Jump();
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
                Jump();
                _jumpTimeCounter = 0;
            }
            else if (_jump && _jumpTimeCounter < JumpTimeMax) {
                velocity.y = jumpTakeOffSpeed * _model.jumpModifier;
                _jumpTimeCounter += Time.deltaTime;
            }
            else if (_stopJump || _jumpTimeCounter >= JumpTimeMax) {
                _stopJump = false;
                if (velocity.y > 0) {
                    velocity.y *= _model.jumpDeceleration;
                }
            }

            if (_move.x > MovementThreshold)
                _spriteRenderer.flipX = false;
            else if (_move.x < -MovementThreshold)
                _spriteRenderer.flipX = true;

            animator.SetBool("grounded", IsGrounded);
            animator.SetFloat("velocityX", Mathf.Abs(velocity.x) / maxSpeed);

            if (_isCrouching) {
                targetVelocity = _move * (maxSpeed * crouchSpeedMultiplier);
            }
            else if (_isWalking) {
                targetVelocity = _move * (maxSpeed * walkSpeedMultiplier);
            }
            else {
                targetVelocity = _move * maxSpeed;
            }

            if (_isClimbing) {
                velocity.y = Input.GetAxis("Vertical") * maxSpeed * 0.5f;
                gravityModifier = 0;
            }
            else {
                gravityModifier = 1;
            }
        }

        public void Idle() {
            animator.SetTrigger("idle");
        }


        public void Walk(bool value = true) {
            _isWalking = value;
        }

        public void Run(bool value = true) {
            _isWalking = value;
        }

        public void Crouch(bool value = true) {
            _isCrouching = value;
        }

        public void Climb(bool value = true) {
            _isClimbing = value;
        }

        public void Jump(bool value = true) {
            _jump = value;
        }
    }
}