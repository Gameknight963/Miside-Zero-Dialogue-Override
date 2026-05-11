using System.IO;

namespace MZDO
{
    public class NodeAudioManager
    {
        public static bool DoesNodeAudioExist(DialogueNodeDTO node)
        {
            if (!Directory.Exists(Core.tmp)) return false;
            string[] files = Directory.GetFiles(
                Core.tmp,
                $"{node.id}.*"
            );
            return (files.Length > 0);
        }
        public static string GetNodeAudioPath(DialogueNodeDTO node)
        {
            if (!Directory.Exists(Core.tmp))
            {
                Directory.CreateDirectory(Core.tmp);
                return null;
            }
            string[] files = Directory.GetFiles(
                Core.tmp,
                $"{node.id}.*"
            );
            return (files.Length > 0 ? files[0] : null);
        }
    }
}
