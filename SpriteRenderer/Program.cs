// =============================================================================
// Program.cs - Pac-Man 5F Sprite Renderer (All 32 Color Palettes - 2x Double Size)
//
// Reads Pac-Man sprite ROM data (pacman.5f or include/roms/pacman.5f.h)
// and renders all 64 sprites across ALL 32 hardware color palettes (0 to 31)
// in double size (2x scale):
//   - 32 individual BMP files (sprite_palette_00.bmp .. sprite_palette_31.bmp)
//   - 1 overall combined master BMP file (sprites_all_palettes.bmp - 8x4 matrix grid)
//   - 1 dedicated fruit palette summary BMP (sprites_fruit_palettes.bmp)
// =============================================================================

using System;
using System.IO;
using System.Text.RegularExpressions;

namespace SpriteRenderer
{
    class Program
    {
        static byte[] paletteRom = new byte[16];
        static byte[] colourTable = new byte[128];

        /// <summary>
        /// Finds the root directory containing 'rom' or 'include' folders.
        /// </summary>
        static string FindProjectRoot()
        {
            string dir = AppDomain.CurrentDomain.BaseDirectory;
            while (!string.IsNullOrEmpty(dir))
            {
                if (Directory.Exists(Path.Combine(dir, "rom")) || Directory.Exists(Path.Combine(dir, "include")))
                {
                    return dir;
                }
                var parent = Directory.GetParent(dir);
                if (parent == null) break;
                dir = parent.FullName;
            }
            dir = Directory.GetCurrentDirectory();
            while (!string.IsNullOrEmpty(dir))
            {
                if (Directory.Exists(Path.Combine(dir, "rom")) || Directory.Exists(Path.Combine(dir, "include")))
                {
                    return dir;
                }
                var parent = Directory.GetParent(dir);
                if (parent == null) break;
                dir = parent.FullName;
            }
            return Directory.GetCurrentDirectory();
        }

        /// <summary>
        /// Decodes a 2bpp pixel value (0..3) using palette table and PROM lookup.
        /// </summary>
        static (byte R, byte G, byte B) LookupColour(int colIndex)
        {
            // Step 1: colour table lookup (128-entry PROM)
            byte palIdx = colourTable[colIndex & 0x7F];

            // Step 2: palette PROM lookup (16-entry)
            byte raw = paletteRom[palIdx & 0x0F];

            // Step 3: decode BBGGGRRR → RGB
            byte r = (byte)((raw & 0x07) * 36);        // 3-bit red   → 0–252
            byte g = (byte)(((raw >> 3) & 0x07) * 36); // 3-bit green → 0–252
            byte b = (byte)((raw >> 6) * 85);           // 2-bit blue  → 0–255

            return (r, g, b);
        }

        /// <summary>
        /// Decodes a single 16×16 sprite pixel value (0..3) from ROM 5F data.
        /// Matches Pac-Man hardware sprite decoding logic in video.c exactly:
        ///   z = shape * 64 + ((y + 4) & 0x0c) * 2 + (7 - (x & 7))
        ///   if (x < 8) z += 32
        ///   bit0 = (rom5f[z] & (0x08 >> (y & 3))) != 0
        ///   bit1 = (rom5f[z] & (0x80 >> (y & 3))) != 0
        /// </summary>
        static int DecodeSpritePixel(byte[] rom5f, int shape, int x, int y)
        {
            int z = shape * 64;
            z += ((y + 4) & 0x0C) << 1;
            z += (7 - (x & 7));
            if ((x & 8) == 0)
            {
                z += 32;
            }

            byte data = rom5f[z];

            int bit0 = (data & (0x08 >> (y & 3))) != 0 ? 1 : 0;
            int bit1 = (data & (0x80 >> (y & 3))) != 0 ? 1 : 0;

            return bit0 | (bit1 << 1);
        }

        /// <summary>
        /// Loads binary ROM or parses C header file hex array.
        /// </summary>
        static byte[] LoadRomData(string basePath, string binaryFilename, string headerFilename)
        {
            string binPath = Path.Combine(basePath, "rom", binaryFilename);
            if (File.Exists(binPath))
            {
                Console.WriteLine($"Loading binary ROM file: {binPath}");
                return File.ReadAllBytes(binPath);
            }

            string headerPath = Path.Combine(basePath, "include", "roms", headerFilename);
            if (File.Exists(headerPath))
            {
                Console.WriteLine($"Parsing ROM C header file: {headerPath}");
                return ParseHeaderFile(headerPath);
            }

            throw new FileNotFoundException($"Could not find {binaryFilename} or {headerFilename}");
        }

        static byte[] ParseHeaderFile(string path)
        {
            string content = File.ReadAllText(path);
            int braceStart = content.IndexOf('{');
            int braceEnd = content.IndexOf('}');

            if (braceStart < 0 || braceEnd < 0 || braceEnd <= braceStart)
            {
                throw new FormatException($"Could not find array braces in {path}");
            }

            string arrayBody = content.Substring(braceStart + 1, braceEnd - braceStart - 1);
            var matches = Regex.Matches(arrayBody, @"0x([0-9A-Fa-f]{2})");
            byte[] result = new byte[matches.Count];

            for (int i = 0; i < matches.Count; i++)
            {
                result[i] = Convert.ToByte(matches[i].Groups[1].Value, 16);
            }

            return result;
        }

        /// <summary>
        /// Writes an uncompressed 24-bit RGB BMP file.
        /// </summary>
        static void WriteBmp(string path, byte[] pixelsRgb, int width, int height)
        {
            int rowStride = (width * 3 + 3) & ~3; // pad each row to 4-byte boundary
            int imageSize = rowStride * height;
            int fileSize = 54 + imageSize;

            using (var stream = new FileStream(path, FileMode.Create))
            using (var writer = new BinaryWriter(stream))
            {
                // BMP file header (14 bytes)
                writer.Write((byte)'B');
                writer.Write((byte)'M');
                writer.Write(fileSize);
                writer.Write((short)0);
                writer.Write((short)0);
                writer.Write(54);

                // DIB header (BITMAPINFOHEADER, 40 bytes)
                writer.Write(40);
                writer.Write(width);
                writer.Write(height);
                writer.Write((short)1);   // colour planes
                writer.Write((short)24);  // 24 bpp
                writer.Write(0);          // compression (none)
                writer.Write(imageSize);
                writer.Write(2835);       // horizontal DPI (~72 DPI)
                writer.Write(2835);       // vertical DPI (~72 DPI)
                writer.Write(0);
                writer.Write(0);

                // Pixel data (bottom-up row order, BGR order per pixel)
                for (int row = height - 1; row >= 0; row--)
                {
                    for (int col = 0; col < width; col++)
                    {
                        int idx = (row * width + col) * 3;
                        writer.Write(pixelsRgb[idx + 2]); // B
                        writer.Write(pixelsRgb[idx + 1]); // G
                        writer.Write(pixelsRgb[idx + 0]); // R
                    }

                    int padding = rowStride - (width * 3);
                    for (int p = 0; p < padding; p++)
                    {
                        writer.Write((byte)0);
                    }
                }
            }
        }

        /// <summary>
        /// Renders an 8x8 grid of 64 sprites for a specific palette ID into an RGB pixel buffer in double size (scale 2x).
        /// </summary>
        static void RenderSpriteSheet(byte[] rom5f, int paletteId, byte[] pixels, int imageWidth, int imageHeight, int startX = 0, int startY = 0, int scale = 2)
        {
            int spritesPerRow = 8;
            int baseSpriteSize = 16;
            int spriteSize = baseSpriteSize * scale;
            int gap = 1;

            for (int shape = 0; shape < 64; shape++)
            {
                int gridCol = shape % spritesPerRow;
                int gridRow = shape / spritesPerRow;

                int originX = startX + gap + gridCol * (spriteSize + gap);
                int originY = startY + gap + gridRow * (spriteSize + gap);

                for (int y = 0; y < baseSpriteSize; y++)
                {
                    for (int x = 0; x < baseSpriteSize; x++)
                    {
                        int pixelVal = DecodeSpritePixel(rom5f, shape, x, y);
                        int colIndex = (paletteId << 2) | pixelVal;
                        var (r, g, b) = LookupColour(colIndex);

                        for (int dy = 0; dy < scale; dy++)
                        {
                            for (int dx = 0; dx < scale; dx++)
                            {
                                int px = originX + x * scale + dx;
                                int py = originY + y * scale + dy;

                                if (px >= 0 && px < imageWidth && py >= 0 && py < imageHeight)
                                {
                                    int idx = (py * imageWidth + px) * 3;
                                    pixels[idx + 0] = r;
                                    pixels[idx + 1] = g;
                                    pixels[idx + 2] = b;
                                }
                            }
                        }
                    }
                }
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("==================================================================");
            Console.WriteLine("    Pac-Man ROM 5F Sprite Renderer (All 32 Palettes - 2x Scale)");
            Console.WriteLine("==================================================================");

            // Locate project root
            string baseDir = FindProjectRoot();
            Console.WriteLine($"Project Root Directory: {baseDir}");

            // Load ROM files
            byte[] rom5f;
            try
            {
                rom5f = LoadRomData(baseDir, "pacman.5f", "pacman.5f.h");
                Console.WriteLine($"Successfully loaded Sprite ROM 5F: {rom5f.Length} bytes ({rom5f.Length / 64} sprites)");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"ERROR: Failed to load pacman.5f: {ex.Message}");
                return;
            }

            try
            {
                paletteRom = LoadRomData(baseDir, "82s123.7f", "82s123.7f.h");
                Console.WriteLine($"Successfully loaded Palette PROM 82s123.7f: {paletteRom.Length} bytes");
            }
            catch
            {
                Console.WriteLine("Using embedded default Palette PROM 82s123.7f");
            }

            try
            {
                colourTable = LoadRomData(baseDir, "82s126.4a", "82s126.4a.h");
                Console.WriteLine($"Successfully loaded Colour Table PROM 82s126.4a: {colourTable.Length} bytes");
            }
            catch
            {
                Console.WriteLine("Using embedded default Colour Table PROM 82s126.4a");
            }

            // Output directory
            string outputDir = Path.Combine(baseDir, "output_sprites");
            Directory.CreateDirectory(outputDir);
            Console.WriteLine($"Output directory: {outputDir}");

            int scale = 2; // Output double size sprites

            // Dimensions for 1 single palette sheet (8x8 grid of double size 32x32 sprites + 1px gaps)
            int spritesPerRow = 8;
            int baseSpriteSize = 16;
            int spriteSize = baseSpriteSize * scale; // 32 px
            int gap = 1;
            int sheetWidth = spritesPerRow * (spriteSize + gap) + gap; // 8 * 33 + 1 = 265 px
            int sheetHeight = spritesPerRow * (spriteSize + gap) + gap; // 265 px

            // ── 1. Render ALL 32 Individual BMP Files (Palettes 0 to 31) ─────────────────
            Console.WriteLine("\n[1/3] Rendering 32 individual double-sized BMP files for ALL hardware sprite palettes (0 to 31)...");

            for (int pal = 0; pal < 32; pal++)
            {
                byte[] sheetPixels = new byte[sheetWidth * sheetHeight * 3];

                // Fill background with dark grey (0x1A) for grid gaps
                for (int i = 0; i < sheetPixels.Length; i += 3)
                {
                    sheetPixels[i + 0] = 0x1A;
                    sheetPixels[i + 1] = 0x1A;
                    sheetPixels[i + 2] = 0x1A;
                }

                RenderSpriteSheet(rom5f, pal, sheetPixels, sheetWidth, sheetHeight, 0, 0, scale);

                string fileName = $"sprite_palette_{pal:D2}.bmp";
                string fullPath = Path.Combine(outputDir, fileName);
                WriteBmp(fullPath, sheetPixels, sheetWidth, sheetHeight);

                // Also write to workspace root for convenient access
                string rootPath = Path.Combine(baseDir, fileName);
                WriteBmp(rootPath, sheetPixels, sheetWidth, sheetHeight);

                Console.WriteLine($"  -> Generated {fileName} ({sheetWidth}x{sheetHeight} px)");
            }

            // ── 2. Render 1 Master Overall BMP File (8x4 Grid of All 32 Palette Sheets) ───
            Console.WriteLine("\n[2/3] Rendering master overall double-sized BMP file combining ALL 32 palettes (8x4 grid)...");

            int gridCols = 8;
            int gridRows = 4;
            int margin = 4; // 4px margin between palette blocks

            int overallWidth = gridCols * sheetWidth + (gridCols + 1) * margin;  // 8*265 + 9*4 = 2156 px
            int overallHeight = gridRows * sheetHeight + (gridRows + 1) * margin; // 4*265 + 5*4 = 1080 px

            byte[] overallPixels = new byte[overallWidth * overallHeight * 3];

            // Fill background with dark slate grey (0x28) for section gaps
            for (int i = 0; i < overallPixels.Length; i += 3)
            {
                overallPixels[i + 0] = 0x28;
                overallPixels[i + 1] = 0x28;
                overallPixels[i + 2] = 0x28;
            }

            for (int pal = 0; pal < 32; pal++)
            {
                int col = pal % gridCols;
                int row = pal / gridCols;

                int blockX = margin + col * (sheetWidth + margin);
                int blockY = margin + row * (sheetHeight + margin);

                // Pre-fill block grid background
                for (int py = 0; py < sheetHeight; py++)
                {
                    for (int px = 0; px < sheetWidth; px++)
                    {
                        int idx = ((blockY + py) * overallWidth + (blockX + px)) * 3;
                        overallPixels[idx + 0] = 0x1A;
                        overallPixels[idx + 1] = 0x1A;
                        overallPixels[idx + 2] = 0x1A;
                    }
                }

                RenderSpriteSheet(rom5f, pal, overallPixels, overallWidth, overallHeight, blockX, blockY, scale);
            }

            string overallFileName = "sprites_all_palettes.bmp";
            string overallFullPath = Path.Combine(outputDir, overallFileName);
            WriteBmp(overallFullPath, overallPixels, overallWidth, overallHeight);

            string overallRootPath = Path.Combine(baseDir, overallFileName);
            WriteBmp(overallRootPath, overallPixels, overallWidth, overallHeight);

            string altOverallPath = Path.Combine(baseDir, "pacman_sprites_overall.bmp");
            WriteBmp(altOverallPath, overallPixels, overallWidth, overallHeight);

            Console.WriteLine($"  -> Generated master overall bitmap: {overallFileName} ({overallWidth}x{overallHeight} px)");

            // ── 3. Render Dedicated Fruit Palettes Summary BMP (Palettes 20 to 31) ────────
            Console.WriteLine("\n[3/3] Rendering dedicated Fruit & Bonus Items summary double-sized BMP (Palettes 20 to 31)...");

            int fruitCols = 4;
            int fruitRows = 3;
            int fruitWidth = fruitCols * sheetWidth + (fruitCols + 1) * margin;  // 4*265 + 5*4 = 1080 px
            int fruitHeight = fruitRows * sheetHeight + (fruitRows + 1) * margin; // 3*265 + 4*4 = 811 px

            byte[] fruitPixels = new byte[fruitWidth * fruitHeight * 3];

            for (int i = 0; i < fruitPixels.Length; i += 3)
            {
                fruitPixels[i + 0] = 0x28;
                fruitPixels[i + 1] = 0x28;
                fruitPixels[i + 2] = 0x28;
            }

            for (int i = 0; i < 12; i++)
            {
                int pal = 20 + i; // Palettes 20 through 31
                int col = i % fruitCols;
                int row = i / fruitCols;

                int blockX = margin + col * (sheetWidth + margin);
                int blockY = margin + row * (sheetHeight + margin);

                for (int py = 0; py < sheetHeight; py++)
                {
                    for (int px = 0; px < sheetWidth; px++)
                    {
                        int idx = ((blockY + py) * fruitWidth + (blockX + px)) * 3;
                        fruitPixels[idx + 0] = 0x1A;
                        fruitPixels[idx + 1] = 0x1A;
                        fruitPixels[idx + 2] = 0x1A;
                    }
                }

                RenderSpriteSheet(rom5f, pal, fruitPixels, fruitWidth, fruitHeight, blockX, blockY, scale);
            }

            string fruitFileName = "sprites_fruit_palettes.bmp";
            string fruitFullPath = Path.Combine(outputDir, fruitFileName);
            WriteBmp(fruitFullPath, fruitPixels, fruitWidth, fruitHeight);

            string fruitRootPath = Path.Combine(baseDir, fruitFileName);
            WriteBmp(fruitRootPath, fruitPixels, fruitWidth, fruitHeight);

            Console.WriteLine($"  -> Generated fruit summary bitmap: {fruitFileName} ({fruitWidth}x{fruitHeight} px)");
            Console.WriteLine("\nDONE! All 32 individual palette BMPs, master overall BMP, and fruit summary BMP rendered successfully in 2x double size.");
        }
    }
}

