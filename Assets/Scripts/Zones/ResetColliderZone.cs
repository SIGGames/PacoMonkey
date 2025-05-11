using System;
using Enums;
using Managers;
using UnityEngine;
using static Utils.TagUtils;

namespace Zones {
    public class ResetColliderZone : MonoBehaviour {
        private ColliderManager _colliderManager;
        private Character _character;

        private void Start() {
            _character = CharacterManager.Instance.currentCharacter;
            InitializeColliderManager();
        }

        private void Update() {
            if (CharacterManager.Instance == null) {
                return;
            }

            if (CharacterManager.Instance.currentCharacter != _character) {
                _character = CharacterManager.Instance.currentCharacter;
                InitializeColliderManager();
            }
        }

        private void InitializeColliderManager() {
            _colliderManager = new ColliderManager(CharacterManager.Instance.currentPlayerController.collider2d);
        }

        private void OnTriggerEnter2D(Collider2D col) {
            if (!col.CompareTag(Player)) {
                return;
            }

            if (CharacterManager.Instance == null) {
                return;
            }


            _colliderManager.UpdateCollider(false, Vector2.zero);
        }
    }
}