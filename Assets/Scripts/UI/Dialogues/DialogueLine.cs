using System;
using Controllers;
using Enums;

namespace UI.Dialogues {
    [Serializable]
    public class DialogueLine {
        public DialogueCharacter character = DialogueCharacter.Npc;
        public string text;
    }
}