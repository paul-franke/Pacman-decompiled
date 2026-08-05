// =============================================================================
// SoundPlayer / Program.cs
//
// Pac-Man Authentic Sound Effects & Songs Player in C#
// Uses 1/60th second frame rate logic (60 Hz) and Windows WinMM (waveOut @ 48 kHz).
// Real-time formatted printf output of sound parameters for Voice 1, Voice 2, Voice 3.
// Generates live speaker audio AND saves high-fidelity .wav files into output_audio/.
// =============================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;

namespace SoundPlayer
{
    // ── Kernel32 Event API Definitions ──────────────────────────────────────────
    public static class Kernel32
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr CreateEvent(IntPtr lpEventAttributes, bool bManualReset, bool bInitialState, string? lpName);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool CloseHandle(IntPtr hObject);
    }

    // ── WinMM P/Invoke API Definitions ─────────────────────────────────────────
    [StructLayout(LayoutKind.Sequential)]
    public struct WAVEFORMATEX
    {
        public ushort wFormatTag;
        public ushort nChannels;
        public uint nSamplesPerSec;
        public uint nAvgBytesPerSec;
        public ushort nBlockAlign;
        public ushort wBitsPerSample;
        public ushort cbSize;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WAVEHDR
    {
        public IntPtr lpData;
        public uint dwBufferLength;
        public uint dwBytesRecorded;
        public IntPtr dwUser;
        public uint dwFlags;
        public uint dwLoops;
        public IntPtr lpDataPointer;
        public IntPtr reserved;
    }

    public static class WinMM
    {
        public const ushort WAVE_FORMAT_PCM = 1;
        public const uint WHDR_DONE = 0x00000001;
        public const uint WHDR_PREPARED = 0x00000002;
        public const uint WHDR_INQUEUE = 0x00000010;
        public const int WAVE_MAPPER = -1;
        public const uint CALLBACK_EVENT = 0x00050000;

        [DllImport("winmm.dll", SetLastError = true)]
        public static extern int waveOutOpen(out IntPtr hWaveOut, int uDeviceID, ref WAVEFORMATEX lpFormat, IntPtr dwCallback, IntPtr dwInstance, uint dwFlags);

        [DllImport("winmm.dll", SetLastError = true)]
        public static extern int waveOutPrepareHeader(IntPtr hWaveOut, IntPtr lpWaveOutHdr, int uSize);

        [DllImport("winmm.dll", SetLastError = true)]
        public static extern int waveOutWrite(IntPtr hWaveOut, IntPtr lpWaveOutHdr, int uSize);

        [DllImport("winmm.dll", SetLastError = true)]
        public static extern int waveOutUnprepareHeader(IntPtr hWaveOut, IntPtr lpWaveOutHdr, int uSize);

        [DllImport("winmm.dll", SetLastError = true)]
        public static extern int waveOutReset(IntPtr hWaveOut);

        [DllImport("winmm.dll", SetLastError = true)]
        public static extern int waveOutClose(IntPtr hWaveOut);

        [DllImport("winmm.dll", SetLastError = true)]
        public static extern int waveOutSetVolume(IntPtr hWaveOut, uint dwVolume);
    }

    // ── WAV Exporter Utility ──────────────────────────────────────────────────
    public static class WavWriter
    {
        public static void SaveWavFile(string filePath, short[] samples, int sampleRate)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            using FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            using BinaryWriter bw = new BinaryWriter(fs);

            int subchunk2Size = samples.Length * sizeof(short);
            int chunkSize = 36 + subchunk2Size;

            // RIFF header
            bw.Write(new char[] { 'R', 'I', 'F', 'F' });
            bw.Write(chunkSize);
            bw.Write(new char[] { 'W', 'A', 'V', 'E' });

            // fmt subchunk
            bw.Write(new char[] { 'f', 'm', 't', ' ' });
            bw.Write(16); // Subchunk1Size (PCM)
            bw.Write((short)1); // AudioFormat (PCM)
            bw.Write((short)1); // NumChannels (Mono)
            bw.Write(sampleRate); // SampleRate
            bw.Write(sampleRate * sizeof(short)); // ByteRate
            bw.Write((short)sizeof(short)); // BlockAlign
            bw.Write((short)16); // BitsPerSample

            // data subchunk
            bw.Write(new char[] { 'd', 'a', 't', 'a' });
            bw.Write(subchunk2Size);
            foreach (short sample in samples)
            {
                bw.Write(sample);
            }
        }
    }

    // ── Sound Effect Data Structure ───────────────────────────────────────────
    public class SoundEffect
    {
        public byte mask;
        public byte unused1;
        public byte current;
        public byte selected;           // high nybble = freq scale, low nybble = sound effect
        public byte frequencyInitial;
        public byte frequencyDelta;
        public ushort offset;
        public byte repeat;
        public byte volumeInitial;
        public byte volumeDelta;
        public byte type;
        public byte duration;
        public byte dir;
        public byte frequency;
        public byte volume;
    }

    // ── Sound Catalog Item ───────────────────────────────────────────────────
    public class SoundItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Category { get; set; } = "";
        public int ChannelIndex { get; set; } // 1, 2, 3
        public byte MaskValue { get; set; }
        public bool IsSong { get; set; }
        public int DurationFrames { get; set; }
        public string FileName { get; set; } = "";
    }

    class Program
    {
        // ── Namco WSG Hardware Sound Registers (Memory mapped at 0x5040 - 0x505F) ──
        static readonly byte[] SoundRegs = new byte[0x20];
        static readonly byte[] SoundRom = new byte[0x200]; // 512 bytes audio PROMs
        static readonly byte[] MainRom = new byte[0x4000]; // 16 KB program ROM

        // ── Sound Engine RAM Registers ───────────────────────────────────────────
        static readonly byte[] Ch1Freq = new byte[5];
        static byte Ch1Vol;
        static readonly byte[] Ch2Freq = new byte[4];
        static byte Ch2Vol;
        static readonly byte[] Ch3Freq = new byte[4];
        static byte Ch3Vol;

        static readonly SoundEffect Ch1SoundEffect = new SoundEffect();
        static readonly SoundEffect Ch2SoundEffect = new SoundEffect();
        static readonly SoundEffect Ch3SoundEffect = new SoundEffect();

        static readonly SoundEffect Ch1SoundWave = new SoundEffect();
        static readonly SoundEffect Ch2SoundWave = new SoundEffect();
        static readonly SoundEffect Ch3SoundWave = new SoundEffect();

        static uint soundCounter = 0;

        // ── WinMM Audio Thread State (48 kHz 16-bit Mono) ────────────────────────
        const int AUDIO_FREQUENCY = 48000;
        const int SAMPLE_COUNT = AUDIO_FREQUENCY / 100; // 480 samples per 10ms buffer
        const int BUFFER_COUNT = 4;

        static IntPtr hWaveOut = IntPtr.Zero;
        static IntPtr hAudioEvent = IntPtr.Zero;
        static bool audioRunning = false;
        static Thread? audioThread;

        static readonly IntPtr[] pWaveHeaders = new IntPtr[BUFFER_COUNT];
        static readonly IntPtr[] pWaveBuffers = new IntPtr[BUFFER_COUNT];

        // Captured PCM Audio Samples for WAV Export
        static readonly List<short> recordedSamples = new List<short>();
        static readonly object recordLock = new object();

        // ── Authentic ROM Fallback Data ──────────────────────────────────────────
        static readonly byte[] DefaultAudioRom1M = {
            0x07, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0d, 0x0e, 0x0e, 0x0e, 0x0d, 0x0d, 0x0c, 0x0b, 0x0a, 0x09,
            0x07, 0x05, 0x04, 0x03, 0x02, 0x01, 0x01, 0x00, 0x00, 0x00, 0x01, 0x01, 0x02, 0x03, 0x04, 0x05,
            0x07, 0x0c, 0x0e, 0x0e, 0x0d, 0x0b, 0x09, 0x0a, 0x0b, 0x0b, 0x0a, 0x09, 0x06, 0x04, 0x03, 0x05,
            0x07, 0x09, 0x0b, 0x0a, 0x08, 0x05, 0x04, 0x03, 0x03, 0x04, 0x05, 0x03, 0x01, 0x00, 0x00, 0x02,
            0x07, 0x0a, 0x0c, 0x0d, 0x0e, 0x0d, 0x0c, 0x0a, 0x07, 0x04, 0x02, 0x01, 0x00, 0x01, 0x02, 0x04,
            0x07, 0x0b, 0x0d, 0x0e, 0x0d, 0x0b, 0x07, 0x03, 0x01, 0x00, 0x01, 0x03, 0x07, 0x0e, 0x07, 0x00,
            0x07, 0x0d, 0x0b, 0x08, 0x0b, 0x0d, 0x09, 0x06, 0x0b, 0x0e, 0x0c, 0x07, 0x09, 0x0a, 0x06, 0x02,
            0x07, 0x0c, 0x08, 0x04, 0x05, 0x07, 0x02, 0x00, 0x03, 0x08, 0x05, 0x01, 0x03, 0x06, 0x03, 0x01,
            0x00, 0x08, 0x0f, 0x07, 0x01, 0x08, 0x0e, 0x07, 0x02, 0x08, 0x0d, 0x07, 0x03, 0x08, 0x0c, 0x07,
            0x04, 0x08, 0x0b, 0x07, 0x05, 0x08, 0x0a, 0x07, 0x06, 0x08, 0x09, 0x07, 0x07, 0x08, 0x08, 0x07,
            0x07, 0x08, 0x06, 0x09, 0x05, 0x0a, 0x04, 0x0b, 0x03, 0x0c, 0x02, 0x0d, 0x01, 0x0e, 0x00, 0x0f,
            0x00, 0x0f, 0x01, 0x0e, 0x02, 0x0d, 0x03, 0x0c, 0x04, 0x0b, 0x05, 0x0a, 0x06, 0x09, 0x07, 0x08,
            0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f,
            0x0f, 0x0e, 0x0d, 0x0c, 0x0b, 0x0a, 0x09, 0x08, 0x07, 0x06, 0x05, 0x04, 0x03, 0x02, 0x01, 0x00,
            0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f,
            0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f
        };

        static readonly byte[] DefaultPowerOf2Table = { 1, 2, 4, 8, 16, 32, 64, 128 };
        static readonly byte[] DefaultFreqTable = { 0x00, 0x57, 0x5c, 0x61, 0x67, 0x6d, 0x74, 0x7b, 0x82, 0x8a, 0x92, 0x9a, 0xa3, 0xad, 0xb8, 0xc3 };

        // ── Sound Catalog ────────────────────────────────────────────────────────
        static readonly SoundItem[] SoundCatalog = new SoundItem[]
        {
            new SoundItem { Id = 1,  Name = "Game Start Intro Theme (3-Channel Song)", Category = "Songs", ChannelIndex = 0, MaskValue = 0x01, IsSong = true, DurationFrames = 260, FileName = "01_game_start_intro.wav" },
            new SoundItem { Id = 2,  Name = "Intermission / Coffee Break Theme", Category = "Songs", ChannelIndex = 3, MaskValue = 0x10, IsSong = false, DurationFrames = 320, FileName = "02_intermission_theme.wav" },
            new SoundItem { Id = 3,  Name = "Waka-Waka / Eating Dot", Category = "Ch1 Effects", ChannelIndex = 1, MaskValue = 0x02, IsSong = false, DurationFrames = 180, FileName = "03_waka_waka_eating_dot.wav" },
            new SoundItem { Id = 4,  Name = "Siren 1 (Slow Background Siren)", Category = "Ch1 Effects", ChannelIndex = 1, MaskValue = 0x01, IsSong = false, DurationFrames = 180, FileName = "04_siren_1_ch1.wav" },
            new SoundItem { Id = 5,  Name = "Siren 1 (Ch2 Siren)", Category = "Ch2 Effects", ChannelIndex = 2, MaskValue = 0x01, IsSong = false, DurationFrames = 180, FileName = "05_siren_1_ch2.wav" },
            new SoundItem { Id = 6,  Name = "Siren 2 (Medium Siren)", Category = "Ch2 Effects", ChannelIndex = 2, MaskValue = 0x02, IsSong = false, DurationFrames = 180, FileName = "06_siren_2.wav" },
            new SoundItem { Id = 7,  Name = "Siren 3 (Fast Siren)", Category = "Ch2 Effects", ChannelIndex = 2, MaskValue = 0x04, IsSong = false, DurationFrames = 180, FileName = "07_siren_3.wav" },
            new SoundItem { Id = 8,  Name = "Siren 4 (Faster Siren)", Category = "Ch2 Effects", ChannelIndex = 2, MaskValue = 0x08, IsSong = false, DurationFrames = 180, FileName = "08_siren_4.wav" },
            new SoundItem { Id = 9,  Name = "Siren 5 (Cruise Elroy / Fastest Siren)", Category = "Ch2 Effects", ChannelIndex = 2, MaskValue = 0x10, IsSong = false, DurationFrames = 180, FileName = "09_siren_5.wav" },
            new SoundItem { Id = 10, Name = "Frightened Ghosts Siren (Blue Ghost Wawa)", Category = "Ch2 Effects", ChannelIndex = 2, MaskValue = 0x20, IsSong = false, DurationFrames = 180, FileName = "10_blue_ghost_frightened.wav" },
            new SoundItem { Id = 11, Name = "Eyes Returning to Ghost House", Category = "Ch2 Effects", ChannelIndex = 2, MaskValue = 0x40, IsSong = false, DurationFrames = 180, FileName = "11_ghost_eyes_returning.wav" },
            new SoundItem { Id = 12, Name = "Extra Life / Bonus Sound", Category = "Ch2 Effects", ChannelIndex = 2, MaskValue = 0x80, IsSong = false, DurationFrames = 120, FileName = "12_extra_life.wav" },
            new SoundItem { Id = 13, Name = "Pac-Man Death / Dying Crumple Animation", Category = "Ch3 Effects", ChannelIndex = 3, MaskValue = 0x01, IsSong = false, DurationFrames = 150, FileName = "13_pacman_death.wav" },
            new SoundItem { Id = 14, Name = "Ghost Eaten (200 / 400 / 800 / 1600 Pts)", Category = "Ch3 Effects", ChannelIndex = 3, MaskValue = 0x02, IsSong = false, DurationFrames = 60, FileName = "14_ghost_eaten.wav" },
            new SoundItem { Id = 15, Name = "Bonus Fruit Eaten (100-5000 Pts)", Category = "Ch3 Effects", ChannelIndex = 3, MaskValue = 0x04, IsSong = false, DurationFrames = 60, FileName = "15_fruit_eaten.wav" },
            new SoundItem { Id = 16, Name = "Credit Inserted (Coin Sound)", Category = "Ch3 Effects", ChannelIndex = 3, MaskValue = 0x08, IsSong = false, DurationFrames = 80, FileName = "16_credit_coin.wav" }
        };

        // ── WinMM Audio Driver Implementation ────────────────────────────────────
        static unsafe void InitAudioDriver()
        {
            WAVEFORMATEX wfx = new WAVEFORMATEX
            {
                wFormatTag = WinMM.WAVE_FORMAT_PCM,
                nChannels = 1,
                nSamplesPerSec = AUDIO_FREQUENCY,
                wBitsPerSample = 16,
                nBlockAlign = 2,
                nAvgBytesPerSec = AUDIO_FREQUENCY * 2,
                cbSize = 0
            };

            hAudioEvent = Kernel32.CreateEvent(IntPtr.Zero, false, false, null);
            int res = WinMM.waveOutOpen(out hWaveOut, WinMM.WAVE_MAPPER, ref wfx, hAudioEvent, IntPtr.Zero, WinMM.CALLBACK_EVENT);
            if (res != 0)
            {
                Console.WriteLine($"WARNING: waveOutOpen failed with code {res}. Audio output disabled.");
                return;
            }

            WinMM.waveOutSetVolume(hWaveOut, 0xFFFFFFFF); // Maximum master volume

            int headerSize = Marshal.SizeOf(typeof(WAVEHDR));
            short[] initBuf = new short[SAMPLE_COUNT];

            for (int i = 0; i < BUFFER_COUNT; i++)
            {
                int bufferSizeBytes = SAMPLE_COUNT * sizeof(short);
                pWaveBuffers[i] = Marshal.AllocHGlobal(bufferSizeBytes);

                Marshal.Copy(initBuf, 0, pWaveBuffers[i], SAMPLE_COUNT);

                pWaveHeaders[i] = Marshal.AllocHGlobal(headerSize);

                WAVEHDR* pHeader = (WAVEHDR*)pWaveHeaders[i];
                pHeader->lpData = pWaveBuffers[i];
                pHeader->dwBufferLength = (uint)bufferSizeBytes;
                pHeader->dwBytesRecorded = 0;
                pHeader->dwUser = IntPtr.Zero;
                pHeader->dwFlags = 0;
                pHeader->dwLoops = 0;
                pHeader->lpDataPointer = IntPtr.Zero;
                pHeader->reserved = IntPtr.Zero;

                WinMM.waveOutPrepareHeader(hWaveOut, pWaveHeaders[i], headerSize);
                WinMM.waveOutWrite(hWaveOut, pWaveHeaders[i], headerSize);
            }

            audioRunning = true;
            audioThread = new Thread(WinAudioLoop) { IsBackground = true };
            audioThread.Start();
        }

        static unsafe void CloseAudioDriver()
        {
            audioRunning = false;
            if (hAudioEvent != IntPtr.Zero) Kernel32.CloseHandle(hAudioEvent);
            audioThread?.Join(500);

            if (hWaveOut != IntPtr.Zero)
            {
                WinMM.waveOutReset(hWaveOut);
                int headerSize = Marshal.SizeOf(typeof(WAVEHDR));
                for (int i = 0; i < BUFFER_COUNT; i++)
                {
                    if (pWaveHeaders[i] != IntPtr.Zero)
                    {
                        WinMM.waveOutUnprepareHeader(hWaveOut, pWaveHeaders[i], headerSize);
                        Marshal.FreeHGlobal(pWaveHeaders[i]);
                    }
                    if (pWaveBuffers[i] != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(pWaveBuffers[i]);
                    }
                }
                WinMM.waveOutClose(hWaveOut);
                hWaveOut = IntPtr.Zero;
            }
        }

        static unsafe void WinAudioLoop()
        {
            int bufIdx = 0;
            short[] sampleBuffer = new short[SAMPLE_COUNT];
            int headerSize = Marshal.SizeOf(typeof(WAVEHDR));

            while (audioRunning)
            {
                WAVEHDR* pHeader = (WAVEHDR*)pWaveHeaders[bufIdx];

                while ((pHeader->dwFlags & WinMM.WHDR_DONE) == 0 && audioRunning)
                {
                    Kernel32.WaitForSingleObject(hAudioEvent, 5);
                }

                if (!audioRunning) break;

                for (int s = 0; s < SAMPLE_COUNT; s++)
                {
                    sampleBuffer[s] = GenerateSample();
                }

                lock (recordLock)
                {
                    recordedSamples.AddRange(sampleBuffer);
                }

                Marshal.Copy(sampleBuffer, 0, pHeader->lpData, SAMPLE_COUNT);
                WinMM.waveOutWrite(hWaveOut, pWaveHeaders[bufIdx], headerSize);

                bufIdx = (bufIdx + 1) % BUFFER_COUNT;
            }
        }

        // ── Hardware Namco WSG Sample Generator (48 kHz output) ──────────────────
        static short GenerateSample()
        {
            short sample = 0;
            sample = GenerateTone(0x00, 0x10, 0x15, 0x05, 5); // Voice 1
            sample += GenerateTone(0x06, 0x16, 0x1A, 0x0A, 4);     // Voice 2
            sample += GenerateTone(0x0B, 0x1B, 0x1F, 0x0F, 4);     // Voice 3

            sample = GenerateTone(0x00, 0x10, 0x15, 0x05, 5);
            sample += GenerateTone(0x06, 0x16, 0x1A, 0x0A, 4);
            sample += GenerateTone(0x0B, 0x1B, 0x1F, 0x0F, 4);

            return (short)(sample / 2);
        }

        static short GenerateTone(int countOffset, int freqOffset, int volOffset, int waveOffset, int bytes)
        {
            int freq = 0;
            int freqCounter = 0;

            for (int i = bytes - 1; i >= 0; i--)
            {
                freq <<= 4;
                freqCounter <<= 4;
                freq |= (SoundRegs[freqOffset + i] & 0x0F);
                freqCounter |= (SoundRegs[countOffset + i] & 0x0F);
            }

            freqCounter += freq;
            byte waveForm = (byte)(SoundRegs[waveOffset] & 0x0F);

            int ix = (bytes == 5) ? ((freqCounter >> 15) & 0x1F) : ((freqCounter >> 11) & 0x1F);
            ix |= (waveForm << 5);

            byte romSample = SoundRom[ix & 0x1FF];

            for (int i = 0; i < bytes; i++)
            {
                SoundRegs[countOffset + i] = (byte)(freqCounter & 0x0F);
                freqCounter >>= 4;
            }

            byte volume = (byte)(SoundRegs[volOffset] & 0x0F);

            // High clarity synthesizer gain scaling
            return (short)((romSample - 8) * volume * 320);
        }

        // ── ROM Loading ──────────────────────────────────────────────────────────
        static void LoadRoms(string baseDir)
        {
            // Load Sound PROMs 82s126.1m and 82s126.3m
            byte[]? p1m = TryLoadRom(baseDir, "82s126.1m", "82s126.1m.h");
            byte[]? p3m = TryLoadRom(baseDir, "82s126.3m", "82s126.3m.h");

            if (p1m != null) Array.Copy(p1m, 0, SoundRom, 0x0000, Math.Min(256, p1m.Length));
            else Array.Copy(DefaultAudioRom1M, 0, SoundRom, 0x0000, DefaultAudioRom1M.Length);

            if (p3m != null) Array.Copy(p3m, 0, SoundRom, 0x0100, Math.Min(256, p3m.Length));

            // Load Program ROMs pacman.6e .. 6j into MainRom (0x0000 - 0x3FFF)
            byte[]? r6e = TryLoadRom(baseDir, "pacman.6e", "pacman.6e.h");
            byte[]? r6f = TryLoadRom(baseDir, "pacman.6f", "pacman.6f.h");
            byte[]? r6h = TryLoadRom(baseDir, "pacman.6h", "pacman.6h.h");
            byte[]? r6j = TryLoadRom(baseDir, "pacman.6j", "pacman.6j.h");

            if (r6e != null) Array.Copy(r6e, 0, MainRom, 0x0000, Math.Min(0x1000, r6e.Length));
            if (r6f != null) Array.Copy(r6f, 0, MainRom, 0x1000, Math.Min(0x1000, r6f.Length));
            if (r6h != null) Array.Copy(r6h, 0, MainRom, 0x2000, Math.Min(0x1000, r6h.Length));
            if (r6j != null) Array.Copy(r6j, 0, MainRom, 0x3000, Math.Min(0x1000, r6j.Length));
        }

        static byte[]? TryLoadRom(string baseDir, string binFile, string headerFile)
        {
            string binPath = Path.Combine(baseDir, "rom", binFile);
            if (File.Exists(binPath)) return File.ReadAllBytes(binPath);

            string hdrPath = Path.Combine(baseDir, "include", "roms", headerFile);
            if (File.Exists(hdrPath))
            {
                string content = File.ReadAllText(hdrPath);
                int bStart = content.IndexOf('{');
                int bEnd = content.IndexOf('}');
                if (bStart >= 0 && bEnd > bStart)
                {
                    string body = content.Substring(bStart + 1, bEnd - bStart - 1);
                    var matches = Regex.Matches(body, @"0x([0-9A-Fa-f]{2})");
                    byte[] result = new byte[matches.Count];
                    for (int i = 0; i < matches.Count; i++)
                        result[i] = Convert.ToByte(matches[i].Groups[1].Value, 16);
                    return result;
                }
            }
            return null;
        }

        // ── Authentic Pac-Man Sound Processing Engine ──────────────────────────────
        static void ClearAllSounds()
        {
            Array.Clear(SoundRegs, 0, SoundRegs.Length);
            Array.Clear(Ch1Freq, 0, Ch1Freq.Length);
            Array.Clear(Ch2Freq, 0, Ch2Freq.Length);
            Array.Clear(Ch3Freq, 0, Ch3Freq.Length);

            Ch1Vol = Ch2Vol = Ch3Vol = 0;
            ClearEffect(Ch1SoundEffect);
            ClearEffect(Ch2SoundEffect);
            ClearEffect(Ch3SoundEffect);
            ClearEffect(Ch1SoundWave);
            ClearEffect(Ch2SoundWave);
            ClearEffect(Ch3SoundWave);
        }

        static void ClearEffect(SoundEffect eff)
        {
            eff.mask = eff.current = eff.selected = eff.frequencyInitial = eff.frequencyDelta = 0;
            eff.repeat = eff.volumeInitial = eff.volumeDelta = eff.type = eff.duration = 0;
            eff.dir = eff.frequency = eff.volume = 0;
            eff.offset = 0;
        }

        static void SoundEffectsAllChannels()
        {
            Ch1Vol = SoundEffectOneChannel(Ch1SoundEffect, Ch1Freq, 0x3B30, 1);
            Ch2Vol = SoundEffectOneChannel(Ch2SoundEffect, Ch2Freq, 0x3B40, 2);
            Ch3Vol = SoundEffectOneChannel(Ch3SoundEffect, Ch3Freq, 0x3B80, 3);
            Ch1Freq[4] = 0;
        }

        static void PlaySongsAllChannels()
        {
            byte v1 = PlaySongOneChannel(Ch1SoundWave, Ch1Freq, 0x3BC8);
            if (Ch1SoundWave.mask != 0) Ch1Vol = v1;

            byte v2 = PlaySongOneChannel(Ch2SoundWave, Ch2Freq, 0x3BCC);
            if (Ch2SoundWave.mask != 0) Ch2Vol = v2;

            byte v3 = PlaySongOneChannel(Ch3SoundWave, Ch3Freq, 0x3BD0);
            if (Ch3SoundWave.mask != 0) Ch3Vol = v3;
        }

        static byte SoundEffectOneChannel(SoundEffect eff, byte[] freq, ushort tableAddr, int chan)
        {
            if (eff.mask != 0)
                return SoundEffectProcess(eff, freq, tableAddr, chan);
            else
                return SoundEffectClear(eff, freq);
        }

        static byte SoundEffectClear(SoundEffect eff, byte[] freq)
        {
            if (eff.current == 0) return 0;
            eff.current = eff.dir = eff.frequency = eff.volume = 0;
            Array.Clear(freq, 0, freq.Length);
            return 0;
        }

        static byte SoundEffectProcess(SoundEffect eff, byte[] freq, ushort tableAddr, int chan)
        {
            byte mask = 0x80;
            int bit;
            for (bit = 8; bit > 0; bit--)
            {
                if ((eff.mask & mask) != 0) break;
                mask >>= 1;
            }

            if (bit == 0) return 0;

            if ((eff.current & mask) == 0)
            {
                eff.current = mask;
                ushort entryAddr = (ushort)(tableAddr + (bit - 1) * 8);

                eff.selected = MainRom[entryAddr + 0];
                eff.frequencyInitial = MainRom[entryAddr + 1];
                eff.frequencyDelta = MainRom[entryAddr + 2];
                eff.offset = (ushort)(MainRom[entryAddr + 3] | (MainRom[entryAddr + 4] << 8));
                eff.repeat = MainRom[entryAddr + 5];
                eff.volumeInitial = MainRom[entryAddr + 6];
                eff.volumeDelta = MainRom[entryAddr + 7];

                eff.duration = (byte)(eff.offset & 0x7F);
                eff.frequency = eff.frequencyInitial;
                eff.type = (byte)(eff.volumeInitial >> 4);

                if ((eff.type & 8) == 0)
                {
                    eff.volume = eff.volumeInitial;
                    eff.dir = 0;
                }
            }

            if (--eff.duration != 0)
                goto jump_ecd;

            if (eff.repeat != 0 && --eff.repeat == 0)
            {
                eff.mask = (byte)(eff.mask & ~mask);
                return SoundEffectOneChannel(eff, freq, tableAddr, chan);
            }
            else
            {
                eff.duration = (byte)(eff.offset & 0x7F);
                if ((eff.offset & 0x80) != 0)
                {
                    eff.frequencyDelta = (byte)(-((sbyte)eff.frequencyDelta));
                    eff.dir ^= 1;
                    if ((eff.dir & 1) != 0) goto jump_ecd;
                }

                eff.frequencyInitial = (byte)(eff.frequencyInitial + (eff.offset >> 8));
                eff.frequency = eff.frequencyInitial;
                eff.volumeInitial = (byte)(eff.volumeInitial + eff.volumeDelta);
                if ((eff.type & 8) == 0) eff.volume = eff.volumeInitial;
            }

        jump_ecd:
            eff.frequency = (byte)(eff.frequency + eff.frequencyDelta);

            if ((eff.selected & 0x70) == 0)
                return FrequencyWithVolume(eff, freq, eff.frequency);
            else
                return FrequencyScaledWithVolume(eff, freq, eff.frequency, (byte)((eff.selected & 0x70) >> 4));
        }

        static byte PlaySongOneChannel(SoundEffect eff, byte[] freq, ushort tableAddr)
        {
            if (eff.mask == 0) return SoundEffectClear(eff, freq);

            byte mask = 0x80;
            int bit;
            for (bit = 8; bit > 0; bit--)
            {
                if ((eff.mask & mask) != 0) break;
                mask >>= 1;
            }

            if (bit == 0) return 0;

            ushort addr;
            if ((eff.current & mask) == 0)
            {
                eff.current = mask;
                bit--;
                addr = (ushort)(MainRom[tableAddr + bit * 2] | (MainRom[tableAddr + bit * 2 + 1] << 8));
            }
            else
            {
                if (--eff.duration != 0) goto jump_d77;
                addr = eff.offset;
            }

        jump_d6c:
            byte a = MainRom[addr++];
            eff.offset = addr;

            if (a >= 0xF0)
            {
                a &= 0x0F;
                ExecuteSongCommand(a, eff, freq);
                goto jump_d6c;
            }

            if ((a & 0x1F) != 0) eff.dir = a;

            byte c = eff.volumeInitial;
            if ((eff.type & 8) != 0) c = 0;
            eff.volume = c;

            byte durIdx = (byte)((a >> 5) & 7);
            eff.duration = MainRom[0x3BB0 + durIdx] != 0 ? MainRom[0x3BB0 + durIdx] : DefaultPowerOf2Table[durIdx];

            if ((a & 0x1F) != 0)
            {
                byte freqIdx = (byte)(a & 0x0F);
                eff.frequency = MainRom[0x3BB8 + freqIdx] != 0 ? MainRom[0x3BB8 + freqIdx] : DefaultFreqTable[freqIdx];
            }

        jump_d77:
            byte dirCheck = (byte)(eff.dir & 0x10);
            byte scale = (byte)((dirCheck != 0 ? 1 : 0) + eff.frequencyInitial);

            if (scale == 0)
                return FrequencyWithVolume(eff, freq, eff.frequency);
            else
                return FrequencyScaledWithVolume(eff, freq, eff.frequency, scale);
        }

        static void ExecuteSongCommand(byte cmd, SoundEffect eff, byte[] freq)
        {
            switch (cmd)
            {
                case 0: eff.offset = (ushort)(MainRom[eff.offset] | (MainRom[eff.offset + 1] << 8)); break;
                case 1: eff.selected = MainRom[eff.offset++]; break;
                case 2: eff.frequencyInitial = MainRom[eff.offset++]; break;
                case 3: eff.volumeInitial = MainRom[eff.offset++]; break;
                case 4: eff.type = MainRom[eff.offset++]; break;
                case 15: ClearEffect(eff); Array.Clear(freq, 0, freq.Length); break;
            }
        }

        static byte FrequencyScaledWithVolume(SoundEffect eff, byte[] freq, ushort freqVal, byte scale)
        {
            freqVal <<= scale;
            return FrequencyWithVolume(eff, freq, freqVal);
        }

        static byte FrequencyWithVolume(SoundEffect eff, byte[] freq, ushort freqVal)
        {
            freq[0] = (byte)(freqVal & 0xFF);
            freq[1] = (byte)((freqVal >> 4) & 0x0F);
            freq[2] = (byte)((freqVal >> 8) & 0xFF);
            freq[3] = (byte)((freqVal >> 12) & 0x0F);

            return ProcessVolumeDecay(eff);
        }

        static byte ProcessVolumeDecay(SoundEffect eff)
        {
            switch (eff.type)
            {
                case 0: return eff.volume;
                case 1: return VolumeDecrease(eff, eff.volume);
                case 2: return VolumeDecreaseConditional(eff, (soundCounter & 1) != 0);
                case 3: return VolumeDecreaseConditional(eff, (soundCounter & 3) != 0);
                case 4: return VolumeDecreaseConditional(eff, (soundCounter & 7) != 0);
                default: return 0;
            }
        }

        static byte VolumeDecreaseConditional(SoundEffect eff, bool condition)
        {
            if (condition) return eff.volume;
            return VolumeDecrease(eff, eff.volume);
        }

        static byte VolumeDecrease(SoundEffect eff, byte volume)
        {
            volume &= 0x0F;
            if (volume == 0) return 0;
            eff.volume = --volume;
            return volume;
        }

        // ── Real-Time Formatted Sound Parameter Output (printf style) ───────────────
        static void PrintSoundParameters(int frameIndex, double timeSeconds)
        {
            // Voice 1 (20-bit frequency)
            uint f1 = (uint)(SoundRegs[0x10] | (SoundRegs[0x11] << 4) | (SoundRegs[0x12] << 8) | (SoundRegs[0x13] << 12) | ((SoundRegs[0x14] & 0x0F) << 16));
            byte w1 = (byte)(SoundRegs[0x05] & 0x0F);
            byte v1 = (byte)(SoundRegs[0x15] & 0x0F);
            double hz1 = f1 * 96000.0 / 1048576.0;

            // Voice 2 (16-bit frequency)
            uint f2 = (uint)(SoundRegs[0x16] | (SoundRegs[0x17] << 4) | (SoundRegs[0x18] << 8) | ((SoundRegs[0x19] & 0x0F) << 12));
            byte w2 = (byte)(SoundRegs[0x0A] & 0x0F);
            byte v2 = (byte)(SoundRegs[0x1A] & 0x0F);
            double hz2 = f2 * 96000.0 / 65536.0;

            // Voice 3 (16-bit frequency)
            uint f3 = (uint)(SoundRegs[0x1B] | (SoundRegs[0x1C] << 4) | (SoundRegs[0x1D] << 8) | ((SoundRegs[0x1E] & 0x0F) << 12));
            byte w3 = (byte)(SoundRegs[0x0F] & 0x0F);
            byte v3 = (byte)(SoundRegs[0x1F] & 0x0F);
            double hz3 = f3 * 96000.0 / 65536.0;

            // Print formatted parameters (printf equivalent) using InvariantCulture for consistent dot decimal format
            Console.WriteLine(string.Format(System.Globalization.CultureInfo.InvariantCulture,
                "[Frame {0:D4} | {1,5:F2}s] V1: Wave={2} Vol={3,2} Freq=0x{4:X5} ({5,6:F1} Hz) | V2: Wave={6} Vol={7,2} Freq=0x{8:X4} ({9,6:F1} Hz) | V3: Wave={10} Vol={11,2} Freq=0x{12:X4} ({13,6:F1} Hz)",
                frameIndex, timeSeconds,
                w1, v1, f1, hz1,
                w2, v2, f2, hz2,
                w3, v3, f3, hz3));
        }

        // ── 1/60th Second Frame Rate Execution Loop ─────────────────────────────
        static void PlaySoundItem(SoundItem item, string baseDir)
        {
            ClearAllSounds();
            lock (recordLock)
            {
                recordedSamples.Clear();
            }

            Console.WriteLine("\n========================================================================================================");
            Console.WriteLine($" PLAYING [{item.Id,2}]: {item.Name} ({item.Category})");
            Console.WriteLine(" Frame Rate: 60.0 Hz (16.66 ms per tick) | Hardware Synthesizer: WinMM waveOut (48 kHz)");
            Console.WriteLine($" Duration: {item.DurationFrames} frames ({item.DurationFrames / 60.0:F2} seconds)");
            Console.WriteLine("========================================================================================================");

            // Run 60 Hz frame rate loop
            for (int frame = 0; frame < item.DurationFrames; frame++)
            {
                soundCounter++;

                // Re-assert sound/song triggers every VBlank frame tick (emulating arcade ISR hardware behavior)
                if (item.IsSong)
                {
                    Ch1SoundWave.mask = 1;
                    Ch2SoundWave.mask = 1;
                    Ch3SoundWave.mask = 1;
                }
                else
                {
                    if (item.ChannelIndex == 1) Ch1SoundEffect.mask = item.MaskValue;
                    else if (item.ChannelIndex == 2) Ch2SoundEffect.mask = item.MaskValue;
                    else if (item.ChannelIndex == 3) Ch3SoundEffect.mask = item.MaskValue;
                }

                // 1. Process authentic sound engine subroutines
                SoundEffectsAllChannels();
                PlaySongsAllChannels();

                // 2. Map RAM frequency/volume output to hardware SoundRegs
                Array.Copy(Ch1Freq, 0, SoundRegs, 0x10, 5);
                SoundRegs[0x15] = Ch1Vol;
                Array.Copy(Ch2Freq, 0, SoundRegs, 0x16, 4);
                SoundRegs[0x1A] = Ch2Vol;
                Array.Copy(Ch3Freq, 0, SoundRegs, 0x1B, 4);
                SoundRegs[0x1F] = Ch3Vol;

                byte w1 = (Ch1SoundWave.mask != 0) ? Ch1SoundWave.selected : Ch1SoundEffect.selected;
                byte w2 = (Ch2SoundWave.mask != 0) ? Ch2SoundWave.selected : Ch2SoundEffect.selected;
                byte w3 = (Ch3SoundWave.mask != 0) ? Ch3SoundWave.selected : Ch3SoundEffect.selected;

                SoundRegs[0x05] = w1;
                SoundRegs[0x0A] = w2;
                SoundRegs[0x0F] = w3;

                // 3. Output current sound parameters for each frame tick
                PrintSoundParameters(frame, frame / 60.0);

                // 4. Maintain 1/60 second frame timing (16.66 ms)
                Thread.Sleep(16);
            }

            ClearAllSounds();

            // Export WAV audio file to output_audio directory
            string wavPath = Path.Combine(baseDir, "output_audio", item.FileName);
            short[] samplesCopy;
            lock (recordLock)
            {
                samplesCopy = recordedSamples.ToArray();
            }

            if (samplesCopy.Length > 0)
            {
                WavWriter.SaveWavFile(wavPath, samplesCopy, AUDIO_FREQUENCY);
                Console.WriteLine($" -> Saved audio WAV file: {wavPath} ({samplesCopy.Length} samples)");
            }

            Console.WriteLine($"--- Finished: {item.Name} ---\n");
        }

        static void PrintMenu()
        {
            Console.WriteLine("\n========================================================================================================");
            Console.WriteLine("                       Pac-Man Authentic Sound & Music Player (C#)");
            Console.WriteLine("                       Synthesizer Engine: WinMM waveOut (48kHz)");
            Console.WriteLine("                       WAV Audio Export Directory: ./output_audio/");
            Console.WriteLine("========================================================================================================");
            Console.WriteLine("   [1] Game Start Intro Theme (3-Channel Song)");
            Console.WriteLine("   [2] Intermission / Coffee Break Theme\n");

            Console.WriteLine("   --- Channel 1 Sound Effects ---");
            Console.WriteLine("   [3] Waka-Waka / Eating Dot");
            Console.WriteLine("   [4] Siren 1 (Slow Background Siren)\n");

            Console.WriteLine("   --- Channel 2 Sound Effects ---");
            Console.WriteLine("   [5] Siren 1 (Ch2 Siren)");
            Console.WriteLine("   [6] Siren 2 (Medium Siren)");
            Console.WriteLine("   [7] Siren 3 (Fast Siren)");
            Console.WriteLine("   [8] Siren 4 (Faster Siren)");
            Console.WriteLine("   [9] Siren 5 (Cruise Elroy / Fastest Siren)");
            Console.WriteLine("  [10] Frightened Ghosts Siren (Blue Ghost Wawa)");
            Console.WriteLine("  [11] Eyes Returning to Ghost House");
            Console.WriteLine("  [12] Extra Life / Bonus Sound\n");

            Console.WriteLine("   --- Channel 3 Sound Effects ---");
            Console.WriteLine("  [13] Pac-Man Death / Dying Crumple Animation");
            Console.WriteLine("  [14] Ghost Eaten (200 / 400 / 800 / 1600 Pts)");
            Console.WriteLine("  [15] Bonus Fruit Eaten (100-5000 Pts)");
            Console.WriteLine("  [16] Credit Inserted (Coin Sound)\n");

            Console.WriteLine("  [17] PLAY & SAVE ALL SOUNDS & SONGS SEQUENTIALLY (Jukebox Mode)");
            Console.WriteLine("   [0] Quit Program");
            Console.WriteLine("========================================================================================================");
            Console.Write("Select a sound option (0-17): ");
        }

        static void Main(string[] args)
        {
            Console.WriteLine("==================================================================");
            Console.WriteLine("          Pac-Man Sound Effects & Songs Player (C#)");
            Console.WriteLine("==================================================================");

            string baseDir = Directory.GetCurrentDirectory();
            if (!Directory.Exists(Path.Combine(baseDir, "rom")) && !Directory.Exists(Path.Combine(baseDir, "include")))
            {
                string parentDir = Path.GetFullPath(Path.Combine(baseDir, ".."));
                if (Directory.Exists(Path.Combine(parentDir, "rom")) || Directory.Exists(Path.Combine(parentDir, "include")))
                    baseDir = parentDir;
            }

            Console.WriteLine($"Project Root Directory: {baseDir}");
            LoadRoms(baseDir);

            InitAudioDriver();
            Console.WriteLine("Audio hardware driver initialized (winmm waveOut @ 48,000 Hz active).");

            // Non-interactive automated execution mode
            if (args.Length > 0)
            {
                string arg = args[0].Trim().ToLower();
                if (arg == "17" || arg == "all" || arg == "jukebox")
                {
                    Console.WriteLine("\nRunning Jukebox Mode: Playing all sound effects and songs sequentially...\n");
                    foreach (var item in SoundCatalog)
                    {
                        PlaySoundItem(item, baseDir);
                        Thread.Sleep(300);
                    }
                }
                else if (int.TryParse(arg, out int choice) && choice >= 1 && choice <= SoundCatalog.Length)
                {
                    PlaySoundItem(SoundCatalog[choice - 1], baseDir);
                }

                CloseAudioDriver();
                return;
            }

            // Interactive Console Menu Loop
            while (true)
            {
                PrintMenu();
                string? line = Console.ReadLine();
                if (line == null) break;

                line = line.Trim();
                if (line == "0" || line.Equals("q", StringComparison.OrdinalIgnoreCase) || line.Equals("quit", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Exiting sound player. Goodbye!");
                    break;
                }

                if (int.TryParse(line, out int choice))
                {
                    if (choice == 17)
                    {
                        Console.WriteLine($"\n--- Starting Jukebox Mode: Playing all {SoundCatalog.Length} sounds & songs ---");
                        foreach (var item in SoundCatalog)
                        {
                            PlaySoundItem(item, baseDir);
                            Thread.Sleep(300);
                        }
                    }
                    else if (choice >= 1 && choice <= SoundCatalog.Length)
                    {
                        PlaySoundItem(SoundCatalog[choice - 1], baseDir);
                    }
                    else
                    {
                        Console.WriteLine("Invalid choice. Please enter a number between 0 and 17.");
                    }
                }
                else
                {
                    Console.WriteLine("Invalid input. Please enter a number.");
                }
            }

            CloseAudioDriver();
        }
    }
}
