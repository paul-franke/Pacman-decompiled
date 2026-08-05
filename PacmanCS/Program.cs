using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace PacmanCS
{
    public static unsafe class Program
    {
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
        private static extern bool SetDllDirectory(string lpPathName);

        public static void cpuThreadFunc()
        {
            Pacman.reset_0000();
        }

        public static int Main(string[] args)
        {
            try
            {
                SetDllDirectory("libs");
            }
            catch { }

            // Copy ROM data into charset and ROM memory arrays
            Marshal.Copy(Roms.rom_pacman_5e, 0, (IntPtr)(MemMap.charset + 0x0000), 0x1000);
            Marshal.Copy(Roms.rom_pacman_5f, 0, (IntPtr)(MemMap.charset + 0x1000), 0x1000);

            Marshal.Copy(Roms.rom_pacman_6e, 0, (IntPtr)(MemMap.ROM + 0x0000), 0x1000);
            Marshal.Copy(Roms.rom_pacman_6f, 0, (IntPtr)(MemMap.ROM + 0x1000), 0x1000);
            Marshal.Copy(Roms.rom_pacman_6h, 0, (IntPtr)(MemMap.ROM + 0x2000), 0x1000);
            Marshal.Copy(Roms.rom_pacman_6j, 0, (IntPtr)(MemMap.ROM + 0x3000), 0x1000);

            // Input switches are active low
            MemMap.IO_INPUT0 = 0xef;
            MemMap.IO_INPUT1 = 0x6f;
            MemMap.IO_INPUT0 |= 0x10; // remove for rack advance
            MemMap.IO_INPUT1 |= 0x10; // remove for service mode
            MemMap.IO_INPUT1 |= 0x80; // upright mode, remove for cocktail mode

            // DIP_INPUT = 0xff; default, 5 lives, 2 coins per game, etc
            MemMap.DIP_INPUT = 0x49;
            MemMap.DIP_INPUT |= 0x80; // remove for alt names

            bool verbose = false;
            int arg_idx = 0;
            while (arg_idx < args.Length)
            {
                if (args[arg_idx] == "-f")
                {
                    if (arg_idx + 1 < args.Length)
                    {
                        if (int.TryParse(args[arg_idx + 1], out int val) && val >= 1 && val <= 60)
                        {
                            Cpu.target_fps = val;
                        }
                        else
                        {
                            Console.Error.WriteLine($"Error: -f FPS value must be between 1 and 60. Got: {args[arg_idx + 1]}");
                            return 1;
                        }
                        arg_idx += 2;
                    }
                    else
                    {
                        Console.Error.WriteLine("Error: -f flag requires a value.");
                        return 1;
                    }
                }
                else if (args[arg_idx] == "-v")
                {
                    verbose = true;
                    arg_idx++;
                }
                else
                {
                    if (byte.TryParse(args[arg_idx], out byte dipVal))
                    {
                        MemMap.DIP_INPUT = dipVal;
                    }
                    arg_idx++;
                }
            }

            Video.videoInit(224, 288, 3); // scale is 3 x 3
            Sound.soundInit();
            Kbd.keyboardInit();

            Thread cpuThread = new Thread(cpuThreadFunc) { IsBackground = true };
            cpuThread.Start();

            Video.videoStartGlutLoop();

            return 0;
        }
    }
}
