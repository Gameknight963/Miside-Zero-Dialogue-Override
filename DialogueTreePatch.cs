using HarmonyLib;
using Il2Cpp;

namespace MZDO
{
    [HarmonyPatch(typeof(DialogueTree), "PlayNode")]
    public static class DialogueTreePatch
    {
        static void Prefix(DialogueTree __instance, DialogueNode node)
        {
            DialogueEvents.OnNodePlayed?.Invoke(node);
            if (node.voiceClip)
                __instance.GetSpeakerAudioSource(node.speakerName)?.pitch = 1f;
        }
    }
}
