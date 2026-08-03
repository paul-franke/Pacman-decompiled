// =============================================================================
// TileRenderer.cs
//
// Reads the Pac-Man tile ROM data from include/roms/pacman.5e.h and renders all
// 256 tiles into a BMP file (pacman_tiles.bmp).
//
// Each tile is 8x8 pixels, 2 bits per pixel. The ROM is 4096 bytes =
// 256 tiles × 16 bytes/tile.
//
// Pixel decoding matches the original video.c videoDrawChar() logic exactly.
//
// The colour palette uses the authentic 82s123.7f (RGB palette) and 82s126.4a
// (colour table) PROMs. Each tile is drawn with colour index 1 (the standard
// Pac-Man maze/text colour) so all four 2bpp pixel values map to visible
// palette entries.
//
// Usage:
//   csc TileRenderer.cs
//   TileRenderer.exe
//
// Output: pacman_tiles.bmp  (16×16 grid of tiles, each 8×8 pixels = 128×128 px)
// =============================================================================

using System;
using System.IO;
using System.Text.RegularExpressions;

class TileRenderer
{
    // ── Authentic Pac-Man colour palette PROM (82s123.7f) ────────────────────
    // 16 active entries. Each byte encodes BBGGGRRR (2-3-3 bit RGB).
    static readonly byte[] PaletteRom = {
        0x00, 0x07, 0x66, 0xEF, 0x00, 0xF8, 0xEA, 0x6F,
        0x00, 0x3F, 0x00, 0xC9, 0x38, 0xAA, 0xAF, 0xF6
    };

    // ── Authentic Pac-Man colour table PROM (82s126.4a) ─────────────────────
    // 128 entries (only first 128 bytes used, masked with 0x7F).
    // Each entry is a 4-bit palette index into PaletteRom.
    static readonly byte[] ColourTable = {
        0x00, 0x00, 0x00, 0x00, 0x00, 0x0F, 0x0B, 0x01,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x0F, 0x0B, 0x03,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x0F, 0x0B, 0x05,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x0F, 0x0B, 0x07,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x0B, 0x01, 0x09,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x0F, 0x00, 0x0E, 0x00, 0x01, 0x0C, 0x0F,
        0x00, 0x0E, 0x00, 0x0B, 0x00, 0x0C, 0x0B, 0x0E,
        0x00, 0x0C, 0x0F, 0x01, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x01, 0x02, 0x0F, 0x00, 0x07, 0x0C, 0x02,
        0x00, 0x09, 0x06, 0x0F, 0x00, 0x0D, 0x0C, 0x0F,
        0x00, 0x05, 0x03, 0x09, 0x00, 0x0F, 0x0B, 0x00,
        0x00, 0x0E, 0x00, 0x0B, 0x00, 0x0E, 0x00, 0x0B,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x0F, 0x0E, 0x01,
        0x00, 0x0F, 0x0B, 0x0E, 0x00, 0x0E, 0x00, 0x0F
    };

    /// <summary>
    /// Converts a colour index (chrCol << 2 | 2bpp pixel value) to an RGB
    /// tuple using the authentic Pac-Man palette PROMs.
    /// Matches video.c videoColourLookup() exactly.
    /// </summary>
    static (byte R, byte G, byte B) LookupColour(int colIndex)
    {
        // Step 1: colour table lookup (128-entry PROM)
        byte palIdx = ColourTable[colIndex & 0x7F];

        // Step 2: palette PROM lookup (16-entry)
        byte raw = PaletteRom[palIdx & 0x0F];

        // Step 3: decode BBGGGRRR → RGB
        byte r = (byte)((raw & 0x07) * 36);       // 3-bit red   → 0–252
        byte g = (byte)(((raw >> 3) & 0x07) * 36); // 3-bit green → 0–252
        byte b = (byte)((raw >> 6) * 85);           // 2-bit blue  → 0–255

        return (r, g, b);
    }

    /// <summary>
    /// Decodes a single 8×8 tile pixel value using the exact same algorithm as
    /// videoDrawChar() in video.c — no rotation applied.
    ///
    /// For pixel (x, y) within the tile:
    ///   z = tileIndex * 16 + (4 - (y & 4)) * 2 + (7 - x)
    ///   bit0 = (rom[z] & (0x08 >> (y & 3))) != 0
    ///   bit1 = (rom[z] & (0x80 >> (y & 3))) != 0
    ///   pixelValue = bit0 | (bit1 << 1)    → 0..3
    /// </summary>
    static int DecodeTilePixel(byte[] rom, int tileIndex, int x, int y)
    {
        int z = tileIndex * 16;
        z += (4 - (y & 4)) * 2;
        z += (7 - x);

        byte data = rom[z];

        int bit0 = (data & (0x08 >> (y & 3))) != 0 ? 1 : 0;
        int bit1 = (data & (0x80 >> (y & 3))) != 0 ? 1 : 0;

        return bit0 | (bit1 << 1);
    }

    /// <summary>
    /// Parses the C header file to extract the hex byte array.
    /// Handles the format: unsigned char rom_pacman_5e[] = { 0xCC, ... };
    /// </summary>
    static byte[] ParseHeaderFile(string path)
    {
        string content = File.ReadAllText(path);

        // Extract everything between the braces { ... }
        int braceStart = content.IndexOf('{');
        int braceEnd = content.IndexOf('}');

        if (braceStart < 0 || braceEnd < 0 || braceEnd <= braceStart)
        {
            throw new FormatException($"Could not find array braces in {path}");
        }

        string arrayBody = content.Substring(braceStart + 1, braceEnd - braceStart - 1);

        // Match all hex values (0xNN)
        var matches = Regex.Matches(arrayBody, @"0x([0-9A-Fa-f]{2})");
        byte[] result = new byte[matches.Count];

        for (int i = 0; i < matches.Count; i++)
        {
            result[i] = Convert.ToByte(matches[i].Groups[1].Value, 16);
        }

        return result;
    }

    /// <summary>
    /// Writes a 24-bit BMP file (no external dependencies).
    /// Pixels are stored bottom-up as per BMP spec; rows are padded to 4-byte
    /// alignment.
    /// </summary>
    static void WriteBmp(string path, byte[] pixelsRgb, int width, int height)
    {
        int rowStride = (width * 3 + 3) & ~3; // pad each row to 4-byte boundary
        int imageSize = rowStride * height;
        int fileSize = 54 + imageSize;         // 14 (file hdr) + 40 (DIB hdr) + pixels

        using (var stream = new FileStream(path, FileMode.Create))
        using (var writer = new BinaryWriter(stream))
        {
            // ── BMP file header (14 bytes) ──────────────────────────────────
            writer.Write((byte)'B');
            writer.Write((byte)'M');
            writer.Write(fileSize);
            writer.Write((short)0);      // reserved
            writer.Write((short)0);      // reserved
            writer.Write(54);            // pixel data offset

            // ── DIB header (BITMAPINFOHEADER, 40 bytes) ─────────────────────
            writer.Write(40);            // header size
            writer.Write(width);
            writer.Write(height);
            writer.Write((short)1);      // colour planes
            writer.Write((short)24);     // bits per pixel
            writer.Write(0);             // compression (none)
            writer.Write(imageSize);
            writer.Write(2835);          // horizontal DPI (~72)
            writer.Write(2835);          // vertical DPI (~72)
            writer.Write(0);             // colours in palette
            writer.Write(0);             // important colours

            // ── Pixel data (bottom-up row order, BGR per pixel) ─────────────
            for (int row = height - 1; row >= 0; row--)
            {
                for (int col = 0; col < width; col++)
                {
                    int idx = (row * width + col) * 3;
                    // BMP stores BGR
                    writer.Write(pixelsRgb[idx + 2]); // B
                    writer.Write(pixelsRgb[idx + 1]); // G
                    writer.Write(pixelsRgb[idx + 0]); // R
                }

                // Write padding bytes to reach 4-byte row alignment
                int padding = rowStride - (width * 3);
                for (int p = 0; p < padding; p++)
                {
                    writer.Write((byte)0);
                }
            }
        }
    }

    static void Main(string[] args)
    {
        // ── Locate the ROM header file ──────────────────────────────────────
        string scriptDir = AppDomain.CurrentDomain.BaseDirectory;
        string tempLoc = "D:\\repos\\Pacman-decompiled\\";
               tempLoc = Path.Combine(scriptDir, "include", "roms", "pacman.5e.h");

        // Try relative path from build output first, then current directory
        string headerPath = Path.Combine(scriptDir, "include", "roms", "pacman.5e.h");

        if (!File.Exists(headerPath))
        {
            headerPath = Path.Combine(Directory.GetCurrentDirectory(),
                                      "include", "roms", "pacman.5e.h");
        }

        if (!File.Exists(headerPath))
        {
            // Allow explicit path as argument
            if (args.Length > 0 && File.Exists(args[0]))
            {
                headerPath = args[0];
            }
            else
            {
                headerPath = Path.Combine("D:\\repos\\Pacman-decompiled\\",
                                      "include", "roms", "pacman.5e.h");
                if (!File.Exists(headerPath))
                {

                    Console.Error.WriteLine("ERROR: Cannot find include\\roms\\pacman.5e.h");
                    Console.Error.WriteLine("Run from the project root or pass the path as an argument.");
                    Environment.Exit(1);
                }
            }
        }

        Console.WriteLine($"Reading ROM data from: {headerPath}");

        // ── Parse the header ────────────────────────────────────────────────
        byte[] rom = ParseHeaderFile(headerPath);
        Console.WriteLine($"Parsed {rom.Length} bytes ({rom.Length / 16} tiles)");

        if (rom.Length != 4096)
        {
            Console.Error.WriteLine($"WARNING: Expected 4096 bytes, got {rom.Length}");
        }

        int tileCount = rom.Length / 16;

        // ── Layout: 16 tiles per row ────────────────────────────────────────
        int tilesPerRow = 16;
        int gridRows = (tileCount + tilesPerRow - 1) / tilesPerRow;
        int tilePixels = 8;
        int gap = 1; // 1-pixel gap between tiles for clarity

        int imageWidth = tilesPerRow * (tilePixels + gap) + gap;
        int imageHeight = gridRows * (tilePixels + gap) + gap;

        // RGB pixel buffer (3 bytes per pixel)
        byte[] pixels = new byte[imageWidth * imageHeight * 3];

        // Fill background with dark grey (0x1A) for the grid gaps
        for (int i = 0; i < pixels.Length; i += 3)
        {
            pixels[i + 0] = 0x1A;
            pixels[i + 1] = 0x1A;
            pixels[i + 2] = 0x1A;
        }

        // ── Render each tile ────────────────────────────────────────────────
        // Use colour index 1 (standard Pac-Man blue/white palette) for all
        // tiles so the shapes are clearly visible.
        int chrCol = 1;

        for (int tile = 0; tile < tileCount; tile++)
        {
            int gridCol = tile % tilesPerRow;
            int gridRow = tile / tilesPerRow;

            int originX = gap + gridCol * (tilePixels + gap);
            int originY = gap + gridRow * (tilePixels + gap);

            for (int y = 0; y < tilePixels; y++)
            {
                for (int x = 0; x < tilePixels; x++)
                {
                    int pixelValue = DecodeTilePixel(rom, tile, x, y);

                    // Build the colour table index the same way as video.c:
                    //   col = (chrCol << 2) | 2bpp_value
                    int colIndex = (chrCol << 2) | pixelValue;
                    var (r, g, b) = LookupColour(colIndex);

                    int px = originX + x;
                    int py = originY + y;
                    int idx = (py * imageWidth + px) * 3;

                    pixels[idx + 0] = r;
                    pixels[idx + 1] = g;
                    pixels[idx + 2] = b;
                }
            }
        }

        // ── Write the BMP ───────────────────────────────────────────────────
        string outputPath = Path.Combine(Directory.GetCurrentDirectory(), "pacman_tiles.bmp");
        WriteBmp(outputPath, pixels, imageWidth, imageHeight);

        Console.WriteLine($"Wrote {imageWidth}×{imageHeight} bitmap to: {outputPath}");
        Console.WriteLine($"Rendered {tileCount} tiles in a {tilesPerRow}×{gridRows} grid");
    }
}
