using System.Collections.Generic;
using UnityEngine;

namespace MZDO
{
    public class DialoguePack
    {
        public int PackFormat = Core.PackFormatVersion;
        public string TargetGameVersion;
        public List<DialogueTreeDTO> trees;
        public void MatchGameVersion() => TargetGameVersion = Application.version;
    }
}
