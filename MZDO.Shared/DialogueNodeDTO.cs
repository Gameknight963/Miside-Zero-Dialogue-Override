namespace MZDO.Shared
{
    public class DialogueNodeDTO
    {
        public int id;
        public int[] nextNodeIds;
        public string dialogueText;
        public string speakerName;
        public string expression;
        public float delay;
    }
}
