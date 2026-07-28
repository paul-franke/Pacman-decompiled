# Pac-Man ROM Graphics & Audio Analysis (5E, 5F, 82s123.7f, 82s126.1m, 82s126.3m & 82s126.4a)

The original physical *Pac-Man* arcade machine uses several PROM and ROM chips to store graphics, colors, and audio waveform data. Below is the detailed analysis of these components.

---

## 1. Pac-Man ROM 5E (Character Graphics)

### Specifications
- **Size:** 4,096 bytes (4 KB)
- **Format:** 2 bits-per-pixel (2bpp) indexed color graphics
- **Resolution:** 8x8 pixels per tile
- **Capacity:** 256 tiles

### Decode Logic and Memory Layout
For any tile index `chr` (0 to 255) and pixel coordinates `(x, y)` (0 to 7) within the 8x8 cell:
1. The 16 bytes representing the tile are located at `chr * 16`.
2. The byte index `z` is calculated depending on the row `y`:
   - If `y < 4` (top half): `z = 8 + (7 - x)` (bytes 8 to 15)
   - If `y >= 4` (bottom half): `z = 0 + (7 - x)` (bytes 0 to 7)
3. The bit planes are extracted:
   - **Plane 0 bit:** `p0 = 1` if `(byte & (0x08 >> (y & 3)))` is non-zero, else `0`
   - **Plane 1 bit:** `p1 = 1` if `(byte & (0x80 >> (y & 3)))` is non-zero, else `0`
4. The pixel color value is: `color_index = (p1 << 1) | p0` (values 0, 1, 2, or 3)

### Decoded Character Grid
- **Index `0x00` - `0x0F`:** Alphanumeric digits `0`-`9` and letters `A`-`F`
- **Index `0x10` - `0x1F`:** Special symbols, bonus items, ghost eyes, and HUD elements
- **Index `0x20` - `0xFF`:** Maze borders, corners, walls, and gameplay text strings

![Pac-Man 5E Tile Graphics Grid](file:///C:/Users/paulf/.gemini/antigravity/brain/09200c2d-ffdf-4599-90a0-b08f3c93e31f/pacman_5e_tiles.png)

---

## 2. Pac-Man ROM 5F (Sprite Graphics)

### Specifications
- **Size:** 4,096 bytes (4 KB)
- **Format:** 2 bits-per-pixel (2bpp) indexed color graphics
- **Resolution:** 16x16 pixels per sprite
- **Capacity:** 64 sprites (each sprite occupies 64 bytes)

### Decode Logic and Memory Layout
For any sprite index `shape` (0 to 63) and pixel coordinates `(x, y)` (0 to 15) within the 16x16 cell:
1. The 64 bytes representing the sprite are located at `shape * 64`.
2. The byte index `z` (0 to 63) within the sprite block is calculated:
   - Base offset `offset_1 = ((y + 4) & 0x0c) << 1`
   - Horizontal offset `offset_2 = 7 - (x & 7)`
   - Combine: `z = offset_1 + offset_2`
   - If `x < 8` (left half of sprite): `z += 32`
3. The bit planes are extracted:
   - **Plane 0 bit:** `p0 = 1` if `(byte & (0x08 >> (y & 3)))` is non-zero, else `0`
   - **Plane 1 bit:** `p1 = 1` if `(byte & (0x80 >> (y & 3)))` is non-zero, else `0`
4. The pixel color value is: `color_index = (p1 << 1) | p0`

### Decoded Sprite Grid
- **Index `0x00` - `0x07`:** Fruits (cherries, strawberries, peaches, apples, melons, galaxian flagships, bells, keys)
- **Index `0x08` - `0x0F`:** Ghosts (Blinky, Pinky, Inky, Clyde with directional eyes, and flashing states)
- **Index `0x10` - `0x1F`:** Pac-Man movement orientations and dying/crumpling frames
- **Index `0x20` - `0x3F`:** Special cutscene sprites, animations, and auxiliary assets

![Pac-Man 5F Sprite Graphics Grid](file:///C:/Users/paulf/.gemini/antigravity/brain/09200c2d-ffdf-4599-90a0-b08f3c93e31f/pacman_5f_sprites.png)

---

## 3. Pac-Man ROM 82s123.7f (Color Palette)

### Specifications
- **Type:** Bipolar PROM (82s123 or compatible)
- **Size:** 32 bytes (only the first 16 bytes are used by the hardware palette)
- **Format:** 1 byte per color entry, mapped as **2-3-3 RGB** (BBGGGRRR in binary):
  - **Bits 0–2:** Red intensity (0 to 7)
  - **Bits 3–5:** Green intensity (0 to 7)
  - **Bits 6–7:** Blue intensity (0 to 3)

### Decoded Palette Table
Using the hardware RGB weighting formulas (R, G multiplied by 36; B multiplied by 85):

| Index | Hex Value | Binary (BBGGGRRR) | Red (raw) | Green (raw) | Blue (raw) | RGB Color | Hex Color | Visual Color | Usage / Role |
| :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :--- |
| **0** | `0x00` | `00 000 000` | 0 | 0 | 0 | (0, 0, 0) | `#000000` | <span style="color:#000000">████</span> | Transparent / Black |
| **1** | `0x07` | `00 000 111` | 7 | 0 | 0 | (252, 0, 0) | `#FC0000` | <span style="color:#FC0000">████</span> | Red (Blinky / Cherry) |
| **2** | `0x66` | `01 100 110` | 6 | 4 | 1 | (216, 144, 85) | `#D89055` | <span style="color:#D89055">████</span> | Orange-Brown |
| **3** | `0xEF` | `11 101 111` | 7 | 5 | 3 | (252, 180, 255) | `#FCB4FF` | <span style="color:#FCB4FF">████</span> | Pink (Pinky) |
| **4** | `0x00` | `00 000 000` | 0 | 0 | 0 | (0, 0, 0) | `#000000` | <span style="color:#000000">████</span> | Unused |
| **5** | `0xF8` | `11 111 000` | 0 | 7 | 3 | (0, 252, 255) | `#00FCFF` | <span style="color:#00FCFF">████</span> | Cyan (Inky) |
| **6** | `0xEA` | `11 101 010` | 2 | 5 | 3 | (72, 180, 255) | `#48B4FF` | <span style="color:#48B4FF">████</span> | Light Blue (Frightened Ghost) |
| **7** | `0x6F` | `01 101 111` | 7 | 5 | 1 | (252, 180, 85) | `#FCB455` | <span style="color:#FCB455">████</span> | Orange (Clyde) |
| **8** | `0x00` | `00 000 000` | 0 | 0 | 0 | (0, 0, 0) | `#000000` | <span style="color:#000000">████</span> | Unused |
| **9** | `0x3F` | `00 111 111` | 7 | 7 | 0 | (252, 252, 0) | `#FCFC00` | <span style="color:#FCFC00">████</span> | Yellow (Pac-Man) |
| **10** | `0x00` | `00 000 000` | 0 | 0 | 0 | (0, 0, 0) | `#000000` | <span style="color:#000000">████</span> | Unused |
| **11** | `0xC9` | `11 001 001` | 1 | 1 | 3 | (36, 36, 255) | `#2424FF` | <span style="color:#2424FF">████</span> | Blue (Maze Walls / Eyes) |
| **12** | `0x38` | `00 111 000` | 0 | 7 | 0 | (0, 252, 0) | `#00FC00` | <span style="color:#00FC00">████</span> | Green |
| **13** | `0xAA` | `10 101 010` | 2 | 5 | 2 | (72, 180, 170) | `#48B4AA` | <span style="color:#48B4AA">████</span> | Light Green / Teal |
| **14** | `0xAF` | `10 101 111` | 7 | 5 | 2 | (252, 180, 170) | `#FCB4AA` | <span style="color:#FCB4AA">████</span> | Salmon Pink / Flesh tone |
| **15** | `0xF6` | `11 110 110` | 6 | 6 | 3 | (216, 216, 255) | `#D8D8FF` | <span style="color:#D8D8FF">████</span> | Faded Cyan-White (Frightened flash) |

*Note: Indices 16–31 are all `0x00` (unused padding).*

### Decoded Palette Swatches
Here is the visual rendering of the 16 colors:

![Pac-Man 16-Color Palette Swatches](file:///C:/Users/paulf/.gemini/antigravity/brain/09200c2d-ffdf-4599-90a0-b08f3c93e31f/pacman_palette.png)

---

## 4. Pac-Man ROMs 82s126.1m & 82s126.3m (Sound Waves)

The sound hardware uses custom sound waves stored in two identical bipolar PROMs (1M and 3M). Together, they are loaded into a single contiguous `soundRom` block of 512 bytes:
- **`82s126.1m` (Audio ROM 0):** Loaded at offset `0x0000` (Waveforms 0 to 7)
- **`82s126.3m` (Audio ROM 1):** Loaded at offset `0x0100` (Waveforms 8 to 15)

### Specifications
- **Type:** Bipolar PROM (82s126 or compatible)
- **Size:** 256 bytes per PROM
- **Format:** 4-bit sound amplitude samples, stored at 1 sample per byte (values `0` to `15`, center axis is `8`)
- **Capacity:** 8 waveforms per PROM (each waveform consists of 32 sequential samples: $8 \times 32 = 256$ bytes)

---

### Waveforms in ROM 1M (Waveforms 0 to 7)
Contains the active synthesized waves for standard sound effects:
- **Wave 0 (Bytes 0–31):** Sine Wave Approximation (smooth rising and falling curve)
- **Wave 1 (Bytes 32–63):** Double-Peak Harmonics 1
- **Wave 2 (Bytes 64–95):** Double-Peak Harmonics 2
- **Wave 3 (Bytes 96–127):** Complex Acoustic Waveform
- **Wave 4 (Bytes 128–159):** High-Frequency Fluctuation (rapidly oscillating sound wave)
- **Wave 5 (Bytes 160–191):** Expand-and-Oscillate Triangle (expanding triangular oscillation centered around 7.5)
- **Wave 6 (Bytes 192–223):** Classic Symmetric Triangle Wave (ramps from 0 to 15, then back to 0)
- **Wave 7 (Bytes 224–255):** Sawtooth Wave (two consecutive ramps from 0 to 15)

![Pac-Man 8-Wave Sound ROM Shapes](file:///C:/Users/paulf/.gemini/antigravity/brain/09200c2d-ffdf-4599-90a0-b08f3c93e31f/pacman_sound_waves.png)

---

### Waveforms in ROM 3M (Waveforms 8 to 15)
Contains alternate waves. Only the first four waves have data, while the final four are padded with `0x00`:
- **Wave 8 (Bytes 0–31):** Pulse-like Waveform A
- **Wave 9 (Bytes 32–63):** Pulse-like Waveform B (exact duplicate of Wave 8)
- **Wave 10 (Bytes 64–95):** Jagged Pulse Waveform A
- **Wave 11 (Bytes 96–127):** Jagged Pulse Waveform B (exact duplicate of Wave 10)
- **Waves 12–15 (Bytes 128–255):** Unused (Padded entirely with `0x00`)

*Note: In the standard game code, these waveforms (8 to 15) are generally not referenced by active voice effects.*

![Pac-Man 8-Wave Sound ROM 3M Shapes](file:///C:/Users/paulf/.gemini/antigravity/brain/09200c2d-ffdf-4599-90a0-b08f3c93e31f/pacman_sound_waves_3m.png)

---

## 5. Pac-Man ROM 82s126.4a (Color Lookup Table)

### Specifications
- **Type:** Bipolar PROM (82s126 or compatible)
- **Size:** 256 bytes (only the first 128 bytes are active/used)
- **Capacity:** 32 color palettes (each palette consists of 4 color index bytes mapping to `82s123.7f` color codes: $32 \times 4 = 128$ bytes)

### Decode Logic
The hardware indexes into this ROM using a combined 7-bit value:
`col_index = (chrCol << 2) | pixel_2bpp`
- `chrCol` is the 5-bit palette ID (0 to 31) written by the CPU to Video/Sprite attribute RAM.
- `pixel_2bpp` is the 2-bit value (0 to 3) representing the raw pixel shape color from graphic ROM `5e` or `5f`.

### Decoded Palette Swatches
Below is the rendered grid of the 32 palettes. Each entry displays its 4 colors (Pixel 0, 1, 2, and 3):

![Pac-Man 32-Palette Color Lookup Table Grid](file:///C:/Users/paulf/.gemini/antigravity/brain/09200c2d-ffdf-4599-90a0-b08f3c93e31f/pacman_color_table.png)

---

## 6. Video RAM (`0x4000 - 0x43FF`) Screen Mapping

The Pac-Man VRAM (Video RAM) contains 1,024 bytes mapping to a physical tile-based monitor of **28 columns** (x = 0 to 27) and **36 rows** (y = 0 to 35) in portrait layout. Because the video processor scans the screen vertically, the VRAM has a highly custom, non-linear mapping scheme.

### Memory Organization Regions
The 1,024-byte VRAM is divided into three distinct segments:

1. **Top HUD Area (2 Rows, $y = 0, 1$):**
   - Mapped as contiguous horizontal rows, read right-to-left.
   - **Row 0 ($y=0$):** CPU addresses `0x43DD` (left side) down to `0x43C2` (right side).
   - **Row 1 ($y=1$):** CPU addresses `0x43FD` (left side) down to `0x43E2` (right side).
2. **Bottom Status Area (2 Rows, $y = 34, 35$):**
   - Mapped as contiguous horizontal rows, read right-to-left.
   - **Row 34 ($y=34$):** CPU addresses `0x401D` (left side) down to `0x4002` (right side).
   - **Row 35 ($y=35$):** CPU addresses `0x403D` (left side) down to `0x4022` (right side).
3. **Main Playfield Area (32 Rows, $y = 2$ to $33$):**
   - Mapped column-by-column rather than row-by-row.
   - Each column is 32 bytes wide, read bottom-to-top.
   - For column `x` (0 to 27) and row `y` (2 to 33):
     $$\text{address} = \text{0x43A0} + (y - 2) - 32 \times x$$
     * **Column 0 ($x=0$):** Addresses `0x43A0` (row 2) to `0x43BF` (row 33).
     * **Column 1 ($x=1$):** Addresses `0x4380` (row 2) to `0x439F` (row 33).
     * ...
     * **Column 27 ($x=27$):** Addresses `0x4000` (row 2) to `0x401F` (row 33).

### Reused VRAM for Sprite Attributes
The "gaps" or unused bytes at the end of each special row block are reused as the **Sprite Attribute Memory (OAM)** at the very end of VRAM:
- **`0x4FF0 - 0x4FFF`:** Maps directly to offset `0x3F0 - 0x3FF` relative to `0x4000`, which contains coordinates and flip configurations for the 8 active sprites.

### Screen Layout Grid Diagram
The following diagram colors the cells by their region and displays the exact hexadecimal Z80 address mapped to each tile coordinates:

![Pac-Man Video RAM 0x4000-0x43FF Screen Mapping](file:///C:/Users/paulf/.gemini/antigravity/brain/09200c2d-ffdf-4599-90a0-b08f3c93e31f/pacman_vram_map.png)

---

## 7. Color Attribute RAM (`0x4400 - 0x47FF`)

The Pac-Man Color RAM is a 1,024-byte block of memory located immediately following the Video RAM in the Z80 CPU memory space. It holds the palette configurations for background tiles.

### Memory Organization
* **1:1 Mapping to VRAM:** Every tile position at offset `pos` (0 to `0x3FF`) has its character index stored at Video RAM `0x4000 + pos`, and its color palette settings stored at Color RAM `0x4400 + pos`.
* It follows the **exact same** non-linear 28x36 grid layout as the Video RAM described in Section 6.

### How it Works (Bits 0–4)
For each tile, the byte in Color RAM contains a **5-bit palette ID** in its lower 5 bits (values `0` to `31`):
1. **CPU Write:** The CPU writes the 5-bit color code to `0x4400 + pos`.
2. **Video Pipeline Offset:** The video hardware reads the 5-bit value (`chrCol`) and multiplies it by 4 (shifting left: `chrCol << 2`) to select one of the 32 palettes from the **Color Lookup Table PROM (`82s126.4a`)**.
3. **Pixel Indexing:** For each pixel in the 8x8 tile, the 2-bit color index (0 to 3) from the Graphic ROM `5e` is added to this base offset.
4. **Color Output:** The resulting 7-bit index addresses the lookup PROM `82s126.4a` to retrieve the final 4-bit palette color which indexes the analog RGB color palette PROM `82s123.7f`.

*Note: The upper 3 bits (bits 5–7) of each Color RAM byte are ignored by the standard background character hardware.*

---

## 8. Sprite Attribute Memory (OAM) (`0x4FF0 - 0x4FFF`)

The **Sprite Attribute RAM (OAM)** occupies 16 bytes at the upper end of VRAM. It controls the display properties of the 8 hardware sprites:

### Memory Layout
For each sprite index `i` (from 0 to 7):
* **Byte 0 (`0x4FF0 + i * 2`):** Sprite shape and mirror/flip controls:
  * **Bits 2–7 (6 bits):** Sprite graphic index (0 to 63) inside Sprite ROM `5f`.
  * **Bit 1 (1 bit):** Flip X (`1` = mirror sprite horizontally, `0` = normal rendering).
  * **Bit 0 (1 bit):** Flip Y (`1` = mirror sprite vertically, `0` = normal rendering).
* **Byte 1 (`0x4FF0 + i * 2 + 1`):** Color palette:
  * **Bits 0–4 (5 bits):** Palette ID (0 to 31) pointing to Color Lookup Table `82s126.4a`.

### Render Priority and Sprite Swapping
* **Priority Order:** The video generation chip scans and renders sprites in reverse order from Sprite 7 down to Sprite 0. This gives **Sprite 0 the highest priority** (drawn on top of all other sprites).
* **Power-up Priority Swapping:** Normally, Blinky (Red Ghost) is assigned to Sprite 0 and Pac-Man is assigned to Sprite 4. When Pac-Man eats a power pill and is powered up, the game code swaps their position states in RAM. This shifts Pac-Man to Sprite 0, ensuring he is drawn on top of the blue ghosts when overlapping/eating them.

---

## 9. CPU Working RAM Variables Map (`0x4800 - 0x4FEF`)

The CPU Working RAM occupies `2,032` bytes. Decompilation of the ROM discloses the locations of key variables that control the game state, positioning, speeds, states, and scores:

### General & Timer Variables
- **`0x4C80 - 0x4C81`:** `TASK_LIST_END` (2-byte Z80 address pointer)
- **`0x4C82 - 0x4C83`:** `TASK_LIST_BEGIN` (2-byte Z80 address pointer)
- **`0x4C84 - 0x4C85`:** `SOUND_COUNTER`
- **`0x4C86 - 0x4C87`:** `TIMER_SIXTIETHS` (2-byte tick counter)
- **`0x4C87`:** `TIMER_SECONDS` (1 byte)
- **`0x4C88`:** `TIMER_MINUTES` (1 byte)
- **`0x4C89`:** `TIMER_HOURS` (1 byte)
- **`0x4C8B`:** `RND_NUM_GEN1` (Pseudo-random number generator register 1)
- **`0x4C8C`:** `RND_NUM_GEN2` (Pseudo-random number generator register 2)
- **`0x4C90 - 0x4CBF`:** `ISR_TASKS` (Interrupt tasks list, 16 entries of 3 bytes each)
- **`0x4CC0 - 0x4CDF`:** `NONISR_TASKS` (Non-interrupt tasks list, 16 entries of 2 bytes each)

### Entity Position & Orientation Variables
Positions are represented using the 2-byte structure `XYPOS` (`x` and `y` offsets):

| Entity | Position Coord (`0x4D00+`) | Tile Coord (`0x4D0A+`) | Direction Vector (`0x4D14+`) | Current Orientation (`0x4D2C+`) | Next Tile Coord (`0x4D31+`) |
| :--- | :---: | :---: | :---: | :---: | :---: |
| **Blinky (Red)** | `0x4D00` | `0x4D0A` | `0x4D14` | `0x4D2C` | `0x4D31` |
| **Pinky (Pink)** | `0x4D02` | `0x4D0C` | `0x4D16` | `0x4D2D` | `0x4D33` |
| **Inky (Cyan)** | `0x4D04` | `0x4D0E` | `0x4D18` | `0x4D2E` | `0x4D35` |
| **Clyde (Orange)**| `0x4D06` | `0x4D10` | `0x4D1A` | `0x4D2F` | `0x4D37` |
| **Pac-Man** | `0x4D08` | `0x4D12` | `0x4D1C` | `0x4D30` | `0x4D39` |

*Note: Orientation values correspond to `0 = Up`, `1 = Left`, `2 = Down`, `3 = Right`.*

### Speed & Step Delay Patterns (4-byte registers)
- **`0x4D46`:** `PACMAN_MOVE_PAT_NORMAL` (Pac-Man standard speed mask)
- **`0x4D4A`:** `PACMAN_MOVE_PAT_POWERUP` (Pac-Man power pill speed mask)
- **`0x4D56`:** `BLINKY_MOVE_PAT_NORMAL` (Blinky standard speed mask)
- **`0x4D5A`:** `BLINKY_MOVE_PAT_EDIBLE` (Blinky frightened speed mask)
- **`0x4D5E`:** `BLINKY_MOVE_PAT_TUNNEL` (Blinky tunnel speed mask)

### State, Level & Scoring Variables
- **`0x4D9F`:** `EATEN_PILLS_COUNT` (Total dots eaten in current level)
- **`0x4DA6`:** `PACMAN_POWEREDUP` (Flag indicating Pac-Man is powered up by a power pill)
- **`0x4DA7 - 0x4DAA`:** Edible states for Blinky, Pinky, Inky, Clyde respectively (`1` if blue/edible, `0` otherwise)
- **`0x4DAB`:** `GHOST_STATE` (General ghost state: scatter, chase, frightened)
- **`0x4DAC - 0x4DAF`:** Individual states for Blinky, Pinky, Inky, Clyde respectively
- **`0x4DB6`:** `CRUISE_ELROY_MODE_1` (Blinky fast chase mode trigger 1)
- **`0x4DB7`:** `CRUISE_ELROY_MODE_2` (Blinky ultra-fast chase mode trigger 2)
- **`0x4DBD - 0x4DBE`:** `GHOST_EDIBLE_TIME` (2-byte tick counter remaining for frightened ghost state)
- **`0x4DBF`:** `PACMAN_IN_TUNNEL` (1 byte)
- **`0x4DC0`:** `GHOST_ANIMATION` (1 byte)
- **`0x4DC1`:** `NONRANDOM_MOVEMENT` (1 byte)
- **`0x4DC2`:** `ORIENTATION_CHANGE_COUNT` (2 bytes)
- **`0x4DC4`:** `GHOST_ANIMATION_COUNTER` (1 byte)
- **`0x4DC5`:** `COUNT_SINCE_PAC_KILLED` (2 bytes)
- **`0x4DC7`:** `TRIAL_ORIENTATION` (1 byte)
- **`0x4DC8`:** `GHOST_COL_POWERUP_COUNTER` (1 byte)
- **`0x4DC9`:** `RND_VAL_PTR` (2 bytes)
- **`0x4DCB`:** `EDIBLE_REMAIN_COUNT` (2 bytes)
- **`0x4DCE`:** `COIN_TIMER` (1 byte)
- **`0x4DCF`:** `PILL_CHANGE_COUNTER` (1 byte)
- **`0x4DD0`:** `KILLED_COUNT` (1 byte)
- **`0x4DD1`:** `KILLED_STATE` (1 byte)
- **`0x4DD2`:** `FRUIT_POS` (current bonus fruit display tile position)
- **`0x4DD4`:** `FRUIT_POINTS` (point value for current level fruit)
- **`0x4E00`:** `MAIN_STATE` (overall game state)
- **`0x4E03`:** `CREDIT_STATE` (coin credits counter check)
- **`0x4E04`:** `LEVEL_STATE` (level transition controller)
- **`0x4E09`:** `PLAYER` (currently active player: `0` = Player 1, `1` = Player 2)

#### Player 1 Statistics Block (at `0x4E0A`)
- **`0x4E13`:** `P1_LEVEL` (Player 1 current level number)
- **`0x4E14`:** `P1_REAL_LIVES` (Player 1 remaining life count)
- **`0x4E16 - 0x4E33`:** `P1_PILL_ARRAY` (19 bytes = 152 bits representing dot coordinates on the maze)
- **`0x4E80 - 0x4E83`:** `P1_SCORE` (Player 1 Score in 4-byte packed BCD format)

#### Player 2 Statistics Block (at `0x4E38`)
- **`0x4E38`:** `P2_CURR_DIFFICULTY` (2 bytes)
- **`0x4E41`:** `P2_LEVEL` (Player 2 current level number)
- **`0x4E42`:** `P2_REAL_LIVES` (Player 2 remaining life count)
- **`0x4E44 - 0x4E61`:** `P2_PILL_ARRAY` (19-byte active dots grid for Player 2)
- **`0x4E84 - 0x4E87`:** `P2_SCORE` (Player 2 Score in 4-byte packed BCD format)

- **`0x4E88 - 0x4E8B`:** `HIGH_SCORE` (Current Session High Score in 4-byte packed BCD format)

---

## 10. Memory-Mapped I/O Registers (`0x5000 - 0x50FF`)

The memory space between `0x5000` and `0x50FF` maps directly to hardware ports for handling user inputs, board switches, interrupts, display transformations, and sound synthesis commands:

### Read Ports (Input & Configuration)

#### A. Input Port 0 (`0x5000 - 0x503F`)
Commonly read at address `0x5000` (replicated every 4 bytes). Represents buttons and switches connected to Cabinet Board:
* **Bit 0 (`0x01`):** Player 1 Up
* **Bit 1 (`0x02`):** Player 1 Left
* **Bit 2 (`0x04`):** Player 1 Right
* **Bit 3 (`0x08`):** Player 1 Down
* **Bit 4 (`0x10`):** Rack Advance / Service Test Button (jump level)
* **Bit 5 (`0x20`):** Coin 1 Trigger
* **Bit 6 (`0x40`):** Coin 2 Trigger
* **Bit 7 (`0x80`):** Service Button (Manual credit)

#### B. Input Port 1 (`0x5040 - 0x507F`)
Commonly read at address `0x5040`. Represents player start buttons and cocktail-mode joystick inputs:
* **Bit 0 (`0x01`):** Player 2 Up (Active in cocktail cabinet)
* **Bit 1 (`0x02`):** Player 2 Left
* **Bit 2 (`0x04`):** Player 2 Right
* **Bit 3 (`0x08`):** Player 2 Down
* **Bit 4 (`0x10`):** Service Switch (Cabinet open trigger)
* **Bit 5 (`0x20`):** Player 1 Start Button
* **Bit 6 (`0x40`):** Player 2 Start Button
* **Bit 7 (`0x80`):** Cabinet Mode (`1` = Cocktail Cabinet, `0` = Upright Cabinet)

#### C. Dip Switches (`0x5080 - 0x50BF`)
Commonly read at address `0x5080`. Board-level switches configured by arcade operator:
* **Bits 0–1 (`0x03`):** Coinage Settings:
  * `00` (0) = Free Play
  * `01` (1) = 1 Coin / 1 Credit
  * `10` (2) = 1 Coin / 2 Credits
  * `11` (3) = 2 Coins / 1 Credit
* **Bits 2–3 (`0x0C`):** Initial Player Lives:
  * `00` (0) = 1 Life
  * `01` (1) = 2 Lives
  * `10` (2) = 3 Lives (default)
  * `11` (3) = 5 Lives
* **Bits 4–5 (`0x30`):** Extra Life Score threshold:
  * `00` (0) = 10,000 points
  * `01` (1) = 15,000 points
  * `10` (2) = 20,000 points
  * `11` (3) = Disabled
* **Bit 6 (`0x40`):** Gameplay Difficulty (`0` = Hard, `1` = Normal)
* **Bit 7 (`0x80`):** Ghost Names Set (`0` = Alternate names, `1` = Normal names)

---

### Write Ports (Hardware Control)

#### A. Direct Control Lines
* **`0x5000` (Write):** **Interrupt Enable Toggle** — Writing `1` enables Z80 CPU VBlank interrupts (60Hz clock), `0` disables them.
* **`0x5001` (Write):** **Sound Hardware Enable** — Writing `1` enables WSG audio output speaker stage, `0` mutes all sound.
* **`0x5003` (Write):** **Video Screen Flip Toggle** — Writing `1` rotates the entire monitor layout 180 degrees (used in cocktail mode for player 2).

#### B. Namco WSG (Waveform Sound Generator) Registers (`0x5040 - 0x505F`)
Controls three independent voice generators utilizing raw waveform shapes from sound PROMs:

| Voice Channel | Phase Accumulator / Counter (Read/Write) | Waveform Index Select (Bits 0-2) | Pitch Frequency (20 or 16 bits) | Output Volume (Bits 0-3) |
| :---: | :---: | :---: | :---: | :---: |
| **Voice 1** | `0x5040 - 0x5044` (5 nibbles) | `0x5045` | `0x5050 - 0x5054` (20-bit) | `0x5055` (0 to 15) |
| **Voice 2** | `0x5046 - 0x5049` (4 nibbles) | `0x504A` | `0x5056 - 0x5059` (16-bit) | `0x505A` (0 to 15) |
| **Voice 3** | `0x504B - 0x504E` (4 nibbles) | `0x504F` | `0x505B - 0x505E` (16-bit) | `0x505F` (0 to 15) |

*Note: The pitch frequencies are written as 4-bit nibbles, least significant nibble first.*

#### C. Sprite Coordinates (`0x5060 - 0x506F`)
Specifies the screen position coordinates for the 8 active sprites:
- **`0x5060 + i * 2`:** X screen pixel position for sprite `i` (0 to 7)
- **`0x5060 + i * 2 + 1`:** Y screen pixel position for sprite `i` (0 to 7)

#### D. Watchdog Reset (`0x50C0 - 0x50FF`)
Writing any value to this register range kicks/resets the physical hardware watchdog timer circuit. Prevents game locking by automatically resetting the CPU if the game loop fails to execute within the threshold.

---

## 11. Pac-Man Z80 CPU Address Space & ROM Mapping Addresses

The original arcade game runs on an 8-bit Z80 microprocessor addressing up to 64 KB of memory space. Program code is mapped directly into CPU memory, whereas graphics and sound registers control physical custom hardware connected to their respective dedicated daughterboards.

### Memory Map Table

| Z80 CPU Address Range | Mapped Component | Component Size | Directly Mapped ROM Files |
| :--- | :--- | :---: | :--- |
| **`0x0000 - 0x0FFF`** | Game Code ROM 0 | 4 KB | `pacman.6e` (Program ROM) |
| **`0x1000 - 0x1FFF`** | Game Code ROM 1 | 4 KB | `pacman.6f` (Program ROM) |
| **`0x2000 - 0x2FFF`** | Game Code ROM 2 | 4 KB | `pacman.6h` (Program ROM) |
| **`0x3000 - 0x3FFF`** | Game Code ROM 3 | 4 KB | `pacman.6j` (Program ROM) |
| **`0x4000 - 0x43FF`** | Video RAM (Tilemap Indices) | 1 KB | *None (Indexed to `pacman.5e` by video hardware)* |
| **`0x4400 - 0x47FF`** | Color attribute RAM | 1 KB | *None (Indexed to `82s123.7f` by video hardware)* |
| **`0x4800 - 0x4FEF`** | CPU Work RAM | ~2 KB | *None* |
| **`0x4FF0 - 0x4FFF`** | Sprite Attribute Memory (OAM) | 16 Bytes | *None (Indexed to `pacman.5f` by sprite hardware)* |
| **`0x5000 - 0x503F`** | Input Registers 0 (Read) | 64 Bytes | *None (Joystick buttons / Coin doors)* |
| **`0x5040 - 0x507F`** | Input Registers 1 (Read) | 64 Bytes | *None (Start buttons / Switches)* |
| **`0x5080 - 0x50BF`** | Dip Switches (Read) | 64 Bytes | *None (Board Configuration Switches)* |
| **`0x5000`** (Write)  | Interrupt Enable Toggle | 1 Byte | *None* |
| **`0x5001`** (Write)  | Sound Hardware Enable | 1 Byte | *None* |
| **`0x5003`** (Write)  | Video Screen Flip Toggle | 1 Byte | *None* |
| **`0x5040 - 0x505F`** (Write) | Custom Sound WSG Registers | 32 Bytes | *None (Registers select voice waves from `82s126.1m` & `126.3m`)* |
| **`0x5060 - 0x506F`** (Write) | Sprite Coordinates (X, Y Positions) | 16 Bytes | *None* |
| **`0x50C0 - 0x50FF`** (Write) | Watchdog Reset registers | 64 Bytes | *None* |

### Hardware ROMs Not Directly Mapped to Z80 Space
Certain graphics and sound PROMs are connected directly to separate bus lanes for custom hardware processors:
1. **Character ROM (`pacman.5e`):** Mapped inside the video controller board. The video processor reads the VRAM coordinates (`0x4000 - 0x43FF`) to fetch the tile indices and draws pixels from this ROM.
2. **Sprite ROM (`pacman.5f`):** Mapped inside the sprite controller hardware. The hardware reads the active sprite attribute table (`0x4FF0 - 0x4FFF`) and coordinate registers (`0x5060 - 0x506F`) to draw moving sprite quadrants.
3. **Color Palette PROM (`82s123.7f`):** Wired directly into the final video DAC output stage to map the digital indexes into analog RGB colors.
4. **Color Lookup Table PROM (`82s126.4a`):** Read by the video board to resolve 4-color palettes for each text tile or sprite drawing step.
5. **Sound Wave PROMs (`82s126.1m` / `82s126.3m`):** Wired directly to the Namco custom Waveform Sound Generator (WSG) hardware. The Z80 selects the waveform index and details via the sound register block (`0x5040 - 0x505F`).
