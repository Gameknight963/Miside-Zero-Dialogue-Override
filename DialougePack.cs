using System.Collections.Generic;
using UnityEngine;

namespace MZDO
{
    public class DialoguePack
    {
        public int PackFormat = 1;
        public string TargetGameVersion = Application.version;
        public List<DialogueTreeDTO> trees;
    }
}
