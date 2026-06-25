using Il2Cpp;
using MelonLoader;
using MelonLoader.Utils;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEngine;
using UnityEngine.SceneManagement;
using MZDO.Shared;

namespace MZDO
{
    public class Core : MelonMod
    {
        bool IsGameScene => SceneManager.GetActiveScene().name == "Version 1.9 POST";

        DialogueTree[] trees;

        public static List<DialogueNode> MappedNodes;
        public static DialoguePack Pack { get; set; }

        private static readonly string dialougePacksPath = Path.Combine(MelonEnvironment.ModsDirectory, "mszdlg");
        public static readonly string tmp = Path.Combine(Application.temporaryCachePath, "Miside Zero Dialogue Override");
        private static readonly string nodesJsonPath = Path.Combine(tmp, "nodes.json");

        public static MelonLogger.Instance Logger;

        public static float AvgDt;
        private const float smoothing = 5f;

        public static System.Action<DialogueNode> OnNodePlayed;
        public static System.Action<DialogueNodeDTO> OnDTOPlayed;

        /// <summary>
        /// Gets or sets whether user dialogue packs should be loaded.
        /// Defaults to true.
        /// </summary>
        public static bool UserPacksEnabled { get; set; } = true;

        public override void OnEarlyInitializeMelon()
        {
            Logger = LoggerInstance;
        }

        public override void OnLateInitializeMelon()
        {
            if (!UserPacksEnabled)
            {
                LoggerInstance.Msg("User packs have been disabled by a mod.");
                return;
            }
            LoggerInstance.Msg("Loading custom dialogue pack...");
            Directory.CreateDirectory(dialougePacksPath);
            string[] files = Directory.GetFiles(dialougePacksPath);
            if (files.Length == 0)
            {
                LoggerInstance.Error($"No packs found in {dialougePacksPath}");
                return;
            }

            string file = files[0];
            LoadMszdlg(file);
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (!IsGameScene) return;
            if (Pack == null) return;
            bool wasSuccess = true;

            LoggerInstance.Msg("Patching game dialogue...");
            trees = Object.FindObjectsOfType<DialogueTree>();

            for (int i = 0; i < trees.Length; i++)
            {
                if (i >= Pack.trees.Count) break;
                Dictionary<int, DialogueNode> nodeLookup = [];
                foreach (DialogueNodeDTO dto in Pack.trees[i].nodes)
                {
                    DialogueNode node;

                    if (dto.id >= 0)
                    {
                        if (!nodeLookup.TryGetValue(dto.id, out node))
                        {
                            node = trees[i].GetAllNodes()[dto.id];
                            nodeLookup[dto.id] = node;
                        }
                    }
                    else
                    {
                        node = ScriptableObject.CreateInstance<DialogueNode>();
                        nodeLookup[dto.id] = node;
                    }

                    node.dialogueText = dto.dialogueText;
                    node.speakerName = dto.speakerName;
                    node.delay = dto.delay;
                    node.expression = dto.expression ?? ""; // null expression value causes dialogue to stop
                    string audioPath = NodeAudioManager.GetNodeAudioPath(i, dto.id);
                    if (audioPath != null)
                    {
                        node.voiceClip = VoidLib2.AudioImporter.Bass.LoadAudio(audioPath, out int code);
                        if (code != 0) LoggerInstance.Error($"Bass audio import failed with code {code}");
                    }
                }

                foreach (DialogueNodeDTO dto in Pack.trees[i].nodes)
                {
                    if (!nodeLookup.TryGetValue(dto.id, out DialogueNode node))
                        continue;

                    List<DialogueNode> patchedNextNodes = [];

                    if (dto.nextNodeIds != null)
                    {
                        foreach (int nextId in dto.nextNodeIds)
                        {
                            if (nodeLookup.TryGetValue(nextId, out DialogueNode nextNode))
                            {
                                patchedNextNodes.Add(nextNode);
                            }
                        }
                    }
                    node.nextNodes = patchedNextNodes.ToArray();
                }
            }
            if (wasSuccess) LoggerInstance.Msg("Dialogue patched successfully");
            else LoggerInstance.Warning("Some parts of the dialogue patching failed. This may cause incorrect behavior.");
            DialogueEvents.OnDialoguePatched?.Invoke();
        }

        public override void OnUpdate()
        {
            AvgDt = (AvgDt * (smoothing - 1) + Time.unscaledDeltaTime) / smoothing;
        }

        public static void LoadMszdlg(string path)
        {
            if (Directory.Exists(tmp)) Directory.Delete(tmp, true);
            ZipFile.ExtractToDirectory(path, tmp);
            string json = File.ReadAllText(nodesJsonPath);
            Pack = JsonConvert.DeserializeObject<DialoguePack>(json);
            if (Pack.PackFormat != PacksInfo.PacksFormatVersion)
                Logger.Warning($"Pack format mismatch: expected {PacksInfo.PacksFormatVersion}, got {Pack.PackFormat}. Dialogue may not work as expected.");
            if (Pack.TargetGameVersion != Application.version)
                Logger.Warning($"Pack targets game version {Pack.TargetGameVersion} but current version is {Application.version}. " +
                    $"Dialogue may not work as expected.");

            Logger.Msg($"Loaded dialogue file {Path.GetFileName(path)}.");
            DialogueEvents.OnPackLoaded?.Invoke(Pack);
        }
    }
}