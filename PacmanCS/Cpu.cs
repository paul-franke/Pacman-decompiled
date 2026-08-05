using System;
using System.Diagnostics;
using System.Threading;

namespace PacmanCS
{
    public static class Cpu
    {
        public static Action intVector;
        public static bool cpuPaused;
        public static double target_fps = 60.0;

        public static void usleep(int usec) => Thread.Sleep(Math.Max(1, usec / 1000));
        public static void usleep(uint usec) => Thread.Sleep(Math.Max(1, (int)(usec / 1000)));

        public static void interruptEnable()
        {
        }

        public static void interruptDisable()
        {
        }

        public static void interruptMode(int mode)
        {
        }

        private static long frequency = Stopwatch.Frequency;
        private static long last_time = 0;
        private static bool first = true;

        private static void limit_frame_rate(double targetFps)
        {
            if (first)
            {
                last_time = Stopwatch.GetTimestamp();
                first = false;
                return;
            }

            double targetFrameTime = 1.0 / targetFps;
            while (true)
            {
                long currentTime = Stopwatch.GetTimestamp();
                double elapsed = (double)(currentTime - last_time) / frequency;
                double remaining = targetFrameTime - elapsed;
                if (remaining <= 0.0)
                {
                    break;
                }
                if (remaining > 0.001)
                {
                    Thread.Sleep((int)(remaining * 1000.0 - 0.5));
                }
                else
                {
                    Thread.SpinWait(10);
                }
            }
            last_time = Stopwatch.GetTimestamp();
        }

        private static bool inInterrupt;

        public static void interruptHalt()
        {
            limit_frame_rate(target_fps);

            if (!inInterrupt && !cpuPaused && intVector != null)
            {
                inInterrupt = true;
                intVector();
            }
            inInterrupt = false;
        }

        public static void interruptVector(Action func)
        {
            intVector = func;
        }

        public static void kickWatchdog()
        {
        }
    }
}
