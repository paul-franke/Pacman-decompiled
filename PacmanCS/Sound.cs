using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace PacmanCS
{
    public static unsafe class Sound
    {
        private const string WinMM = "winmm.dll";

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
            public IntPtr lpNext;
            public IntPtr reserved;
        }

        public const uint WHDR_DONE = 0x00000001;
        public const ushort WAVE_FORMAT_PCM = 1;
        public const uint WAVE_MAPPER = unchecked((uint)-1);
        public const uint CALLBACK_EVENT = 0x00050000;
        public const uint MMSYSERR_NOERROR = 0;

        [DllImport(WinMM, CallingConvention = CallingConvention.StdCall)]
        public static extern uint waveOutOpen(out IntPtr phwo, uint uDeviceID, ref WAVEFORMATEX pwfx, IntPtr dwCallback, IntPtr dwInstance, uint fdwOpen);

        [DllImport(WinMM, CallingConvention = CallingConvention.StdCall)]
        public static extern uint waveOutPrepareHeader(IntPtr hwo, ref WAVEHDR pwh, uint cbwh);

        [DllImport(WinMM, CallingConvention = CallingConvention.StdCall)]
        public static extern uint waveOutWrite(IntPtr hwo, ref WAVEHDR pwh, uint cbwh);

        [DllImport(WinMM, CallingConvention = CallingConvention.StdCall)]
        public static extern uint waveOutUnprepareHeader(IntPtr hwo, ref WAVEHDR pwh, uint cbwh);

        [DllImport(WinMM, CallingConvention = CallingConvention.StdCall)]
        public static extern uint waveOutReset(IntPtr hwo);

        [DllImport(WinMM, CallingConvention = CallingConvention.StdCall)]
        public static extern uint waveOutClose(IntPtr hwo);

        public static readonly byte[] soundRomArray = new byte[0x200];
        public static byte* soundRom;
        private static GCHandle soundRomHandle;

        public const int AUDIO_FREQUENCY = 96000;
        public const int SAMPLE_COUNT = AUDIO_FREQUENCY / 100;

        static Sound()
        {
            soundRomHandle = GCHandle.Alloc(soundRomArray, GCHandleType.Pinned);
            soundRom = (byte*)soundRomHandle.AddrOfPinnedObject();
        }

        private static short generateTone(byte* freqCountPtr, byte* freqPtr, byte volume, byte waveForm, int bytes)
        {
            short sample;
            int freq = 0;
            int freqCounter = 0;

            for (int i = bytes - 1; i >= 0; i--)
            {
                freq <<= 4;
                freqCounter <<= 4;

                freq |= (freqPtr[i] & 0xf);
                freqCounter |= (freqCountPtr[i] & 0xf);
            }

            freqCounter += freq;
            waveForm &= 0xf;

            int ix;
            if (bytes == 5)
                ix = (freqCounter >> 15) & 0x1f;
            else
                ix = (freqCounter >> 11) & 0x1f;

            ix |= (waveForm << 5);
            sample = soundRom[ix];

            for (int i = 0; i < bytes; i++)
            {
                freqCountPtr[i] = (byte)(freqCounter & 0xf);
                freqCounter >>= 4;
            }

            sample = (short)((sample - 8) * volume * 68);
            return sample;
        }

        private const int TEST_SMP = 1600;
        private const int TEST_AMP = 32;
        private static int testCounter = 0;

        private static short testWave()
        {
            double angle = (2 * Math.PI * testCounter++) / TEST_SMP;
            testCounter %= TEST_SMP;
            return (short)(TEST_AMP * Math.Sin(angle));
        }

        private static short generateSample()
        {
            short sample = 0;
            SOUNDREGS* sound = (SOUNDREGS*)MemMap.SOUND;

            sample = generateTone(sound->v1FreqCount, sound->v1Frequency, sound->v1Volume, sound->v1WaveForm, 5);
            sample += generateTone(sound->v2FreqCount, sound->v2Frequency, sound->v2Volume, sound->v2WaveForm, 4);
            sample += generateTone(sound->v3FreqCount, sound->v3Frequency, sound->v3Volume, sound->v3WaveForm, 4);
            sample += testWave();

            return sample;
        }

        private const int BUFFER_COUNT = 4;
        private static IntPtr hWaveOut = IntPtr.Zero;
        private static WAVEHDR[] waveHeaders = new WAVEHDR[BUFFER_COUNT];
        private static IntPtr[] waveBuffers = new IntPtr[BUFFER_COUNT];
        private static int currentBufferIndex = 0;
        private static AutoResetEvent audioEvent;
        private static Thread audioThread;
        private static bool audioThreadRunning = false;

        private static void winAudioThread()
        {
            while (audioThreadRunning)
            {
                fixed (WAVEHDR* hdr = &waveHeaders[currentBufferIndex])
                {
                    while ((hdr->dwFlags & WHDR_DONE) == 0 && audioThreadRunning)
                    {
                        audioEvent.WaitOne(10);
                    }

                    if (!audioThreadRunning) break;

                    short* buf = (short*)waveBuffers[currentBufferIndex];
                    for (int i = 0; i < SAMPLE_COUNT; i++)
                    {
                        buf[i] = generateSample();
                    }

                    waveOutUnprepareHeader(hWaveOut, ref waveHeaders[currentBufferIndex], (uint)sizeof(WAVEHDR));
                    waveOutPrepareHeader(hWaveOut, ref waveHeaders[currentBufferIndex], (uint)sizeof(WAVEHDR));
                    waveOutWrite(hWaveOut, ref waveHeaders[currentBufferIndex], (uint)sizeof(WAVEHDR));

                    currentBufferIndex = (currentBufferIndex + 1) % BUFFER_COUNT;
                }
            }
        }

        public static void soundInit()
        {
            Array.Copy(Roms.rom_82s126_1m, 0, soundRomArray, 0x0000, 0x100);
            Array.Copy(Roms.rom_82s126_3m, 0, soundRomArray, 0x0100, 0x100);

            WAVEFORMATEX wfx = new WAVEFORMATEX();
            wfx.wFormatTag = WAVE_FORMAT_PCM;
            wfx.nChannels = 1;
            wfx.nSamplesPerSec = AUDIO_FREQUENCY;
            wfx.wBitsPerSample = 16;
            wfx.nBlockAlign = 2;
            wfx.nAvgBytesPerSec = wfx.nSamplesPerSec * wfx.nBlockAlign;
            wfx.cbSize = 0;

            audioEvent = new AutoResetEvent(false);
            if (waveOutOpen(out hWaveOut, WAVE_MAPPER, ref wfx, audioEvent.SafeWaitHandle.DangerousGetHandle(), IntPtr.Zero, CALLBACK_EVENT) != MMSYSERR_NOERROR)
            {
                Console.Error.WriteLine("waveOutOpen failed");
                return;
            }

            for (int i = 0; i < BUFFER_COUNT; i++)
            {
                waveBuffers[i] = Marshal.AllocHGlobal(SAMPLE_COUNT * sizeof(short));
                waveHeaders[i] = new WAVEHDR();
                waveHeaders[i].lpData = waveBuffers[i];
                waveHeaders[i].dwBufferLength = (uint)(SAMPLE_COUNT * sizeof(short));
                waveOutPrepareHeader(hWaveOut, ref waveHeaders[i], (uint)sizeof(WAVEHDR));
                waveHeaders[i].dwFlags |= WHDR_DONE;
            }

            audioThreadRunning = true;
            audioThread = new Thread(winAudioThread) { IsBackground = true };
            audioThread.Start();
        }

        public static void soundClose()
        {
            if (audioThreadRunning)
            {
                audioThreadRunning = false;
                audioEvent?.Set();
                if (audioThread != null && audioThread.IsAlive)
                {
                    audioThread.Join();
                }
                if (hWaveOut != IntPtr.Zero)
                {
                    waveOutReset(hWaveOut);
                    for (int i = 0; i < BUFFER_COUNT; i++)
                    {
                        if (waveBuffers[i] != IntPtr.Zero)
                        {
                            waveOutUnprepareHeader(hWaveOut, ref waveHeaders[i], (uint)sizeof(WAVEHDR));
                            Marshal.FreeHGlobal(waveBuffers[i]);
                        }
                    }
                    waveOutClose(hWaveOut);
                }
                audioEvent?.Dispose();
            }
        }
    }
}
