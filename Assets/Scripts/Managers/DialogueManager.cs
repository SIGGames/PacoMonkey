using System.Linq;
using TMPro;
using UI.Dialogues;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace Managers {
    public class DialogueManager : MonoBehaviour {
        public static DialogueManager Instance { get; private set; }

        public Sprite playerSprite;

        // Components
        public GameObject DialoguePanel { get; private set; }
        public TextMeshProUGUI DialogueText { get; private set; }
        public TextMeshProUGUI DialogueTitle { get; private set; }
        public Image DialogueImage { get; private set; }
        public Image DialogueNextStepImage { get; private set; }

        // Components Titles
        private const string DialoguePanelIdentifier = "DialoguePanel";
        private const string DialogueTextIdentifier = "DialogueText";
        private const string DialogueTitleIdentifier = "DialogueTitle";
        private const string DialogueImageIdentifier = "DialogueImage";
        private const string DialogueNextStepImageIdentifier = "DialogueNextStepImage";

        private void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            DialoguePanel = FindObjectsOfType<GameObject>(true).FirstOrDefault(x => x.name == DialoguePanelIdentifier);

            Debug.Assert(DialoguePanel != null, nameof(DialoguePanel) + " != null");
            DialogueText = DialoguePanel.transform.Find(DialogueTextIdentifier)?.GetComponent<TextMeshProUGUI>();
            DialogueTitle = DialoguePanel.transform.Find(DialogueTitleIdentifier)?.GetComponent<TextMeshProUGUI>();
            DialogueImage = DialoguePanel.transform.Find(DialogueImageIdentifier)?.GetComponent<Image>();
            DialogueNextStepImage = DialoguePanel.transform.Find(DialogueNextStepImageIdentifier)?.GetComponent<Image>();

            Debugger.LogIfNull((nameof(DialoguePanel), DialoguePanel), (nameof(DialogueText), DialogueText),
                (nameof(DialogueTitle), DialogueTitle), (nameof(DialogueImage), DialogueImage),
                (nameof(DialogueNextStepImage), DialogueNextStepImage), (nameof(playerSprite), playerSprite));
        }

        public static void ResetDialogues() {
            Dialogue[] dialogues = FindObjectsOfType<Dialogue>(true);
            foreach (Dialogue dialogue in dialogues) {
                dialogue.ResetDialogue();
            }

            FloatingDialogue[] floatingDialogues = FindObjectsOfType<FloatingDialogue>(true);
            foreach (FloatingDialogue dialogue in floatingDialogues) {
                dialogue.ResetDialogue();
            }
        }
    }
}