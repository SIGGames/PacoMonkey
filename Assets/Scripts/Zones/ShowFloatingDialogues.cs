using System.Collections;
using UI.Dialogues;
using UnityEngine;
using static Utils.TagUtils;

namespace Zones {
    [RequireComponent(typeof(Collider2D))]
    public class ShowFloatingDialogues : MonoBehaviour {
        [SerializeField]
        private FloatingDialogue[] floatingDialogues;

        [SerializeField, Range(0, 10)]
        private float delayTime = 1f;

        [SerializeField, Range(0, 20)]
        private float nextLineDelayTime = 5f;

        [SerializeField]
        private bool showAlternativeDialogues;

        private Coroutine _dialoguesCoroutine;

        private void OnTriggerEnter2D(Collider2D col) {
            if (col.gameObject.CompareTag(Player) && _dialoguesCoroutine == null) {
                _dialoguesCoroutine = StartCoroutine(ShowDialoguesWithDelay());
            }
        }

        private IEnumerator ShowDialoguesWithDelay() {
            foreach (var floatingDialogue in floatingDialogues) {
                StartCoroutine(ShowDialogue(floatingDialogue));
                yield return new WaitForSeconds(delayTime);
            }

            _dialoguesCoroutine = null;
        }


        private IEnumerator ShowDialogue(FloatingDialogue dialogue) {
            dialogue.mustShowAlternativeDialogue = showAlternativeDialogues;
            dialogue.ShowDialogue(false);
            yield return new WaitForSeconds(delayTime);
            while (dialogue.IsDialogueActive()) {
                dialogue.NextLine();
                yield return new WaitForSeconds(nextLineDelayTime);
            }
        }
    }
}