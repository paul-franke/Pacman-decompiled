using System;
using System.Runtime.InteropServices;

namespace PacmanCS
{
    public static unsafe class Video
    {
        // ── FreeGLUT & OpenGL P/Invoke ──────────────────────────────────────
        private const string FreeGlutLib = "freeglut.dll";
        private const string OpenGL32Lib = "opengl32.dll";

        public const int GLUT_RGBA = 0x0000;
        public const int GLUT_DOUBLE = 0x0002;
        public const int GLUT_KEY_UP = 0x0065;
        public const int GLUT_KEY_DOWN = 0x0067;
        public const int GLUT_KEY_LEFT = 0x0064;
        public const int GLUT_KEY_RIGHT = 0x0066;

        public const int GL_UNSIGNED_BYTE = 0x1401;
        public const int GL_RGBA = 0x1908;
        public const int GL_PROJECTION = 0x1701;
        public const int GL_MODELVIEW = 0x1700;
        public const int GL_LINE_LOOP = 0x0002;

        public delegate void GlutDisplayCallback();
        public delegate void GlutIdleCallback();
        public delegate void GlutKeyboardCallback(byte key, int x, int y);
        public delegate void GlutSpecialCallback(int key, int x, int y);

        [DllImport(FreeGlutLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern void glutInit(ref int argc, [In, Out] string[] argv);

        [DllImport(FreeGlutLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern void glutInitWindowPosition(int x, int y);

        [DllImport(FreeGlutLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern void glutInitDisplayMode(uint mode);

        [DllImport(FreeGlutLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern void glutInitWindowSize(int width, int height);

        [DllImport(FreeGlutLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern int glutCreateWindow(string title);

        [DllImport(FreeGlutLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern void glutDisplayFunc(GlutDisplayCallback func);

        [DllImport(FreeGlutLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern void glutIdleFunc(GlutIdleCallback func);

        [DllImport(FreeGlutLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern void glutKeyboardFunc(GlutKeyboardCallback func);

        [DllImport(FreeGlutLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern void glutKeyboardUpFunc(GlutKeyboardCallback func);

        [DllImport(FreeGlutLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern void glutSpecialFunc(GlutSpecialCallback func);

        [DllImport(FreeGlutLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern void glutSpecialUpFunc(GlutSpecialCallback func);

        [DllImport(FreeGlutLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern void glutMainLoop();

        [DllImport(FreeGlutLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern void glutSwapBuffers();

        [DllImport(FreeGlutLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern void glutPostRedisplay();

        [DllImport(OpenGL32Lib, CallingConvention = CallingConvention.Cdecl)]
        public static extern void glViewport(int x, int y, int width, int height);

        [DllImport(OpenGL32Lib, CallingConvention = CallingConvention.Cdecl)]
        public static extern void glMatrixMode(uint mode);

        [DllImport(OpenGL32Lib, CallingConvention = CallingConvention.Cdecl)]
        public static extern void glLoadIdentity();

        [DllImport(OpenGL32Lib, CallingConvention = CallingConvention.Cdecl)]
        public static extern void glOrtho(double left, double right, double bottom, double top, double zNear, double zFar);

        [DllImport(OpenGL32Lib, CallingConvention = CallingConvention.Cdecl)]
        public static extern void glDrawPixels(int width, int height, uint format, uint type, void* pixels);

        [DllImport(OpenGL32Lib, CallingConvention = CallingConvention.Cdecl)]
        public static extern void glLineWidth(float width);

        [DllImport(OpenGL32Lib, CallingConvention = CallingConvention.Cdecl)]
        public static extern void glColor4f(float red, float green, float blue, float alpha);

        [DllImport(OpenGL32Lib, CallingConvention = CallingConvention.Cdecl)]
        public static extern void glBegin(uint mode);

        [DllImport(OpenGL32Lib, CallingConvention = CallingConvention.Cdecl)]
        public static extern void glVertex2iv(int* v);

        [DllImport(OpenGL32Lib, CallingConvention = CallingConvention.Cdecl)]
        public static extern void glEnd();

        [DllImport(OpenGL32Lib, CallingConvention = CallingConvention.Cdecl)]
        public static extern void glFlush();

        // ── Video State & Logic ─────────────────────────────────────────────
        private static pixel* frameBuffer;

        private static int screenXSize;
        private static int screenYSize;

        private static int frameBufferXSize;
        private static int frameBufferYSize;
        private static int frameBufferScale;

        public static bool redrawEnable = true;
        public static bool drawTargetEnable = false;

        // Delegates retained to prevent GC collection
        private static GlutDisplayCallback displayCb;
        private static GlutIdleCallback idleCb;
        private static GlutKeyboardCallback kbdDownCb;
        private static GlutKeyboardCallback kbdUpCb;
        private static GlutSpecialCallback specDownCb;
        private static GlutSpecialCallback specUpCb;

        private static void videoPlotRaw(int x, int y, pixel p)
        {
            y = frameBufferYSize - y - 1;
            frameBuffer[y * frameBufferXSize + x] = p;
        }

        public static pixel videoColourLookup(byte col)
        {
            byte c1 = Roms.rom_82s126_4a[col & 0x7f];
            byte c2 = Roms.rom_82s123_7f[c1 & 0xf];

            pixel p;
            p.r = (byte)((c2 & 7) * 36);
            p.g = (byte)(((c2 >> 3) & 7) * 36);
            p.b = (byte)((c2 >> 6) * 85);
            p.unused = 0;
            return p;
        }

        public static void videoPlot(uint x, uint y, pixel p)
        {
            for (int i = 0; i < frameBufferScale; i++)
                for (int j = 0; j < frameBufferScale; j++)
                    videoPlotRaw((int)(x * frameBufferScale + i), (int)(y * frameBufferScale + j), p);
        }

        public static void videoDrawChar(uint cx, uint cy, int chr, int chrCol)
        {
            for (uint y = 0; y < 8; y++)
            {
                for (uint x = 0; x < 8; x++)
                {
                    int z = chr * 16;
                    z += (int)((4 - (y & 4)) << 1);
                    z += (int)(7 - x);
                    byte pixelData = MemMap.charset[z];
                    byte col = (byte)(chrCol << 2);
                    col |= (byte)((pixelData & (0x08 >> (int)(y & 3))) != 0 ? 0x01 : 0);
                    col |= (byte)((pixelData & (0x80 >> (int)(y & 3))) != 0 ? 0x02 : 0);

                    pixel p = videoColourLookup(col);
                    videoPlot((uint)((cx << 3) + x), (uint)((cy << 3) + y), p);
                }
            }
        }

        public static void videoDrawSprite(uint px, uint py, int shape, int mode, int colour)
        {
            px = (uint)(256 - px);
            if (px < 0x12)
                return;

            py = (uint)(screenYSize - py);
            px -= 0x12;
            py -= 0xf;

            for (uint y = 0; y < 16; y++)
            {
                for (uint x = 0; x < 16; x++)
                {
                    int z = shape * 64;
                    z += (int)(((y + 4) & 0xc) << 1);
                    z += (int)(7 - (x & 7));
                    if ((x & 8) == 0)
                        z += 32;
                    byte pixelData = MemMap.charset[z + 0x1000];
                    byte col = (byte)(colour << 2);
                    col |= (byte)((pixelData & (0x08 >> (int)(y & 3))) != 0 ? 0x01 : 0);
                    col |= (byte)((pixelData & (0x80 >> (int)(y & 3))) != 0 ? 0x02 : 0);
                    pixel p = videoColourLookup(col);

                    if (p.r > 0 || p.g > 0 || p.b > 0)
                    {
                        int dx = (int)x;
                        int dy = (int)y;

                        if ((mode & 2) != 0)
                            dx = 15 - dx;
                        if ((mode & 1) != 0)
                            dy = 15 - dy;

                        videoPlot((uint)(px + dx), (uint)(py + dy), p);
                    }
                }
            }
        }

        private struct Target
        {
            public fixed int vertex[4];
            public int col;
        }

        private static Target* targets;

        static Video()
        {
            targets = (Target*)Marshal.AllocHGlobal(sizeof(Target) * 5);
        }

        public static void showTarget(XYPOS a, XYPOS b, int ghost)
        {
            targets[ghost - 1].vertex[0] = (476 - a.x * 8) * frameBufferScale;
            targets[ghost - 1].vertex[1] = (524 - a.y * 8) * frameBufferScale;
            targets[ghost - 1].vertex[2] = (476 - b.x * 8) * frameBufferScale;
            targets[ghost - 1].vertex[3] = (524 - b.y * 8) * frameBufferScale;
            targets[ghost - 1].col = ((ghost * 2 - 1) << 2) | 3;
        }

        private static void videoRedraw()
        {
            for (int y = 0; y < 36; y++)
            {
                int pos;
                int inc;
                switch (y)
                {
                    case 0: pos = 0x3dd; inc = -1; break;
                    case 1: pos = 0x3fd; inc = -1; break;
                    case 34: pos = 0x01d; inc = -1; break;
                    case 35: pos = 0x03d; inc = -1; break;
                    default: pos = 0x3a0 + y - 2; inc = -32; break;
                }
                for (int x = 0; x < 28; x++)
                {
                    videoDrawChar((uint)x, (uint)y, MemMap.SCREEN[pos], MemMap.COLOUR[pos]);
                    pos += inc;
                }
            }

            for (int sprite = 7; sprite >= 0; sprite--)
            {
                videoDrawSprite(MemMap.SPRITECOORDS[sprite * 2], MemMap.SPRITECOORDS[sprite * 2 + 1],
                                 MemMap.SPRITEATTRIB[sprite * 2] >> 2, MemMap.SPRITEATTRIB[sprite * 2] & 3,
                                 MemMap.SPRITEATTRIB[sprite * 2 + 1]);
            }
        }

        private static void drawTargets()
        {
            for (int i = 0; i < 5; i++)
            {
                glLineWidth(4);

                pixel p = videoColourLookup((byte)targets[i].col);
                glColor4f((float)(p.r / 511.0), (float)(p.g / 511.0), (float)(p.b / 511.0), 0.1f);

                glBegin(GL_LINE_LOOP);
                glVertex2iv(targets[i].vertex);
                glVertex2iv(targets[i].vertex + 2);
                glEnd();
            }
        }

        private static void displayFunc()
        {
            if (redrawEnable)
                videoRedraw();

            glDrawPixels(frameBufferXSize, frameBufferYSize, GL_RGBA, GL_UNSIGNED_BYTE, frameBuffer);

            if (drawTargetEnable)
                drawTargets();

            glFlush();
            glutSwapBuffers();
        }

        public static void videoInit(int xsize, int ysize, int scale)
        {
            screenXSize = xsize;
            screenYSize = ysize;

            frameBufferXSize = screenXSize * scale;
            frameBufferYSize = screenYSize * scale;
            frameBufferScale = scale;

            frameBuffer = (pixel*)Marshal.AllocHGlobal(frameBufferXSize * frameBufferYSize * sizeof(pixel));
            new Span<pixel>(frameBuffer, frameBufferXSize * frameBufferYSize).Clear();

            Console.WriteLine($"FB size is {frameBufferXSize} x {frameBufferYSize}");
        }

        public static void videoStartGlutLoop()
        {
            int argc = 1;
            string[] argv = new string[] { "PacmanCS" };
            glutInit(ref argc, argv);
            glutInitWindowPosition(10, 10);
            glutInitDisplayMode(GLUT_RGBA | GLUT_DOUBLE);

            glutInitWindowSize(frameBufferXSize, frameBufferYSize);
            glutCreateWindow("Pacman-c v0.7 (C#)");
            glViewport(0, 0, frameBufferXSize, frameBufferYSize);
            glMatrixMode(GL_PROJECTION);
            glLoadIdentity();

            glOrtho(0, frameBufferXSize, 0, frameBufferYSize, 1, -1);
            glMatrixMode(GL_MODELVIEW);
            glLoadIdentity();

            displayCb = displayFunc;
            idleCb = glutPostRedisplay;
            kbdDownCb = glutKeyboardDown;
            kbdUpCb = glutKeyboardUp;
            specDownCb = glutSpecialDown;
            specUpCb = glutSpecialUp;

            glutDisplayFunc(displayCb);
            glutIdleFunc(idleCb);

            glutKeyboardFunc(kbdDownCb);
            glutKeyboardUpFunc(kbdUpCb);
            glutSpecialFunc(specDownCb);
            glutSpecialUpFunc(specUpCb);

            glutMainLoop();
        }

        public static void glutKeyboardDown(byte key, int x, int y)
        {
            char c = (char)key;
            if (c == '5') MemMap.IO_INPUT0 &= 0xdf; // ~0x20
            else if (c == '1') MemMap.IO_INPUT1 &= 0xdf; // ~0x20
            else if (c == '2') MemMap.IO_INPUT1 &= 0xbf; // ~0x40
            else if (c == 'p' || c == 'P') Cpu.cpuPaused = !Cpu.cpuPaused;
            else if (c == 'd' || c == 'D') drawTargetEnable = !drawTargetEnable;
            else if (c == '8') MemMap.IO_INPUT0 &= 0xfe; // ~0x01
            else if (c == '4') MemMap.IO_INPUT0 &= 0xfd; // ~0x02
            else if (c == '6') MemMap.IO_INPUT0 &= 0xfb; // ~0x04
            else if (c == '2') MemMap.IO_INPUT0 &= 0xf7; // ~0x08
        }

        public static void glutKeyboardUp(byte key, int x, int y)
        {
            char c = (char)key;
            if (c == '5') MemMap.IO_INPUT0 |= 0x20;
            else if (c == '1') MemMap.IO_INPUT1 |= 0x20;
            else if (c == '2') MemMap.IO_INPUT1 |= 0x40;
            else if (c == '8') MemMap.IO_INPUT0 |= 0x01;
            else if (c == '4') MemMap.IO_INPUT0 |= 0x02;
            else if (c == '6') MemMap.IO_INPUT0 |= 0x04;
            else if (c == '2') MemMap.IO_INPUT0 |= 0x08;
        }

        public static void glutSpecialDown(int key, int x, int y)
        {
            if (key == GLUT_KEY_UP) MemMap.IO_INPUT0 &= 0xfe;
            else if (key == GLUT_KEY_LEFT) MemMap.IO_INPUT0 &= 0xfd;
            else if (key == GLUT_KEY_RIGHT) MemMap.IO_INPUT0 &= 0xfb;
            else if (key == GLUT_KEY_DOWN) MemMap.IO_INPUT0 &= 0xf7;
        }

        public static void glutSpecialUp(int key, int x, int y)
        {
            if (key == GLUT_KEY_UP) MemMap.IO_INPUT0 |= 0x01;
            else if (key == GLUT_KEY_LEFT) MemMap.IO_INPUT0 |= 0x02;
            else if (key == GLUT_KEY_RIGHT) MemMap.IO_INPUT0 |= 0x04;
            else if (key == GLUT_KEY_DOWN) MemMap.IO_INPUT0 |= 0x08;
        }
    }
}
