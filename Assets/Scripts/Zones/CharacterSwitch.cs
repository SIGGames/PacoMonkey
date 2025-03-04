using Enums;
using Managers;
using UnityEngine;
using static Utils.TagUtils;

namespace Zones {
    [RequireComponent(typeof(Collider2D))]
    public class CharacterSwitch : MonoBehaviour {
        [SerializeField] private Character character = Character.Micca1;

        private void OnTriggerEnter2D(Collider2D col) {
            if (col.gameObject.CompareTag(Player) && CharacterManager.Instance.GetCurrentCharacter() != character) {
                CharacterManager.Instance.SetCharacter(character);
            }
        }
    }
}