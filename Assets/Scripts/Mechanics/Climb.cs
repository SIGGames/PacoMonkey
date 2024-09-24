using System;
using Enums;
using UnityEngine;

namespace Mechanics {
    public class Climb : MonoBehaviour {
        private float _vertical;
        private const float Speed = 8f;
        private bool _isLadder;
        private bool _isClimbing;

        private Rigidbody2D _rb;
        private PlayerController _playerController;

        private void Awake() {
            _rb = GetComponent<Rigidbody2D>();
            _playerController = GetComponent<PlayerController>();
        }

        private void Update() {
            _vertical = Input.GetAxis("Vertical");

            if (_isLadder && Utils.Keybinds.GetClimbKey()) {
                _isClimbing = true;
            }
            else if (!_isLadder) {
                _isClimbing = false;
            }

            // TODO: Add animation for climbing, and add reference on Awake method
            // _animator.SetBool("Climbing", _isClimbing);
        }

        private void FixedUpdate() {
            if (_isClimbing) {
                _rb.gravityScale = 0f;
                _rb.velocity = new Vector2(0, _vertical * Speed);

                _playerController.velocity.y = 0f;
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
                _rb.gravityScale = 4f;
                _isClimbing = false;
            }
        }
    }
}