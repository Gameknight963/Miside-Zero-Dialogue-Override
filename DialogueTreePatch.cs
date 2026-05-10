using Il2Cpp;
using HarmonyLib;
using UnityEngine;

namespace Miside_Zero_Dialogue_Override
{
    [HarmonyPatch(typeof(DialogueTree), "PlayNode")]
    public static class DialogueTreePatch
    {
        static void Prefix(DialogueTree __instance, DialogueNode node)
        {
            if (node == null)
            {
                Mod.Logger.Warning("node is null, returning...");
                return;
            }
            int index = Mod.MappedNodes.FindIndex(n => n == node);

            if (index == -1)
            {
                Mod.Logger.Error("node does not have an audio clip, returning...");
                return;
            }

            if (Mod.CustomDtos == null)
            {
                Mod.Logger.Error("No custom dialogue loaded");
                return;
            }

            if (Mod.CustomDtos.nodes == null || index >= Mod.CustomDtos.nodes.Count)
            {
                Mod.Logger.Error($"customDtos missing for index {index}, returning...");
                return;
            }

            DialogueNodeDTO dto = Mod.CustomDtos.nodes[index];
            if (dto == null)
            {
                Mod.Logger.Error($"dto at index {index} is null, returning...");
                return;
            }
            try
            {
                string path = NodeAudioManager.GetNodeAudioPath(dto);
                AudioClip clip = AudioImporter.LoadAudio(path);
                if (clip == null)
                {
                    Mod.Logger.Error("bass.dll audio import failed, returning...");
                    return;
                }

                // we're forced to estimate how long it will take based on fps due
                // to il2cpp making patching coroutines impossible

                // not an ideal fix
                float typeSpeed = DialogueManager.instance.typeSpeed;
                float predictedTime = dto.dialogueText.Length * Mathf.Max(typeSpeed, Mod.AvgDt);
                float fpsCompensation = predictedTime - dto.dialogueText.Length * typeSpeed;
                float clipLengthCompensation = clip.length - predictedTime;
                node.dialogueText = dto.dialogueText;
                node.delay += fpsCompensation + clipLengthCompensation;
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
