using Il2Cpp;
using System;

namespace MZDO
{
    public static class DialogueEvents
    {
        public static Action<DialogueNode> OnNodePlayed;
        public static Action<DialoguePack> OnPackLoaded;
        public static Action OnDialoguePatched;
    }
}
