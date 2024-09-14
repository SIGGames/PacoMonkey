using Configuration;
using Enums;
using Model;
using Platformer.Core;
using Platformer.Gameplay;
using Platformer.Mechanics;
using UnityEngine;
using static Platformer.Core.Simulation;
using static Configuration.GlobalConfiguration;
using Mechanics.Utils;

namespace Mechanics {
    public class PlayerController : KinematicObject, IMechanics {
        public AudioClip jumpAudio;
        public AudioClip respawnAudio;
        public AudioClip ouchAudio;

        public float maxRunSpeed = 7;
        public float maxWalkSpeed;
        public float maxJumpTakeOffSpeed = 7;
        public float maxJumpChargeTime = 1.0f;
        public float crouchHeightModifier = 0.5f;
        public float lookUpHeightModifier = 1.5f;

        public JumpState jumpState = JumpState.Grounded;
        private bool _stopJump;
        private bool _doubleJumpAvailable;

        private const float MovementThreshold = 0.01f;

        public Collider2D collider2d;
        public AudioSource audioSource;
        public Health health;
        public bool controlEnabled = true;

        private bool _jump;
        private bool _isWalking;
        private bool _isCrouching;
        private bool _isLookingUp;
        private bool _isClimbing;
        private bool _isHolding;
        private float _jumpChargeTime;
        private float _idleTime;
        private bool _extendedIdle1Played;
        private bool _extendedIdle2Played;

        private Vector2 _move;
        private SpriteRenderer _spriteRenderer;
        internal Animator animator;
        private readonly PlatformerModel _model = Simulation.GetModel<PlatformerModel>();

        private PlayerMovementState movementState = PlayerMovementState.Idle;

        public Bounds Bounds => collider2d.bounds;

        void Awake() {
            health = GetComponent<Health>();
            audioSource = GetComponent<AudioSource>();
            collider2d = GetComponent<Collider2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();

            maxWalkSpeed = maxRunSpeed / 3;
        }

        protected override void Update() {
            if (controlEnabled) {
                HandleInput();
            }
            else {
                _move = Vector2.zero;
            }

            UpdateIdleTime();
            UpdateJumpState();
            base.Update();
        }

        private void HandleInput() {
            _move = Vector2.zero;

            HandleMovementInput();
            HandleActionInput();
            HandleClimbInput();
            ResetIdleTimeIfAnyKey();
        }

        private void HandleMovementInput() {
            float h = Keybinds.GetHorizontalAxis();

            _isWalking = Keybinds.GetWalkKey();

            _isCrouching = Keybinds.GetCrouchKey();

            _isLookingUp = Keybinds.GetClimbKey();

            if (!_isCrouching && !_isClimbing) {
                _move.x = h;
            }
        }

        private void HandleActionInput() {
            if (Keybinds.GetJumpKey()) {
                if (jumpState == JumpState.Grounded || _isHolding) {
                    jumpState = JumpState.PrepareToJump;
                }
                else if (_doubleJumpAvailable) {
                    _doubleJumpAvailable = false;
                    jumpState = JumpState.PrepareToJump;
                }
            }
            else if (Keybinds.GetJumpKeyUp()) {
                _stopJump = true;
                Schedule<PlayerStopJump>().player = this;
            }
        }

        private void HandleClimbInput() {
            if (_isClimbing) {
                float v = Keybinds.GetVerticalAxis();
                _move.y = v;
            }
        }

        private void ResetIdleTimeIfAnyKey() {
            if (Keybinds.AnyKeyPressed()) {
                _idleTime = 0;
                _extendedIdle1Played = false;
                _extendedIdle2Played = false;
            }
        }

        private void UpdateIdleTime() {
            if (_move == Vector2.zero && !_jump && !Keybinds.AnyKeyPressed()) {
                _idleTime += Time.deltaTime;

                if (_idleTime >= 6.0f && !_extendedIdle1Played) {
                    animator.SetTrigger("ExtendedIdle1");
                    _extendedIdle1Played = true;
                }
                else if (_idleTime >= 18.0f && !_extendedIdle2Played) {
                    animator.SetTrigger("ExtendedIdle2");
                    _extendedIdle2Played = true;
                }
            }
            else {
                _idleTime = 0;
            }
        }

        void UpdateJumpState() {
            _jump = false;
            switch (jumpState) {
                case JumpState.PrepareToJump:
                    jumpState = JumpState.Jumping;
                    _jump = true;
                    _stopJump = false;
                    _jumpChargeTime = 0;
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
                        _doubleJumpAvailable = true;
                    }

                    break;
                case JumpState.Landed:
                    jumpState = JumpState.Grounded;
                    break;
            }
        }

        protected override void ComputeVelocity() {
            float speed = GetCurrentSpeed();

            AdjustColliderSize();

            HandleJumpMechanics();

            HandleHorizontalMovement();

            SetAnimatorParameters();

            targetVelocity = _move * speed;
        }

        private float GetCurrentSpeed() {
            if (_isWalking) {
                movementState = PlayerMovementState.Walk;
                return maxWalkSpeed;
            }
            else if (_move.x != 0) {
                movementState = PlayerMovementState.Run;
                return maxRunSpeed;
            }
            else {
                movementState = PlayerMovementState.Idle;
                return 0;
            }
        }

        private void AdjustColliderSize() {
            if (_isCrouching) {
                collider2d.transform.localScale = new Vector3(1, crouchHeightModifier, 1);
            }
            else if (_isLookingUp) {
                collider2d.transform.localScale = new Vector3(1, lookUpHeightModifier, 1);
            }
            else {
                collider2d.transform.localScale = Vector3.one;
            }
        }

        private void HandleJumpMechanics() {
            if (_jump && (IsGrounded || _isHolding)) {
                _jumpChargeTime += Time.deltaTime;
                if (_jumpChargeTime >= maxJumpChargeTime || !Keybinds.GetJumpKeyHeld()) {
                    float jumpSpeed = maxJumpTakeOffSpeed * Mathf.Min(_jumpChargeTime / maxJumpChargeTime, 1.0f);
                    velocity.y = jumpSpeed * _model.jumpModifier;
                    _jump = false;
                    _isHolding = false;
                }
            }
            else if (_stopJump) {
                _stopJump = false;
                if (velocity.y > 0) {
                    velocity.y *= _model.jumpDeceleration;
                }
            }
        }

        private void HandleHorizontalMovement() {
            if (_move.x > MovementThreshold)
                _spriteRenderer.flipX = false;
            else if (_move.x < -MovementThreshold)
                _spriteRenderer.flipX = true;
        }

        private void SetAnimatorParameters() {
            animator.SetBool("grounded", IsGrounded);
            animator.SetFloat("velocityX", Mathf.Abs(velocity.x) / maxRunSpeed);
        }

        public void Jump() {
        }

        public void Idle() {
        }

        public void Walk() {
        }

        public void Run() {
        }

        public void Crouch() {
        }

        public void Climb() {
        }
    }
}