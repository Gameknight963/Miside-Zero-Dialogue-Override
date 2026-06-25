using System.Collections.Generic;

namespace MZDO.Shared
{
    public class DialoguePack
    {
        public int PackFormat = PacksInfo.PacksFormatVersion;
        public string TargetGameVersion = "Alpha 0.72"; // note to future me MAKE SURE TO UPDATE THIS!!!!!!!!
        public List<DialogueTreeDTO> trees;
    }
}
