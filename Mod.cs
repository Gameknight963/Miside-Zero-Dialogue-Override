using HarmonyLib;
using Il2Cpp;
using Il2CppSystem;
using MelonLoader;
using MelonLoader.Utils;
using Miside_Zero_Dialogue_Override;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Miside_Zero_Dialogue_Override
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
        static void Prefix(DialogueNode node)
        {
            if (node == null)
            {
                Mod.Logger.Msg("node is null, returning...");
                return;
            }
            try
            {
                int index = Mod.MappedNodes.FindIndex(n => n == node);

                if (index == -1)
                {
                    Mod.Logger.Msg("node does not have an audio clip, returning...");
                    return;
                }

                if (Mod.customDtos == null || Mod.customDtos.nodes == null || index >= Mod.customDtos.nodes.Count)
                {
                    Mod.Logger.Msg($"customDtos missing for index {index}, returning...");
                    return;
                }

                DialogueNodeDTO dto = Mod.customDtos.nodes[index];
                if (dto == null)
                {
                    Mod.Logger.Msg($"dto at index {index} is null, returning...");
                    return;
                }

                string path = NodeAudioManager.GetNodeAudioPath(dto);
                AudioClip clip = AudioImporter.LoadAudio(path);
                if (clip == null)
                {
                    Mod.Logger.Msg("bass.dll asudio import failed, returning...");
                    return;
                }

                // we're forced to estimate how long it will take based on fps due
                // to il2cpp making patching coroutines impossible

                // not an ideal fix.

                // i couldn't figure out how to get the variable for
                // some reason, if he changes typeSpeed im cooked

                float typeSpeed = 0.025f;
                float predictedTime = dto.dialogueText.Length * Mathf.Max(typeSpeed, Mod.AvgDt);
                float fpsCompensation = predictedTime - dto.dialogueText.Length * typeSpeed;
                float clipLengthCompensation = clip.length - predictedTime;
                node.dialogueText = dto.dialogueText;
                node.delay = fpsCompensation + clipLengthCompensation; // ignoring node delay for now
                node.voiceClip = clip;
            }
            catch (System.Exception ex)
            {
                Mod.Logger.Error($"Failed to patch node with text \"{node.dialogueText}\" " +
                    $"due to {ex.GetType().Name}: {ex.Message}");
            }
        }
    }
}