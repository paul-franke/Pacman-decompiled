using System;
using System.Runtime.InteropServices;

namespace PacmanCS
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct XYPOS
    {
        public byte y; // l
        public byte x; // h
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TASK
    {
        public byte timer;
        public byte func;
        public byte param;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct SOUND_EFFECT
    {
        public byte mask;
        public byte __unused_1;
        public byte current;
        public byte selected;           // high nybble = freq scale, low nyb = sound effect
        public byte frequencyInitial;   // diff mean in song and effect? use union? song=scale
        public byte frequencyDelta;
        public ushort offset;
        public byte repeat;             // 8
        public byte volumeInitial;      // 9
        public byte volumeDelta;        // a
        public byte type;               // b
        public byte duration;           // c
        public byte dir;                // d
        public byte frequency;          // e 
        public byte volume;             // f
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct SOUNDREGS
    {
        public fixed byte v1FreqCount[5];     // 0x5040
        public byte v1WaveForm;                 // 0x5045
        public fixed byte v2FreqCount[4];     // 0x5046
        public byte v2WaveForm;                 // 0x504a
        public fixed byte v3FreqCount[4];     // 0x504b
        public byte v3WaveForm;                 // 0x504f
        public fixed byte v1Frequency[5];     // 0x5050
        public byte v1Volume;                   // 0x5055
        public fixed byte v2Frequency[4];     // 0x5056
        public byte v2Volume;                   // 0x505a
        public fixed byte v3Frequency[4];     // 0x505b
        public byte v3Volume;                   // 0x505f
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct pixel
    {
        public byte r;
        public byte g;
        public byte b;
        public byte unused;
    }
}
