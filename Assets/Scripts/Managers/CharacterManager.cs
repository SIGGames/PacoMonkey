using Controllers;
using Enums;
using Mechanics.Movement;
using UnityEngine;

namespace Managers {
    public class CharacterManager : MonoBehaviour {
        [System.Serializable]
        public class CharacterConfiguration {
            public Character characterType;
            public Sprite characterSprite;
            public Vector2 colliderOffset;
            public Vector2 colliderSize;
            public float crouchSpeedMultiplier;
            public float maxRunSpeed;
            public float jumpModifier;
        }

        [Header("Character Configurations")]
        public CharacterConfiguration[] characters;

        [Header("References")]
        public SpriteRenderer spriteRenderer;
        public BoxCollider2D boxCollider;
        public PlayerController playerController;
        public Crouch crouch;

        private Character _currentCharacter;

        private void Start() {
            SetCharacter(Character.Micca);
        }

        public void SetCharacter(Character character) {
            foreach (var config in characters) {
                if (config.characterType == character) {
                    ApplyCharacterConfiguration(config);
                    _currentCharacter = character;
                    return;
                }
            }

            Debug.LogWarning($"Character {character} not found in configurations.");
        }

        private void ApplyCharacterConfiguration(CharacterConfiguration config) {
            spriteRenderer.sprite = config.characterSprite;

            // Update the collider
            if (boxCollider != null) {
                boxCollider.offset = config.colliderOffset;
                boxCollider.size = config.colliderSize;
            }

            // Update player settings
            if (playerController != null) {
                playerController.maxRunSpeed = config.maxRunSpeed;
                playerController.jumpModifier = config.jumpModifier;
            }

            // Update crouch settings
            if (crouch != null) {
                crouch.crouchSpeedMultiplier = config.crouchSpeedMultiplier;
            }
        }

        public Character GetCurrentCharacter() {
            return _currentCharacter;
        }
    }
}
