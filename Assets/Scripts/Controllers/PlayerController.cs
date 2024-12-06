using Enums;
using Gameplay;
using Managers;
using Mechanics.Utils;
using Platformer.Gameplay;
using UnityEngine;
using UnityEngine.Serialization;
using static Platformer.Core.Simulation;
using static Configuration.GlobalConfiguration;
using static Mechanics.Utils.Keybinds;

namespace Mechanics.Movement {
    public class PlayerController : KinematicObject {
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

        [SerializeField] private Vector2 cameraOffsetOnCrouch = new(0f, -1.3f);

        [Range(0, 5)]
        public float slideDuration = 1.5f;

        private float _slideTimer;
        [SerializeField] private bool isSliding;

        [Range(0, 10)]
        [SerializeField] private float slideMinSpeedMultiplier = 0.5f;

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

        // Threshold to determine if the player sprite should be flipped
        private const float MovementThreshold = 0.00001f;

        [Header("Player Components")]
        public Collider2D collider2d;

        public AudioSource audioSource;
        public Health.Lives lives;
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
        [SerializeField] private bool isFacingRight = true;

        [HideInInspector]
        public Vector2 move;

        private SpriteRenderer _spriteRenderer;
        internal Animator animator;

        private Vector2 _originalColliderSize;
        private readonly Vector2 _crouchColliderSize = new(0.2f, 0.2f);

        public Bounds Bounds => collider2d.bounds;

        [SerializeField] private float flipOffsetChange = 0.06f;
        private BoxCollider2D _boxCollider;

        private FlipManager _flipManager;
        private bool _wasMoving;

        void Awake() {
            InitializeComponents();
            PCInstance = this;
            _originalColliderSize = collider2d.bounds.size;
            _slideTimer = slideDuration;
            _boxCollider = GetComponent<BoxCollider2D>();
            _flipManager = new FlipManager(_spriteRenderer, _boxCollider, flipOffsetChange, isFacingRight);
        }

        protected override void Update() {
            if (controlEnabled) {
                HandleInput();
            }
            else {
                move.x = 0;
            }

            _balanceFactor = Mathf.Clamp(jumpComponentBalance / 100f, 0f, 1f);
            UpdateJumpState();
            base.Update();

            HandleLives();
        }

        protected override void ComputeVelocity() {
            HandleJumpVelocity();
            HandleFlipLogic();
            UpdateAnimatorParameters();
            HandleHorizontalMovement();
        }

        private void HandleLives() {
            if (lives.IsAlive) {
                return;
            }

            Schedule<PlayerDeath>();
        }

        private void HandleHorizontalMovement() {
            if (isSliding) {
                Slide();
            }
            else {
                float targetSpeed = move.x * maxRunSpeed;

                if (_isCrouching) {
                    if (_isWalking) {
                        targetSpeed *= crouchSpeedMultiplier * walkSpeedMultiplier;
                    }
                    else {
                        targetSpeed *= crouchSpeedMultiplier;
                    }
                }
                else if (_isWalking) {
                    targetSpeed *= walkSpeedMultiplier;
                }

                float speedDifference = targetSpeed - velocity.x;
                float accelerationRate =
                    (Mathf.Abs(targetSpeed) > MovementThreshold) ? runAcceleration : runDeceleration;
                float movement = Mathf.Clamp(speedDifference, -accelerationRate * Time.deltaTime,
                    accelerationRate * Time.deltaTime);

                targetVelocity.x = velocity.x + movement;
            }
        }

        private void Slide() {
            _slideTimer -= Time.deltaTime;

            float targetSlideSpeed = maxRunSpeed * slideMinSpeedMultiplier;
            float slideProgress = 1 - (_slideTimer / slideDuration);
            float slideSpeed = Mathf.Lerp(maxRunSpeed, targetSlideSpeed, slideProgress);
            targetVelocity.x = isFacingRight ? slideSpeed : -slideSpeed;

            if (_slideTimer <= 0 || !GetCrouchKey()) {
                isSliding = false;
                _slideTimer = slideDuration;

                if (GetCrouchKey()) {
                    if (Mathf.Abs(move.x) < MovementThreshold) {
                        SetMovementState(PlayerMovementState.Idle);
                    }
                    else {
                        SetMovementState(PlayerMovementState.Run);
                    }
                }
                else if (GetRunKey()) {
                    SetMovementState(PlayerMovementState.Run);
                }
            }
        }

        private void InitializeComponents() {
            lives = GetComponent<Health.Lives>();
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

            float gravityScale = gravityModifier * Time.deltaTime;
            if (velocity.y < 0) {
                gravityScale *= fallSpeedMultiplier;
            }

            velocity += Physics2D.gravity * gravityScale;
        }

        private void HandleMovementInput() {
            move.x = Input.GetAxis("Horizontal");

            if (move.x != 0) {
                if (GetWalkKey()) {
                    _isWalking = true;
                    SetMovementState(PlayerMovementState.Walk);
                }
                else {
                    _isWalking = false;
                    SetMovementState(PlayerMovementState.Run);
                }
            }
            else if (IsGrounded) {
                SetMovementState(PlayerMovementState.Idle);
            }

            if (GetCrouchKey() && IsGrounded) {
                animator.SetBool("isCrouching", true);
                Crouch(true);
            }
            else {
                animator.SetBool("isCrouching", false);
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

            TestCameraManager();
        }

        private void TestCameraManager() {
            // TODO: Remove this test method, now is used for testing camera manager
            if (Input.GetKey(KeyCode.Alpha1)) {
                CameraManager.Instance.SetZoom(3f);
            }

            if (Input.GetKey(KeyCode.Alpha2)) {
                CameraManager.Instance.SetZoom(7f);
            }

            if (Input.GetKey(KeyCode.Alpha3)) {
                CameraManager.Instance.ResetCamera();
            }

            if (Input.GetKey(KeyCode.Alpha4)) {
                CameraManager.Instance.SetOffset(new Vector2(2f, 0f));
            }

            if (Input.GetKey(KeyCode.Alpha5)) {
                CameraManager.Instance.SetOffset(new Vector2(-2f, 0f));
            }

            if (Input.GetKey(KeyCode.Alpha6)) {
                CameraManager.Instance.SetOffset(new Vector2(0f, 2f));
            }

            if (Input.GetKey(KeyCode.Alpha7)) {
                CameraManager.Instance.SetOffset(new Vector2(0f, -2f));
            }

            if (Input.GetKey(KeyCode.Alpha8)) {
                CameraManager.Instance.ShakeCamera(1f, 1f, 0.5f);
            }

            if (Input.GetKey(KeyCode.Alpha9)) {
                CameraManager.Instance.ShakeCamera(3f, 3f, 1f);
            }

            if (Input.GetKey(KeyCode.Alpha0)) {
                CameraManager.Instance.ResetCamera();
            }
        }

        private void UpdateJumpState() {
            _jump = false;

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
                _jump = true;
                animator.SetTrigger("jump");
                movementState = PlayerMovementState.Jump;
                _stopJump = false;
                _jumpBufferCounter = 0;
            }
        }

        private void HandleJumpVelocity() {
            if (_jump && IsGrounded) {
                velocity.y = jumpTakeOffSpeed * jumpModifier * _balanceFactor;
                velocity.x *= (1 - _balanceFactor);
                _jumpTimeCounter = 0;
            }
            else if (_jump && _jumpTimeCounter < JumpTimeMax) {
                velocity.y = jumpTakeOffSpeed * jumpModifier * _balanceFactor;
                _jumpTimeCounter += Time.deltaTime;
            }

            if (_stopJump || _jumpTimeCounter >= JumpTimeMax) {
                _stopJump = false;
                if (velocity.y > 0) {
                    velocity.y *= jumpDeceleration;
                }
            }
        }

        private void HandleFlipLogic() {
            bool isCurrentlyMovingRight = move.x > 0;
            bool isCurrentlyMovingLeft = move.x < 0;

            if (isCurrentlyMovingRight) {
                isFacingRight = _flipManager.Flip(true);
            } else if (isCurrentlyMovingLeft) {
                isFacingRight = _flipManager.Flip(false);
            } else if (_wasMoving) {
                _flipManager.Flip(!isFacingRight);
            }

            _wasMoving = Mathf.Abs(move.x) > MovementThreshold;
        }

        private void UpdateAnimatorParameters() {
            animator.SetBool("grounded", IsGrounded);
            animator.SetFloat("velocityX", Mathf.Abs(velocity.x) / maxRunSpeed);
        }

        public void SetMovementState(PlayerMovementState state) {
            movementState = state;
            switch (state) {
                case PlayerMovementState.Idle:
                    animator.SetTrigger("idle");
                    _isWalking = false;
                    _isCrouching = false;
                    break;
                case PlayerMovementState.Walk:
                    animator.SetTrigger("walk");
                    _isWalking = true;
                    _isCrouching = false;
                    break;
                case PlayerMovementState.Run:
                    animator.SetTrigger("run");
                    _isWalking = false;
                    _isCrouching = false;
                    break;
                case PlayerMovementState.Crouch:
                    animator.SetTrigger("crouch");
                    _isCrouching = true;
                    break;
                case PlayerMovementState.Jump:
                    animator.SetTrigger("jump");
                    _isCrouching = false;
                    break;
                case PlayerMovementState.Climb:
                    animator.SetTrigger("climb");
                    _isCrouching = false;
                    break;
                case PlayerMovementState.Up:
                    break;
            }
        }

        public void Crouch(bool value) {
            _isCrouching = value;
            Bounds collider2dBounds = collider2d.bounds;
            collider2dBounds.size = value ? _crouchColliderSize : _originalColliderSize;

            if (value) {
                CameraManager.Instance.SetOffset(cameraOffsetOnCrouch);
                if (movementState == PlayerMovementState.Run && !isSliding) {
                    isSliding = true;
                    _slideTimer = slideDuration;
                    SetMovementState(PlayerMovementState.Crouch);
                }
                else {
                    SetMovementState(PlayerMovementState.Crouch);
                }
            }
            else {
                CameraManager.Instance.SetOffset(Vector2.zero);
                isSliding = false;
                _slideTimer = slideDuration;
            }
        }


        public bool IsFacingRight() {
            return isFacingRight;
        }
    }
}