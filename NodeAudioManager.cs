using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miside_Zero_Dialouge_Override
{
    public class NodeAudioManager
    {
        public static bool DoesNodeAudioExist(DialogueNodeDTO node)
        {
            if (!Directory.Exists(Mod.tmp)) return false;
            string[] files = Directory.GetFiles(
                Mod.tmp,
                $"{node.id}.*"
            );
            return (files.Length > 0);
        }
        public static string GetNodeAudioPath(DialogueNodeDTO node)
        {
            if (!Directory.Exists(Mod.tmp))
            {
                Directory.CreateDirectory(Mod.tmp);
                return null;
            }
            string[] files = Directory.GetFiles(
                Mod.tmp,
                $"{node.id}.*"
            );
            return (files.Length > 0 ? files[0] : null);
        }
        public static DialogueForest LoadJson(string path)
        {
            string json = File.ReadAllText(path);
            DialogueForest forest = JsonConvert.DeserializeObject<DialogueForest>(json);
            return forest;
        }
    }
}
