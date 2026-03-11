using Il2Cpp;
using MelonLoader;
using MelonLoader.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.IO;
using System.IO.Compression;
using UnityEngine.SceneManagement;
using HarmonyLib;
using System.Diagnostics;
using System.Reflection;
using System.Linq.Expressions;
using UnityEngine.UIElements;
using Il2CppSystem;

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
        
        private static AudioSource source;

        public static MelonLogger.Instance Logger;

        public static float AvgDt;
        private float smoothing = 5f;

        public override void OnInitializeMelon()
        {
            Logger = LoggerInstance; // for use outside this class

            LoggerInstance.Msg("Loading custom dialogue pack...");
            Directory.CreateDirectory(dialougePacksPath);
            string[] files = Directory.GetFiles(dialougePacksPath);
            if (files.Length == 0) 
                throw new System.InvalidOperationException($"No packs found in {dialougePacksPath}");

            string file = files[0];
            try
            {
                if (Directory.Exists(tmp)) Directory.Delete(tmp, true);
                ZipFile.ExtractToDirectory(file, tmp);
                customDtos = NodeAudioManager.LoadJson(nodesJsonPath);
                LoggerInstance.Msg("Loaded custom dialogue!");
            }
            catch (System.Exception ex)
            {
                throw new System.InvalidOperationException($"{ex.GetType().Name} while reading dialogue pack \"{file}\": {ex.Message}");
            }
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (!isGameScene) return;

            LoggerInstance.Msg("Mapping game dialogue...");
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

            LoggerInstance.Msg("Creating audiohost...");
            GameObject audioHost = new GameObject("AudioHost");
            source = audioHost.AddComponent<AudioSource>();
            UnityEngine.Object.DontDestroyOnLoad(audioHost);
        }

        public override void OnUpdate()
        {
            AvgDt = (AvgDt * (smoothing - 1) + Time.unscaledDeltaTime) / smoothing;
        }
    }

    [HarmonyPatch(typeof(DialogueTree), "PlayNode")]
    public static class DialogueTreePatch
    {

        static void Prefix(DialogueTree __instance, DialogueNode node)
        {
            if (node == null) return;

            int index = Mod.MappedNodes.FindIndex(n => n == node);
            if (index == -1) return;

            DialogueNodeDTO dto = Mod.customDtos.nodes[index];

            string path = NodeAudioManager.GetNodeAudioPath(dto);
            AudioClip clip = AudioImporter.LoadAudio(path);

            // we're forced to estimate how long it will take based on fps due
            // to multiple factors.

            // not an ideal fix.

            // i couldn't figure out how to get the variable for
            // some reason, if he changes typeSpeed im cooked

            float typeSpeed = 0.025f; 
            float perChar = Mathf.Ceil(typeSpeed / Mod.AvgDt) * Mod.AvgDt;
            float totalTime = dto.dialogueText.Length * perChar;

            node.dialogueText = dto.dialogueText;
            node.delay = dto.delay + totalTime;
            node.voiceClip = clip;
        }
    }
}
