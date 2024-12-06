using Managers;
using UnityEngine;
using Platformer.Core;

namespace Gameplay {
    public class PlayerCrouch : Simulation.Event<PlayerCrouch> {
        private bool _isCrouching;
        private ColliderManager _colliderManager;
        private Animator _animator;
        private Vector2 _cameraOffset;

        public PlayerCrouch(ColliderManager colliderManager, Animator animator, Vector2 cameraOffset) {
            _colliderManager = colliderManager;
            _animator = animator;
            _cameraOffset = cameraOffset;
        }

        public void Crouch(bool value) {
            if (_isCrouching == value) {
                return;
            }

            _isCrouching = value;

            _colliderManager.UpdateCollider(value);

            if (_animator != null) {
                _animator.SetBool("isCrouching", value);
            }

            CameraManager.Instance.SetOffset(value ? _cameraOffset : Vector2.zero);
        }

        public override void Execute() {
        }
    }
}