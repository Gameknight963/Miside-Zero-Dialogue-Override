using Il2Cpp;
using HarmonyLib;
using UnityEngine;

namespace MZDO
{
    [HarmonyPatch(typeof(DialogueTree), "PlayNode")]
    public static class DialogueTreePatch
    {
        static bool warned = false;
        static void Prefix(DialogueNode node)
        {
            Core.OnNodePlayed.Invoke(node);

            if (Core.CustomDtos == null)
            {
                if (!warned)
                {
                    Core.Logger.Error("Attempted to patch but no custom pack was loaded!");
                    warned = true;
                }
                return;
            }

            warned = false;

            if (node == null)
            {
                Core.Logger.Warning("node is null, returning...");
                return;
            }
            int index = Core.MappedNodes.FindIndex(n => n == node);

            if (index == -1)
            {
                Core.Logger.Error("node is not mapped, returning...");
                return;
            }

            if (Core.CustomDtos.nodes == null || index >= Core.CustomDtos.nodes.Count)
            {
                Core.Logger.Error($"customDtos missing for index {index}, returning...");
                return;
            }

            DialogueNodeDTO dto = Core.CustomDtos.nodes[index];
            if (dto == null)
            {
                Core.Logger.Error($"dto at index {index} is null, returning...");
                return;
            }
            try
            {
                string path = NodeAudioManager.GetNodeAudioPath(dto);
                AudioClip clip = null;
                if (path != null)
                {
                    clip = AudioImporter.LoadAudio(path);
                    if (clip == null)
                    {
                        Core.Logger.Error("bass.dll audio import failed, returning...");
                        return;
                    }
                }

                // we're forced to estimate how long it will take based on fps due
                // to il2cpp making patching coroutines impossible

                // not an ideal fix
                float typeSpeed = DialogueManager.instance.typeSpeed;
                float predictedTime = dto.dialogueText.Length * Mathf.Max(typeSpeed, Core.AvgDt);
                float fpsCompensation = predictedTime - dto.dialogueText.Length * typeSpeed;
                float clipLengthCompensation = clip is null ? 0 : clip.length - predictedTime;
                node.dialogueText = dto.dialogueText;
                node.delay += fpsCompensation + clipLengthCompensation;
                node.voiceClip = clip;
                Core.OnDTOPlayed.Invoke(dto);
            }
            catch (System.Exception ex)
            {
                Core.Logger.Error($"Failed to patch node with text \"{node.dialogueText}\" " +
                    $"due to {ex.GetType().Name}: {ex.Message}");
            }
        }
    }
}
