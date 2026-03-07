using System;
using System.Runtime.InteropServices;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miside_Zero_Dialouge_Override
{
    // get base.dll at https://www.un4seen.com/

    public class NativeBass
    {
        private const string BASS_LIB = "bass"; 

        [DllImport(BASS_LIB)]
        public static extern bool BASS_Init(int device, int freq, uint flags, IntPtr win, IntPtr clsid);

        [DllImport(BASS_LIB)]
        public static extern int BASS_StreamCreateFile(bool mem, string file, long offset, long length, uint flags);

        [DllImport(BASS_LIB)]
        public static extern bool BASS_ChannelGetInfo(int handle, out BASS_CHANNELINFO info);

        [DllImport(BASS_LIB)]
        public static extern long BASS_ChannelGetLength(int handle, uint mode);

        [DllImport(BASS_LIB)]
        public static extern int BASS_ChannelGetData(int handle, [Out] float[] buffer, int length);

        [DllImport(BASS_LIB)]
        public static extern bool BASS_StreamFree(int handle);

        [StructLayout(LayoutKind.Sequential)]
        public struct BASS_CHANNELINFO
        {
            public int freq;
            public int chans;
            public uint flags;
            public uint ctype;
            public uint origres;
            public int plugin;
            public int sample;
            public IntPtr filename;
        }
    }
}
