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

        [Range(0, 100)]
        public float runAcceleration = PlayerConfig.RunAcceleration;

        [Range(0, 100)]
        public float runDeceleration = PlayerConfig.RunDeceleration;

        [Header("Player Walk Configuration")]
        [Range(0, 1)]
        public float walkSpeedMultiplier = 0.33f;

        [Header("Player Crouch Configuration")]
        [Range(0, 1)]
        public float crouchSpeedMultiplier = 0.5f;

        [Header("Player Jump Configuration")]
        [Tooltip("Initial jump velocity")]
        [Range(0, 3)]
        public float jumpModifier = 1.5f;

        [Range(0, 10)]
        public float jumpTakeOffSpeed = 7;

        [Tooltip("Parameter to slow down an active jump when the user releases the jump input")]
        [Range(0, 2)]
        public float jumpDeceleration = 0.7f;

        [Range(0, 1)]
        public float coyoteTime = 0.2f;

        [Tooltip("Time in seconds to allow the player to jump before landing")]
        [Range(0.01f, 1)]
        [SerializeField] private float jumpBufferTime = 0.2f;

        [Tooltip("Multiplier to control the players falling speed (when the player is at the max height of the jump)")]
        [Range(0, 3)]
        public float fallSpeedMultiplier = 1.5f;

        [Tooltip("0 -> totally horizontal, 100 -> totally vertical")]
        [Range(0F, 100F)]
        public float jumpComponentBalance = 55f;

        private float _balanceFactor;

        private float _jumpBufferCounter;

        private float _coyoteTimeCounter;

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

            _balanceFactor = Mathf.Clamp(jumpComponentBalance / 100f, 0f, 1f);

            UpdateJumpState();
            base.Update();
        }

        protected override void ComputeVelocity() {
            HandleJumpVelocity();
            UpdateSpriteDirection();
            UpdateAnimatorParameters();
            HandleHorizontalMovement();
        }

        private void HandleHorizontalMovement() {
            float targetSpeed = _move.x * maxRunSpeed;

            if (_isCrouching) {
                targetSpeed *= crouchSpeedMultiplier;
            }
            else if (_isWalking) {
                targetSpeed *= walkSpeedMultiplier;
            }

            float speedDifference = targetSpeed - velocity.x;
            float accelerationRate = (Mathf.Abs(targetSpeed) > 0.01f) ? runAcceleration : runDeceleration;

            float movement = Mathf.Clamp(speedDifference, -accelerationRate * Time.deltaTime,
                accelerationRate * Time.deltaTime);

            targetVelocity.x = velocity.x + movement;
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

        protected override void ApplyGravity() {
            if (gravityModifier == 0) {
                return;
            }

            if (velocity.y < 0) {
                velocity += Physics2D.gravity * (gravityModifier * fallSpeedMultiplier * Time.deltaTime);
            }
            else {
                velocity += Physics2D.gravity * (gravityModifier * Time.deltaTime);
            }
        }

        private void HandleMovementInput() {
            _move.x = Input.GetAxis("Horizontal");

            if (_move.x != 0) {
                if (GetWalkKey()) {
                    Walk();
                }
                else {
                    Run();
                }
            }
            else if (IsGrounded) {
                Idle();
            }

            if (GetCrouchKey() && IsGrounded && movementState != PlayerMovementState.Climb) {
                Crouch();
            }
            else {
                Crouch(false);
            }
        }

        private void HandleActionInput() {
            if (Input.GetButtonDown("Jump")) {
                _jumpBufferCounter = jumpBufferTime;
            }

            if (jumpState == JumpState.Grounded && _jumpBufferCounter > 0) {
                jumpState = JumpState.PrepareToJump;
            }

            if (Input.GetButtonUp("Jump")) {
                _stopJump = true;
                Schedule<PlayerStopJump>().player = this;
            }
        }

        private void UpdateJumpState() {
            Jump(false);

            if (IsGrounded) {
                _coyoteTimeCounter = coyoteTime;
            }
            else {
                _coyoteTimeCounter -= Time.deltaTime;
            }

            if (_jumpBufferCounter > 0) {
                _jumpBufferCounter -= Time.deltaTime;
            }

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
            if ((IsGrounded || _coyoteTimeCounter > 0f) && _jumpBufferCounter > 0) {
                jumpState = JumpState.Jumping;
                Jump();
                _stopJump = false;
                _jumpBufferCounter = 0;
            }
        }

        private void HandleJumpVelocity() {
            if (_jump && IsGrounded) {
                _balanceFactor = Mathf.Clamp(jumpComponentBalance / 100f, 0f, 1f);

                velocity.y = jumpTakeOffSpeed * jumpModifier * _balanceFactor;

                velocity.x *= (1 - _balanceFactor);

                Jump();
                _jumpTimeCounter = 0;
            }
            else if (_jump && _jumpTimeCounter < JumpTimeMax) {
                velocity.y = jumpTakeOffSpeed * jumpModifier * _balanceFactor;
                _jumpTimeCounter += Time.deltaTime;
            }
            else if (_stopJump || _jumpTimeCounter >= JumpTimeMax) {
                _stopJump = false;
                if (velocity.y > 0) {
                    velocity.y *= jumpDeceleration;
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
            if (value) {
                _isWalking = false;
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