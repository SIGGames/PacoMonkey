using Enums;
using Gameplay;
using Health;
using Managers;
using UnityEngine;
using static Platformer.Core.Simulation;
using static Configuration.GlobalConfiguration;
using static Utils.AnimatorUtils;
using static PlayerInput.KeyBinds;

namespace Controllers {
    public class PlayerController : KinematicObject {
        [Header("Player")]
        public bool controlEnabled = true;

        [SerializeField] private bool isPositionFreezed;

        public PlayerMovementState movementState = PlayerMovementState.Idle;
        [SerializeField] public bool isFacingRight = true;

        [Header("Player Run")]
        public bool canRun = true;

        [Range(0, 10)]
        public float maxRunSpeed = PlayerConfig.MaxRunSpeed;

        private float _maxRunSpeedOriginal = PlayerConfig.MaxRunSpeed;

        [Range(0, 100)]
        public float runAcceleration = PlayerConfig.RunAcceleration;

        [Range(0, 100)]
        public float runDeceleration = PlayerConfig.RunDeceleration;

        [SerializeField] private float flipOffsetChange = 0.06f;

        [Header("Player Walk")]
        public bool canWalk = true;

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

        [Tooltip("Time to wait before decelerating the jump")]
        [Range(0f, 1f)]
        public float jumpDecelerationDelay = 0.2f;

        private float _balanceFactor;
        private float _jumpBufferCounter;
        private float _coyoteTimeCounter;
        public JumpState jumpState = JumpState.Grounded;
        private bool _stopJump;

        // Threshold to determine if the player sprite should be flipped
        private const float MovementThreshold = 0.00001f;

        [Header("Player Components")]
        public Collider2D collider2d;

        [SerializeField] private LayerMask groundLayer;

        public Lives lives;

        [Header("Player Audio")]
        public AudioSource audioSource;

        public AudioClip jumpAudio;
        public AudioClip respawnAudio;
        public AudioClip ouchAudio;

        private bool _jump;
        private float _jumpTimeCounter;
        private const float JumpTimeMax = 1.0f;

        [HideInInspector]
        public Vector2 move;

        private SpriteRenderer _spriteRenderer;
        internal Animator animator;

        public Bounds Bounds => collider2d.bounds;

        public BoxCollider2D boxCollider;

        public FlipManager flipManager;
        private bool _wasMoving;
        private int _currentPriority;
        private ColliderManager _colliderManager;
        private bool _isColliderInitialized;
        public Rigidbody2D rb;

        private const float SpeedMultiplier = 1f;
        private float _jumpDecelerationTimer;
        private bool _isDying;
        [HideInInspector] public Vector3 respawnPosition;

        void Awake() {
            InitializeComponents();
            boxCollider = GetComponent<BoxCollider2D>();
            flipManager = new FlipManager(_spriteRenderer, boxCollider, animator, flipOffsetChange, isFacingRight);
            _colliderManager = new ColliderManager(collider2d);
            _colliderManager.UpdateCollider(false, boxCollider.size);
            respawnPosition = transform.position;
        }

        protected override void Update() {
            HandleDebugInput();

            if (controlEnabled) {
                HandleInput();
            } else {
                move.x = 0;
            }

            _balanceFactor = Mathf.Clamp(jumpComponentBalance / 100f, 0f, 1f);
            UpdateJumpState();
            base.Update();

            HandleLives();
            HandlePlayerInsideWall();

            if (!_isColliderInitialized) {
                InitializeCollider();
                _isColliderInitialized = true;
            }
        }

        public void SetSpeed(float newSpeed) {
            _maxRunSpeedOriginal = maxRunSpeed;
            maxRunSpeed = Mathf.Clamp(newSpeed, 0, 10);
        }

        public void ResetSpeed() {
            maxRunSpeed = _maxRunSpeedOriginal;
        }

        protected override void ComputeVelocity() {
            HandleJumpVelocity();
            HandleFlipLogic();
            UpdateAnimatorParameters();
            HandleHorizontalMovement();
        }

        private void InitializeCollider() {
            if (_colliderManager != null) {
                _colliderManager.UpdateCollider(false, boxCollider.size);
            }
        }

        private void HandleLives() {
            if (lives.IsAlive || _isDying || !enabled || !IsGrounded) {
                return;
            }

            CharacterManager characterManager = CharacterManager.Instance;
            if (characterManager != null) {
                animator.SetBool(Dead, true);
                animator.SetTrigger(Death);
                _isDying = true;
                characterManager.RespawnCharacter();
            }
        }

        public void ResetState() {
            animator.SetBool(Dead, false);
            Teleport(respawnPosition);
            lives.ResetLives();
            _isDying = false;
        }

        private void HandleHorizontalMovement() {
            float targetSpeed = move.x * maxRunSpeed * SpeedMultiplier;

            if (canWalk && !canRun) {
                targetSpeed *= walkSpeedMultiplier;
            } else if (canRun && canWalk && GetWalkKey()) {
                targetSpeed *= walkSpeedMultiplier;
                SetMovementState(PlayerMovementState.Walk);
            }

            float speedDifference = targetSpeed - velocity.x;
            float accelerationRate = (Mathf.Abs(targetSpeed) > MovementThreshold) ? runAcceleration : runDeceleration;
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
            rb = GetComponent<Rigidbody2D>();
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

        private void HandleInput() {
            HandleMovementInput();
            HandleJumpInput();
        }

        private void HandleMovementInput() {
            move.x = GetHorizontalAxis();

            if (move.x != 0) {
                if (canRun) {
                    SetMovementState(PlayerMovementState.Run);
                } else if (canWalk) {
                    SetMovementState(PlayerMovementState.Walk);
                }
            } else if (IsGrounded) {
                SetMovementState(PlayerMovementState.Idle);
            }
        }

        private void HandleJumpInput() {
            if (GetJumpKeyDown() && PlayerMovementStateMethods.IsPlayerAbleToJump(movementState)) {
                _jumpBufferCounter = jumpBufferTime;
            }

            if (jumpState == JumpState.Grounded && _jumpBufferCounter > 0) {
                jumpState = JumpState.PrepareToJump;
            }

            if (GetJumpKeyUp()) {
                _stopJump = true;
                UnlockMovementState();
            }
        }

        private void UpdateJumpState() {
            _jump = false;

            if (IsGrounded) {
                _coyoteTimeCounter = coyoteTime;
            } else {
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
                    animator.SetBool(IsJumping, false);
                    Schedule<PlayerLanded>().player = this;
                    jumpState = JumpState.Grounded;
                    break;
            }
        }

        public void StartJump() {
            if ((IsGrounded || _coyoteTimeCounter > 0f) && _jumpBufferCounter > 0) {
                jumpState = JumpState.Jumping;
                _jump = true;
                _stopJump = false;
                _jumpBufferCounter = 0;
                _jumpDecelerationTimer = jumpDecelerationDelay;
                animator.SetBool(IsJumping, true);
                SetMovementState(PlayerMovementState.Jump, 2);
            }
        }

        private void HandleJumpVelocity() {
            if (_jump && IsGrounded) {
                velocity.y = jumpTakeOffSpeed * jumpModifier * _balanceFactor;
                velocity.x *= (1 - _balanceFactor);
                _jumpTimeCounter = 0;
            } else if (_jump && _jumpTimeCounter < JumpTimeMax && GetJumpKeyHeld()) {
                velocity.y = jumpTakeOffSpeed * jumpModifier * _balanceFactor;
                _jumpTimeCounter += Time.deltaTime;
            }

            if (jumpState is JumpState.Jumping or JumpState.InFlight) {
                if (_jumpDecelerationTimer > 0) {
                    _jumpDecelerationTimer -= Time.deltaTime;
                }
            }

            if (_stopJump || _jumpTimeCounter >= JumpTimeMax || !GetJumpKeyHeld()) {
                _stopJump = false;
                if (velocity.y > 0 && _jumpDecelerationTimer <= 0) {
                    velocity.y *= jumpDeceleration;
                }
            }
        }

        public void DisableFlipAnimation() {
            flipManager.AnimateFlip(false);
        }

        private void HandleFlipLogic() {
            bool isCurrentlyMovingRight = move.x > 0;
            bool isCurrentlyMovingLeft = move.x < 0;

            if (isCurrentlyMovingRight && !isFacingRight) {
                isFacingRight = flipManager.Flip(true);
            } else if (isCurrentlyMovingLeft && isFacingRight) {
                isFacingRight = flipManager.Flip(false);
            }
        }

        private void UpdateAnimatorParameters() {
            animator.SetBool(Grounded, IsGrounded);
            animator.SetFloat(VelocityX, Mathf.Abs(velocity.x) / maxRunSpeed);
            animator.SetFloat(VelocityY, Mathf.Abs(velocity.y));
        }

        public void SetMovementState(PlayerMovementState state, int priority = 1) {
            if (priority < _currentPriority || movementState == state) {
                return;
            }

            if (movementState != state || priority > _currentPriority) {
                _currentPriority = priority;
                movementState = state;
            }
        }

        public void UnlockMovementState() {
            _currentPriority = 0;
        }

        public void FreezePosition(bool value = true) {
            isPositionFreezed = value;

            if (isPositionFreezed) {
                controlEnabled = false;
                SetVelocity(Vector2.zero);
                SetBodyType(RigidbodyType2D.Static);
            } else {
                controlEnabled = true;
                SetBodyType(RigidbodyType2D.Kinematic);
            }
        }

        public void FreezeHorizontalPosition(bool value = true) {
            if (value) {
                controlEnabled = false;
                rb.velocity = new Vector2(0, rb.velocity.y);
                velocity = new Vector2(0, velocity.y);
                rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
            } else {
                controlEnabled = true;
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            }
        }

        public void AddPosition(float x = 0f, float y = 0f, float z = 0f) {
            Vector3 position = transform.position;
            SetPosition(position.x + x, position.y + y, position.z + z);
        }

        public void SetPosition(float? x = null, float? y = null, float? z = null) {
            Vector3 position = transform.position;
            SetPosition(new Vector3(x ?? position.x, y ?? position.y, z ?? position.z));
        }

        public void SetPosition(Vector3 position) {
            transform.position = position;
        }

        public void SetBodyType(RigidbodyType2D bodyType) {
            rb.bodyType = bodyType;
        }

        public void SetVelocity(Vector2 newVelocity) {
            velocity = newVelocity;
            rb.velocity = newVelocity;
            animator.SetFloat(VelocityX, Mathf.Abs(velocity.x) / maxRunSpeed);
            animator.SetFloat(VelocityY, Mathf.Abs(velocity.y));
        }

        private void HandlePlayerInsideWall() {
            if (IsGrounded || isPositionFreezed) {
                return;
            }

            Collider2D wallCollider = Physics2D.OverlapPoint(transform.position, groundLayer);
            if (wallCollider != null) {
                Vector2 wallCenter = wallCollider.bounds.center;
                Vector3 position = transform.position;
                const float distanceTp = 1.5f;
                SetBodyType(RigidbodyType2D.Kinematic);

                if (transform.position.x < wallCenter.x) {
                    Teleport(new Vector3(wallCenter.x - distanceTp, position.y, position.z));
                } else {
                    Teleport(new Vector3(wallCenter.x + distanceTp, position.y, position.z));
                }
            }
        }

        private void OnFinishHurtAnimation() {
            const float hurtOffset = 0.4f;
            Vector3 pos = transform.position;
            pos.x += isFacingRight ? -hurtOffset : hurtOffset;
            transform.position = pos;
        }

        private void HandleDebugInput() {
            // TODO: This keybinds are for debugging purposes
            if (Input.GetKeyDown(KeyCode.F4)) {
                SetBodyType(RigidbodyType2D.Kinematic);
                Teleport(respawnPosition);
            }

            if (CharacterManager.Instance.currentPlayerController == this) {
                if (Input.GetKeyDown(KeyCode.F6)) {
                    lives.IncrementLives();
                }

                if (Input.GetKeyDown(KeyCode.F7)) {
                    lives.DecrementLives();
                }

                if (Input.GetKeyDown(KeyCode.F8)) {
                    lives.Die();
                }
            }
        }
    }
}