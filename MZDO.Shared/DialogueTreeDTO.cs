using System.Collections.Generic;

namespace MZDO.Shared
{
    public class DialogueTreeDTO
    {
        public List<DialogueNodeDTO> nodes;
        public List<int> startNodeIds;
        public float? chirpTime;
        public float? initialDelay;
        public float? exitDelay;
        public string name;
    }
}
