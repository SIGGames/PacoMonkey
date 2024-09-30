using Configuration;
using Enums;
using Model;
using Platformer.Gameplay;
using Platformer.Mechanics;
using UnityEngine;
using static Platformer.Core.Simulation;
using static Configuration.GlobalConfiguration;
using static Mechanics.Utils.Keybinds;

namespace Mechanics.Movement {
    public class PlayerController : KinematicObject, IMechanics {
        public AudioClip jumpAudio;
        public AudioClip respawnAudio;
        public AudioClip ouchAudio;


        [Header("Player Run Configuration")]
        [Range(0, 10)]
        public float maxRunSpeed = PlayerConfig.MaxRunSpeed;
        public float runAcceleration = PlayerConfig.RunAcceleration;
        public float runDeceleration = PlayerConfig.RunDeceleration;

        [Header("Player Walk Configuration")]
        [Range(0, 1)]
        public float walkSpeedMultiplier = 0.33f;

        [Header("Player Crouch Configuration")]
        [Range(0, 1)]
        public float crouchSpeedMultiplier = 0.5f;

        [Header("Player Jump Configuration")]
        [Range(0, 10)]
        public float jumpTakeOffSpeed = 7;
        public JumpState jumpState = JumpState.Grounded;
        private bool _stopJump;
        private GlobalConfiguration _config;

        private const float MovementThreshold = 0.01f;

        [Header("Player Components")]
        public Collider2D collider2d;
        public AudioSource audioSource;
        public Health.Health health;
        public bool controlEnabled = true;
        public PlayerMovementState movementState = PlayerMovementState.Idle;

        public static PlayerController PCInstance { get; private set; }

        public PlayerMovementState MovementState {
            get => movementState;
            set => movementState = value;
        }

        private bool _jump;
        private float _jumpTimeCounter;
        private const float JumpTimeMax = 1.0f;
        private bool _isCrouching;
        private bool _isWalking;
        private Vector2 _move;
        private SpriteRenderer _spriteRenderer;
        internal Animator animator;
        private readonly PlatformerModel _model = GetModel<PlatformerModel>();

        public Bounds Bounds => collider2d.bounds;

        void Awake() {
            _config = Instance;
            InitializeComponents();
        }

        protected override void Update() {
            if (controlEnabled) {
                HandleInput();
            }
            else {
                _move.x = 0;
            }

            UpdateJumpState();
            base.Update();
        }

        protected override void ComputeVelocity() {
            HandleJumpVelocity();
            UpdateSpriteDirection();
            UpdateAnimatorParameters();
            ComputeTargetVelocity();
        }

        private void InitializeComponents() {
            health = GetComponent<Health.Health>();
            audioSource = GetComponent<AudioSource>();
            collider2d = GetComponent<Collider2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
        }

        private void HandleInput() {
            HandleMovementInput();
            HandleActionInput();
        }

        private void HandleMovementInput() {
            _move.x = Input.GetAxis("Horizontal");
            if (GetWalkKey()) {
                Walk();
            }
            else {
                Walk(false);
                movementState = PlayerMovementState.Idle;
            }

            if (GetCrouchKey()) {
                Crouch();
            }
            else {
                Crouch(false);
            }
        }

        private void HandleActionInput() {
            if (jumpState == JumpState.Grounded && Input.GetButtonDown("Jump")) {
                jumpState = JumpState.PrepareToJump;
            }
            else if (Input.GetButtonUp("Jump")) {
                _stopJump = true;
                Schedule<PlayerStopJump>().player = this;
            }
        }

        private void UpdateJumpState() {
            Jump(false);
            switch (jumpState) {
                case JumpState.PrepareToJump:
                    StartJump();
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

        private void StartJump() {
            jumpState = JumpState.Jumping;
            Jump();
            _stopJump = false;
        }

        private void HandleJumpVelocity() {
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
        }

        private void UpdateSpriteDirection() {
            if (_move.x > MovementThreshold)
                _spriteRenderer.flipX = false;
            else if (_move.x < -MovementThreshold)
                _spriteRenderer.flipX = true;
        }

        private void UpdateAnimatorParameters() {
            animator.SetBool("grounded", IsGrounded);
            animator.SetFloat("velocityX", Mathf.Abs(velocity.x) / maxRunSpeed);
        }

        private void ComputeTargetVelocity() {
            if (_isCrouching) {
                targetVelocity = _move * (maxRunSpeed * crouchSpeedMultiplier);
            }
            else if (_isWalking) {
                targetVelocity = _move * (maxRunSpeed * walkSpeedMultiplier);
            }
            else {
                targetVelocity = _move * maxRunSpeed;
            }
        }

        public void Idle() {
            animator.SetTrigger("idle");
            movementState = PlayerMovementState.Idle;
        }

        public void Walk(bool value = true) {
            _isWalking = value;
            if (value) {
                animator.SetTrigger("walk");
                movementState = PlayerMovementState.Walk;
            }
        }

        public void Run(bool value = true) {
            _isWalking = value;
            if (value) {
                animator.SetTrigger("run");
                movementState = PlayerMovementState.Run;
            }
        }

        public void Crouch(bool value = true) {
            _isCrouching = value;
            if (value) {
                movementState = PlayerMovementState.Crouch;
            }
        }

        public void Jump(bool value = true) {
            _jump = value;
            if (value) {
                animator.SetTrigger("jump");
                movementState = PlayerMovementState.Jump;
            }
        }
    }
}