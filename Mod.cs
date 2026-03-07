using Il2Cpp;
using MelonLoader;
using MelonLoader.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using WMPLib;
using System.IO;
using System.IO.Compression;
using UnityEngine.SceneManagement;
using HarmonyLib;

namespace Miside_Zero_Dialouge_Override
{
    public class Mod : MelonMod
    {
        public static string GameVersion => UnityEngine.Application.version;
        bool isGameScene => SceneManager.GetActiveScene().name == "Version 1.9 POST";

        DialogueTree[] trees;

        public static List<DialogueNode> MappedNodes;
        public static DialogueForest customDtos;
        List<DialogueNodeDTO> mappedDtos;

        public static bool disabled = false;

        string dialougePacksPath = Path.Combine(MelonEnvironment.ModsDirectory, "mszdlg");
        public static string tmp => Path.Combine(UnityEngine.Application.temporaryCachePath, "Miside Zero Dialouge Override");
        string nodesJsonPath => Path.Combine(tmp, "nodes.json");

        public static WindowsMediaPlayer wmp = new WindowsMediaPlayer();


        public override void OnInitializeMelon()
        {
            Directory.CreateDirectory(dialougePacksPath);
            string[] files = Directory.GetFiles(dialougePacksPath);
            if (files.Length == 0) 
                throw new InvalidOperationException($"No packs found in {dialougePacksPath}");

            string file = files[0];
            try
            {
                if (Directory.Exists(tmp)) Directory.Delete(tmp, true);
                ZipFile.ExtractToDirectory(file, tmp);
                customDtos = NodeAudioManager.LoadJson(nodesJsonPath);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException ($"{ex.GetType().Name} while reading dialogue pack \"{file}\": {ex.Message}");
            }
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (!isGameScene) return;

            trees = UnityEngine.Object.FindObjectsOfType<DialogueTree>();

            MappedNodes = trees.SelectMany(t => t.GetAllNodes()).ToList();
            mappedDtos = MappedNodes.Select(node => new DialogueNodeDTO
            {
                id = MappedNodes.IndexOf(node),
                dialogueText = node.dialogueText,
                speakerName = node.speakerName,
                delay = node.delay,
                nextNodeIds = node.nextNodes?
                     .Where(n => n != null)
                     .Select(n => MappedNodes.IndexOf(n))
                     .ToArray()
            }).ToList();
        }
    }

    [HarmonyPatch(typeof(DialogueTree), "PlayNode")]
    public static class DialogueTreePatch
    {
        static void Prefix(DialogueTree __instance, DialogueNode node)
        {
            if (node == null) return;

            MelonLogger.Msg("Searching for matching dialouge");
            int index = Mod.MappedNodes.FindIndex(n => n == node);
            if (index == -1) return;
            MelonLogger.Msg("Getting dtos...");
            DialogueNodeDTO dto = Mod.customDtos.nodes[index];
            // might add support for this soon, currently inacessible without
            // manually editing json and also may just not work
            node.dialogueText = dto.dialogueText;

            Mod.wmp.URL = NodeAudioManager.GetNodeAudioClip(dto);
            Mod.wmp.controls.play();
        }
    }
}
