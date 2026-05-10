using Il2Cpp;
using MelonLoader;
using MelonLoader.Utils;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Miside_Zero_Dialogue_Override
{
    public class Mod : MelonMod
    {
        public static string GameVersion => Application.version;
        bool IsGameScene => SceneManager.GetActiveScene().name == "Version 1.9 POST";

        DialogueTree[] trees;

        public static List<DialogueNode> MappedNodes;
        public static DialogueForest CustomDtos { get; set; }
        List<DialogueNodeDTO> mappedDtos;

        public static bool disabled = false;

        private static readonly string dialougePacksPath = Path.Combine(MelonEnvironment.ModsDirectory, "mszdlg");
        public static readonly string tmp = Path.Combine(Application.temporaryCachePath, "Miside Zero Dialouge Override");
        private static readonly string nodesJsonPath = Path.Combine(tmp, "nodes.json");

        private static AudioSource source;

        public static MelonLogger.Instance Logger;

        public static float AvgDt;
        private const float smoothing = 5f;

        /// <summary>
        /// Gets or sets whether user dialogue packs should be loaded.
        /// Defaults to true.
        /// </summary>
        public bool PacksEnabled { get; set; } = true;

        public Mod()
        {
            Logger = LoggerInstance;
        }

        public override void OnLateInitializeMelon()
        {
            if (!PacksEnabled)
            {
                LoggerInstance.Msg("Custom packs have been disabled by a mod." +
                    "The mod will probably load its own pack instead.");
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
            try
            {
                LoadMszdlg(file);
                LoggerInstance.Msg("Loaded custom dialogue!");
            }
            catch (System.Exception ex)
            {
                throw new System.InvalidOperationException($"{ex.GetType().Name} while reading dialogue pack \"{file}\": {ex.Message}");
            }
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (!IsGameScene) return;

            LoggerInstance.Msg("Mapping game dialogue...");
            trees = Object.FindObjectsOfType<DialogueTree>();

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
            Object.DontDestroyOnLoad(audioHost);
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
            CustomDtos = JsonConvert.DeserializeObject<DialogueForest>(json);
        }
    }
}