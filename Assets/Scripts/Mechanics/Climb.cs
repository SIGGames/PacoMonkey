using UnityEngine;

namespace Mechanics {
    public class Climb : MonoBehaviour {
        private float _vertical;
        private const float Speed = 8f;
        private bool _isLadder;
        private bool _isClimbing;
        private bool _isHolding;

        private Rigidbody2D _rb;
        private PlayerController _playerController;
        // private Animator _animator;

        private void Awake() {
            _rb = GetComponent<Rigidbody2D>();
            _playerController = GetComponent<PlayerController>();
            // _animator = GetComponent<Animator>();
        }

        private void Update() {
            _vertical = Input.GetAxis("Vertical");

            if (_isLadder) {
                ShowClimbIndicator(true);

                if (Utils.Keybinds.GetClimbKey() || _isHolding) {
                    _isClimbing = true;
                    _isHolding = true;
                }
            }
            else {
                ShowClimbIndicator(false);
                _isClimbing = false;
                _isHolding = false;
            }

            // _animator.SetBool("Climbing", _isClimbing);
        }

        private void FixedUpdate() {
            if (_isClimbing) {
                _rb.gravityScale = 0f;
                _rb.velocity = new Vector2(0, _vertical * Speed);

                _playerController.velocity.y = 0f;
            }
            else if (_isHolding) {
                _rb.gravityScale = 0f;
                _rb.velocity = Vector2.zero;
            }
            else {
                _rb.gravityScale = 4f;
            }
        }

        private void OnTriggerEnter2D(Collider2D collision) {
            if (collision.CompareTag("Ladder")) {
                _isLadder = true;
            }
        }

        private void OnTriggerExit2D(Collider2D collision) {
            if (collision.CompareTag("Ladder")) {
                _isLadder = false;
                _isClimbing = false;
                _isHolding = false;
                _rb.gravityScale = 4f;
            }
        }

        private void ShowClimbIndicator(bool show) {
            // TODO: Add code to show or hide the climb indicator like enabling/disabling a UI GameObject or a message on screen
        }
    }
}