using System.IO;

namespace MZDO
{
    public class NodeAudioManager
    {
        public static string GetNodeAudioPath(int treeIndex, int nodeId)
        {
            if (!Directory.Exists(Core.tmp)) return null;
            string[] files = Directory.GetFiles(Core.tmp, $"{treeIndex}_{nodeId}.*");
            return files.Length > 0 ? files[0] : null;
        }
    }
}
