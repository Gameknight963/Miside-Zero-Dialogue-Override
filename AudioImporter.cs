using Miside_Zero_Dialogue_Override;
using UnityEngine;
using System;

public static class AudioImporter
{
    public static AudioClip LoadAudio(string filePath)
    {
        // Flags: BASS_SAMPLE_FLOAT (256) | BASS_STREAM_DECODE (2097152)
        uint flags = 256 | 2097152;

        // Initialize (device -1 is default)
        NativeBass.BASS_Init(-1, 44100, 0, IntPtr.Zero, IntPtr.Zero);

        // Create stream
        int handle = NativeBass.BASS_StreamCreateFile(false, filePath, 0, 0, flags);
        if (handle == 0) return null;

        // Get metadata
        NativeBass.BASS_ChannelGetInfo(handle, out var info);
        long lengthBytes = NativeBass.BASS_ChannelGetLength(handle, 0); // 0 = BASS_POS_BYTE
        int totalSamples = (int)(lengthBytes / 4);

        // Pull raw float data
        float[] sampleBuffer = new float[totalSamples];
        NativeBass.BASS_ChannelGetData(handle, sampleBuffer, (int)lengthBytes);

        // Build the Unity clip
        AudioClip clip = AudioClip.Create(
            System.IO.Path.GetFileName(filePath),
            totalSamples / info.chans,
            info.chans,
            info.freq,
            false
        );
        clip.SetData(sampleBuffer, 0);

        NativeBass.BASS_StreamFree(handle);
        return clip;
    }

}
