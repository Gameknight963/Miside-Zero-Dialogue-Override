using System.Collections.Generic;
using UnityEngine;

namespace MZDO
{
    public class DialoguePack
    {
        public int PackFormat = Core.PackFormatVersion;
        public string TargetGameVersion = Application.version;
        public List<DialogueTreeDTO> trees;
    }
}
