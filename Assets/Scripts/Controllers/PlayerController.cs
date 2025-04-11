using System.Diagnostics.CodeAnalysis;
using Enums;
using Gameplay;
using Health;
using Managers;
using Mechanics.Fight;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;
using static Platformer.Core.Simulation;
using static Utils.AnimatorUtils;
using static PlayerInput.KeyBinds;
using static Utils.LayerUtils;
using static Configuration.GameConfig;
using static Utils.PlayerPrefsKeys;

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
        public float maxRunSpeed = 4.5f;

        private float _maxRunSpeedOriginal = 4.5f;

        [Range(0, 100)]
        public float runAcceleration = 70;

        [Range(0, 100)]
        public float runDeceleration = 60;

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

        [Header("Player Death")]
        [SerializeField] private bool rumbleOnHit = true;

        [SerializeField, ShowIf("rumbleOnHit")]
        private float rumbleDuration = 0.2f;

        [SerializeField, ShowIf("rumbleOnHit")]
        private float rumbleIntensity = 0.1f;

        [SerializeField] private Vector2 boxColliderOnDeathSize;
        private Vector2 _originalBoxColliderSize;

        [SerializeField] private Vector2 boxColliderOnDeathOffset;
        private Vector2 _originalBoxColliderOffset;

        [SerializeField, Range(0, 0.5f)] private float verticalPositionOffsetOnDeath;

        [Header("Player Components")]
        public Collider2D collider2d;

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

        private const float SpeedMultiplier = 1f;
        private float _jumpDecelerationTimer;
        private bool _isDying;
        [HideInInspector] public Vector3 respawnPosition;

        private void Awake() {
            InitializeComponents();
            boxCollider = GetComponent<BoxCollider2D>();
            flipManager = new FlipManager(_spriteRenderer, boxCollider, animator, flipOffsetChange, isFacingRight);
            _colliderManager = new ColliderManager(collider2d);
            _colliderManager.UpdateCollider(false, boxCollider.size);
            respawnPosition.x = PlayerPrefs.GetFloat(RespawnPositionX, transform.position.x);
            respawnPosition.y = PlayerPrefs.GetFloat(RespawnPositionY, transform.position.y);
        }

        protected override void Start() {
            _originalBoxColliderSize = boxCollider.size;
            _originalBoxColliderOffset = boxCollider.offset;
            if (boxColliderOnDeathSize == Vector2.zero) {
                boxColliderOnDeathSize = boxCollider.size;
            }

            if (boxColliderOnDeathOffset == Vector2.zero) {
                boxColliderOnDeathOffset = boxCollider.offset;
            }

            if (respawnPosition != Vector3.zero) {
                transform.position = respawnPosition;
            }
        }

        protected override void Update() {
            HandleDebugInput();
            HandleGravityOnDeath();

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
            if (lives.IsAlive || _isDying || !enabled ||
                (!IsGrounded && !PlayerMovementStateMethods.PlayerCanDieNotGrounded(movementState))) {
                return;
            }

            animator.SetBool(Dead, true);
            animator.SetTrigger(Death);
            _isDying = true;
            CharacterManager.Instance.RespawnCharacter();
        }

        public void SetColliderOnDeath() {
            // Update the collider size when the player dies
            boxCollider.size = lives.IsAlive ? _originalBoxColliderSize : boxColliderOnDeathSize;
            // Apply the offset to the collider when the player dies depending on the direction the player is facing
            Vector2 boxColliderDeathOffset = new Vector2(
                isFacingRight ? boxColliderOnDeathOffset.x : -boxColliderOnDeathOffset.x,
                boxColliderOnDeathOffset.y);

            // This is a bit of a hack but like I can adjust the vertical position precisely
            transform.position = new Vector3(transform.position.x, transform.position.y + verticalPositionOffsetOnDeath, 0);

            boxCollider.offset = lives.IsAlive ? _originalBoxColliderOffset : boxColliderDeathOffset;
            // Set body type to kinematic to enable gravity and collisions while the player has died
            SetBodyType(RigidbodyType2D.Kinematic);
        }

        private void HandleGravityOnDeath() {
            if (lives.IsAlive) {
                return;
            }

            if (IsColliderOnWall()) {
                SetBodyType(RigidbodyType2D.Static);
            }
        }

        private bool IsColliderOnWall() {
            return Physics2D.OverlapBox(Bounds.center, Bounds.size, 0f, Ground.value) != null;
        }

        public void ResetState() {
            animator.SetBool(IsClimbing, false);
            animator.SetBool(IsHolding, false);
            animator.SetBool(Dead, false);
            Respawn();
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
            float accelerationRate = (Mathf.Abs(targetSpeed) > MinMoveDistance) ? runAcceleration : runDeceleration;
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
            body = GetComponent<Rigidbody2D>();
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
                        SetBodyType(RigidbodyType2D.Kinematic);
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

        public void FreezePosition(bool value = true, bool applyGravity = false) {
            isPositionFreezed = value;

            if (isPositionFreezed) {
                controlEnabled = false;
                SetVelocity(Vector2.zero);
                if (!applyGravity) {
                    SetBodyType(RigidbodyType2D.Static);
                }
            } else {
                controlEnabled = true;
                SetBodyType(RigidbodyType2D.Kinematic);
            }
        }

        public void FreezeHorizontalPosition(bool value = true) {
            if (value) {
                controlEnabled = false;
                body.velocity = new Vector2(0, body.velocity.y);
                velocity = new Vector2(0, velocity.y);
                body.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
            } else {
                controlEnabled = true;
                body.constraints = RigidbodyConstraints2D.FreezeRotation;
            }
        }

        public void AddPosition(float x = 0f, float y = 0f, float z = 0f) {
            Vector3 position = transform.position;
            SetPosition(position.x + x, position.y + y, position.z + z);
        }

        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public void SetPosition(float? x = null, float? y = null, float? z = null) {
            Vector3 position = transform.position;
            SetPosition(new Vector3(x ?? position.x, y ?? position.y, z ?? position.z));
        }

        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public void SetPosition(Vector3 position) {
            // Do not try to use the Teleport method from the KinematicObject class because it will reset the velocity
            // times witch I have fallen into this: 2
            transform.position = position;
        }

        public void SetBodyType(RigidbodyType2D bodyType) {
            body.bodyType = bodyType;
        }

        public void SetVelocity(Vector2 newVelocity) {
            if (body.bodyType == RigidbodyType2D.Static) {
                return;
            }

            velocity = newVelocity;
            body.velocity = newVelocity;
            animator.SetFloat(VelocityX, Mathf.Abs(velocity.x) / maxRunSpeed);
            animator.SetFloat(VelocityY, Mathf.Abs(velocity.y));
        }

        public void KillPlayer() {
            lives.Die();
        }

        public void TakeDamage(float damage = 1) {
            lives.DecrementLives(damage);

            if (lives.IsAlive) {
                PlayerController player = CharacterManager.Instance.currentPlayerController;
                player.animator.SetTrigger(Hurt);

                // Reset the player's fight state
                PlayerFight playerFight = player.GetComponent<PlayerFight>();
                if (playerFight != null && playerFight.fightState != FightState.Idle) {
                    playerFight.fightState = FightState.Idle;
                    player.FreezeHorizontalPosition(false);
                    playerFight.canMeleeAttack = true;
                    playerFight.canRangedAttack = true;
                    playerFight.canParry = true;
                }
            }

            if (rumbleOnHit && Gamepad.current != null) {
                Gamepad.current.SetMotorSpeeds(rumbleIntensity, rumbleIntensity);
                Invoke(nameof(StopRumble), rumbleDuration);
            }
        }

        private void StopRumble() {
            if (Gamepad.current != null) {
                Gamepad.current.SetMotorSpeeds(0, 0);
            }
        }

        private void HandlePlayerInsideWall() {
            if (IsGrounded || isPositionFreezed) {
                return;
            }

            Collider2D wallCollider = Physics2D.OverlapPoint(transform.position, Ground.value);
            if (wallCollider != null) {
                Bounds wallBounds = wallCollider.bounds;
                Vector3 position = transform.position;
                const float margin = 0.2f;

                if (transform.position.x < wallBounds.center.x) {
                    Teleport(new Vector3(wallBounds.min.x - margin, position.y, position.z));
                } else {
                    Teleport(new Vector3(wallBounds.max.x + margin, position.y, position.z));
                }
            }

            // If the player is out of map bounds tp
            if (transform.position.y is <= MinMapY or >= MaxMapY) {
                Respawn();
            }
        }

        private void OnFinishHurtAnimation() {
            const float hurtOffset = 0.4f;
            Vector3 pos = transform.position;
            pos.x += isFacingRight ? -hurtOffset : hurtOffset;
            transform.position = pos;
        }

        public void Respawn() {
            Teleport(respawnPosition);
        }

        public void SetDifficultyMultiplier(float multiplier) {
            if (lives != null) {
                // Since this is the player, the lives are the inverse of the difficulty multiplier
                lives.MultiplyLives(1f / multiplier);
            }

            PlayerFight playerFight = GetComponent<PlayerFight>();
            if (playerFight != null) {
                playerFight.meleeDamage *= (int)multiplier;
                playerFight.rangedDamage *= (int)multiplier;
            }
        }

        private void HandleDebugInput() {
            // TODO: This key binds are for debugging purposes
            if (Input.GetKeyDown(KeyCode.F4)) {
                Respawn();
            }

            if (CharacterManager.Instance.currentPlayerController == this) {
                if (Input.GetKeyDown(KeyCode.F6)) {
                    lives.IncrementLives();
                }

                if (Input.GetKeyDown(KeyCode.F7)) {
                    TakeDamage();
                }

                if (Input.GetKeyDown(KeyCode.F8)) {
                    KillPlayer();
                }
            }
        }
    }
}