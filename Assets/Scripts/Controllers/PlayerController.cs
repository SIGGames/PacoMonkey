using Enums;
using Gameplay;
using Health;
using Managers;
using Mechanics;
using Platformer.Gameplay;
using UnityEngine;
using static Platformer.Core.Simulation;
using static Configuration.GlobalConfiguration;
using static PlayerInput.KeyBinds;

namespace Controllers {
    public class PlayerController : KinematicObject {
        [Header("Player")]
        public bool controlEnabled = true;

        public PlayerMovementState movementState = PlayerMovementState.Idle;
        [SerializeField] public bool isFacingRight = true;

        [Header("Player Run")]
        [Range(0, 10)]
        public float maxRunSpeed = PlayerConfig.MaxRunSpeed;

        [Range(0, 100)]
        public float runAcceleration = PlayerConfig.RunAcceleration;

        [Range(0, 100)]
        public float runDeceleration = PlayerConfig.RunDeceleration;

        [SerializeField] private float flipOffsetChange = 0.06f;

        [Header("Player Walk")]
        [Range(0, 1)]
        public float walkSpeedMultiplier = 0.33f;

        [Header("Player Jump")]
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
        [Range(0f, 100f)]
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

        public Lives lives;

        [Header("Player Audio")]
        public AudioSource audioSource;

        public AudioClip jumpAudio;
        public AudioClip respawnAudio;
        public AudioClip ouchAudio;

        public static PlayerController PCInstance { get; private set; }

        private bool _jump;
        private float _jumpTimeCounter;
        private const float JumpTimeMax = 1.0f;
        private bool _isWalking;

        [HideInInspector]
        public Vector2 move;

        private SpriteRenderer _spriteRenderer;
        internal Animator animator;

        public Bounds Bounds => collider2d.bounds;

        private BoxCollider2D _boxCollider;

        private FlipManager _flipManager;
        private bool _wasMoving;
        private bool _isMovementStateLocked;
        private ColliderManager _colliderManager;
        private bool _isColliderInitialized;

        private float _speedMultiplier = 1f;

        void Awake() {
            InitializeComponents();
            PCInstance = this;
            _boxCollider = GetComponent<BoxCollider2D>();
            _flipManager = new FlipManager(_spriteRenderer, _boxCollider, flipOffsetChange, isFacingRight);
            _colliderManager = new ColliderManager(collider2d);
            _colliderManager.UpdateCollider(false, _boxCollider.offset, _boxCollider.size);
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

            if (!_isColliderInitialized) {
                InitializeCollider();
                _isColliderInitialized = true;
            }
        }

        public void SetSpeedMultiplier(float multiplier = 1f) {
            _speedMultiplier = multiplier;
        }

        protected override void ComputeVelocity() {
            HandleJumpVelocity();
            HandleFlipLogic();
            UpdateAnimatorParameters();
            HandleHorizontalMovement();
        }

        private void InitializeCollider() {
            if (_colliderManager != null) {
                _colliderManager.UpdateCollider(false, _boxCollider.offset, _boxCollider.size);
            }
        }

        private void HandleLives() {
            if (lives.IsAlive) {
                return;
            }

            Schedule<PlayerDeath>();
        }

        private void HandleHorizontalMovement() {
            float targetSpeed = move.x * maxRunSpeed * _speedMultiplier;

            if (_isWalking) {
                targetSpeed *= walkSpeedMultiplier;
            }

            float speedDifference = targetSpeed - velocity.x;
            float accelerationRate =
                (Mathf.Abs(targetSpeed) > MovementThreshold) ? runAcceleration : runDeceleration;
            float movement = Mathf.Clamp(speedDifference, -accelerationRate * Time.deltaTime,
                accelerationRate * Time.deltaTime);

            targetVelocity.x = velocity.x + movement;
        }

        private void InitializeComponents() {
            lives = GetComponent<Lives>();
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
            move.x = GetHorizontalAxis();

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
        }

        private void HandleActionInput() {
            if (GetJumpKeyDown()) {
                _jumpBufferCounter = jumpBufferTime;
            }

            if (jumpState == JumpState.Grounded && _jumpBufferCounter > 0) {
                jumpState = JumpState.PrepareToJump;
            }

            if (GetJumpKeyUp()) {
                _stopJump = true;
                UnlockMovementState();
                Schedule<PlayerStopJump>().player = this;
            }
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
                SetMovementState(PlayerMovementState.Jump, true);
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
            }
            else if (isCurrentlyMovingLeft) {
                isFacingRight = _flipManager.Flip(false);
            }
            else if (_wasMoving) {
                _flipManager.Flip(!isFacingRight);
            }

            _wasMoving = Mathf.Abs(move.x) > MovementThreshold;
        }

        private void UpdateAnimatorParameters() {
            animator.SetBool("grounded", IsGrounded);
            animator.SetFloat("velocityX", Mathf.Abs(velocity.x) / maxRunSpeed);
        }

        public void SetMovementState(PlayerMovementState state, bool lockState = false) {
            if (movementState == state) {
                return;
            }

            if (_isMovementStateLocked) {
                return;
            }

            if (lockState) {
                _isMovementStateLocked = true;
            }

            movementState = state;
            animator.SetTrigger(state.ToString().ToLower());
            _isWalking = state == PlayerMovementState.Walk;
        }

        public void UnlockMovementState() {
            _isMovementStateLocked = false;
        }

        public bool IsFacingRight() {
            return isFacingRight;
        }
    }
}