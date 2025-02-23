using Managers;
using UnityEditor;
using UnityEngine;
using static Utils.TagUtils;

namespace Mechanics.Environment {
    public class PlayerDamageTile : MonoBehaviour {
        [SerializeField, HalfStepSlider(0, 10)]
        private float damage = 1f;

        private void OnTriggerEnter2D(Collider2D col) {
            if (col.CompareTag(DamageTile) || col.CompareTag(Player)) {
                CharacterManager.Instance.currentPlayerController.TakeDamage(damage);
            }
        }
    }
}