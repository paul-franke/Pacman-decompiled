/*
 * sound_player.c - Pac-Man Sound Effects & Songs Interactive Player
 *
 * Uses the authentic Pac-Man sound engine, 1/60th second (60 Hz) frame timing,
 * and Windows winmm waveOut hardware synthesis.
 * Prints real-time sound parameters for Voice 1, Voice 2, and Voice 3 on every frame tick.
 */

#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <stdbool.h>
#include <math.h>

#ifdef _WIN32
#include <windows.h>
#include <mmsystem.h>
#ifdef _MSC_VER
#pragma comment(lib, "winmm.lib")
#endif
#else
#include <unistd.h>
#endif

#include "memmap.h"
#include "structs.h"
#include "sound.h"
#include "video.h"

// ROM Header inclusions
#include "pacman.5e.h"
#include "pacman.5f.h"
#include "pacman.6e.h"
#include "pacman.6f.h"
#include "pacman.6h.h"
#include "pacman.6j.h"

// Memmap declaration
CPU_MEMMAP memmap;
uint8_t input0 = 0xEF;
uint8_t input1 = 0x6F;
uint8_t dipSwitches = 0xC9;

// External declarations from pacman.c & sound.c
void soundEffectsAllChannels_2d0c(void);
void playSongsAllChannels_2cc1(void);
void soundInit(void);
void soundClose(void);

// Dummy stubs for video/keyboard/cpu symbols required by pacman.c when building standalone audio player
void videoPlot(unsigned x, unsigned y, pixel p) { (void)x; (void)y; (void)p; }
void videoDrawChar(unsigned cx, unsigned cy, int chr, int chrCol) { (void)cx; (void)cy; (void)chr; (void)chrCol; }
void videoDrawSprite(unsigned px, unsigned py, int shape, int mode, int colour) { (void)px; (void)py; (void)shape; (void)mode; (void)colour; }
pixel videoColourLookup(uint8_t col) { (void)col; pixel p = {0,0,0}; return p; }
void videoInit(int x, int y, int scale) { (void)x; (void)y; (void)scale; }
void keyboardInit(void) {}
void keyboardUpdate(void) {}
void interruptDisable(void) {}
void interruptEnable(void) {}
void kickWatchdog(void) {}
void interruptMode(int m) { (void)m; }
void interruptVector(void (*func)(void)) { (void)func; }
void interruptHalt(void) {}
void showTarget(XYPOS a, XYPOS b, int col) { (void)a; (void)b; (void)col; }




// Sound Item Definition
typedef struct {
    int id;
    const char *name;
    const char *category;
    int channelMask;
    int channelIndex; // 1, 2, 3
    uint8_t maskValue;
    bool isSong;
    int durationFrames;
} SoundItem;

static SoundItem soundCatalog[] = {
    // Songs
    { 1, "Game Start Intro Theme (3 Channels)", "Songs", 0, 0, 0x01, true, 260 },
    { 2, "Intermission / Coffee Break Theme", "Songs", 0, 3, 0x10, false, 320 },

    // Channel 1 Effects
    { 3, "Waka-Waka / Eating Dot", "Ch1 Effects", 1, 1, 0x02, false, 90 },
    { 4, "Siren 1 (Slow Siren - Default)", "Ch1 Effects", 1, 1, 0x01, false, 180 },

    // Channel 2 Effects
    { 5, "Siren 1 (Ch2 Siren)", "Ch2 Effects", 2, 2, 0x01, false, 180 },
    { 6, "Siren 2 (Medium Siren)", "Ch2 Effects", 2, 2, 0x02, false, 180 },
    { 7, "Siren 3 (Fast Siren)", "Ch2 Effects", 2, 2, 0x04, false, 180 },
    { 8, "Siren 4 (Faster Siren)", "Ch2 Effects", 2, 2, 0x08, false, 180 },
    { 9, "Siren 5 (Cruise Elroy / Fastest)", "Ch2 Effects", 2, 2, 0x10, false, 180 },
    { 10, "Frightened Ghosts Siren (Blue Ghost)", "Ch2 Effects", 2, 2, 0x20, false, 180 },
    { 11, "Eyes Returning to Ghost House", "Ch2 Effects", 2, 2, 0x40, false, 180 },
    { 12, "Extra Life / Bonus Pac-Man Sound", "Ch2 Effects", 2, 2, 0x80, false, 120 },

    // Channel 3 Effects
    { 13, "Pac-Man Death / Dying Crumple", "Ch3 Effects", 3, 3, 0x01, false, 150 },
    { 14, "Ghost Eaten (Points 200-1600)", "Ch3 Effects", 3, 3, 0x02, false, 60 },
    { 15, "Bonus Fruit Eaten (Points 100-5000)", "Ch3 Effects", 3, 3, 0x04, false, 60 },
    { 16, "Credit Inserted (Coin Sound)", "Ch3 Effects", 3, 3, 0x08, false, 80 },
};

static const int catalogSize = sizeof(soundCatalog) / sizeof(soundCatalog[0]);

// Stop all playing sounds and clear RAM sound registers
static void stopAllSounds(void)
{
    memset(&memmap.mem[0x4e80], 0, 0x100);
    memset(SOUND, 0, 0x20);
}

// Print formatted sound parameters for Voice 1, Voice 2, and Voice 3
static void printSoundParameters(int frameIndex, double timeSeconds)
{
    // Voice 1 (20-bit frequency)
    uint32_t f1 = SOUND[0x10] | (SOUND[0x11] << 4) | (SOUND[0x12] << 8) | (SOUND[0x13] << 12) | ((SOUND[0x14] & 0x0f) << 16);
    uint8_t w1 = SOUND[0x05] & 0x0f;
    uint8_t v1 = SOUND[0x15] & 0x0f;
    double hz1 = (double)f1 * 3000.0 / 1048576.0;

    // Voice 2 (16-bit frequency)
    uint32_t f2 = SOUND[0x16] | (SOUND[0x17] << 4) | (SOUND[0x18] << 8) | ((SOUND[0x19] & 0x0f) << 12);
    uint8_t w2 = SOUND[0x0a] & 0x0f;
    uint8_t v2 = SOUND[0x1a] & 0x0f;
    double hz2 = (double)f2 * 3000.0 / 65536.0;

    // Voice 3 (16-bit frequency)
    uint32_t f3 = SOUND[0x1b] | (SOUND[0x1c] << 4) | (SOUND[0x1d] << 8) | ((SOUND[0x1e] & 0x0f) << 12);
    uint8_t w3 = SOUND[0x0f] & 0x0f;
    uint8_t v3 = SOUND[0x1f] & 0x0f;
    double hz3 = (double)f3 * 3000.0 / 65536.0;

    printf("[Frame %04d | %5.2fs] V1: Wave=%d Vol=%2d Freq=0x%05X (%6.1f Hz) | V2: Wave=%d Vol=%2d Freq=0x%04X (%6.1f Hz) | V3: Wave=%d Vol=%2d Freq=0x%04X (%6.1f Hz)\n",
           frameIndex, timeSeconds,
           w1, v1, f1, hz1,
           w2, v2, f2, hz2,
           w3, v3, f3, hz3);
}

// Play a selected sound item using 1/60 frame rate logic
static void playSoundItem(SoundItem *item)
{
    stopAllSounds();

    printf("\n========================================================================================================\n");
    printf(" PLAYING [%2d]: %s (%s)\n", item->id, item->name, item->category);
    printf(" Frame Rate: 60.0 Hz (16.66 ms per tick) | Hardware Engine: winmm (waveOut @ 96 kHz)\n");
    printf(" Duration: %d frames (%.2f seconds)\n", item->durationFrames, item->durationFrames / 60.0);
    printf("========================================================================================================\n");

    // Trigger sound/song mask
    if (item->isSong)
    {
        CH1_SOUND_WAVE->mask = 1;
        CH2_SOUND_WAVE->mask = 1;
        CH3_SOUND_WAVE->mask = 1;
        CH1_SOUND_WAVE->selected = 0;
        CH2_SOUND_WAVE->selected = 0;
        CH3_SOUND_WAVE->selected = 0;
    }
    else
    {
        if (item->channelIndex == 1)
            CH1_SOUND_EFFECT->mask = item->maskValue;
        else if (item->channelIndex == 2)
            CH2_SOUND_EFFECT->mask = item->maskValue;
        else if (item->channelIndex == 3)
            CH3_SOUND_EFFECT->mask = item->maskValue;
    }

    // Run 1/60th second frame rate simulation loop
    for (int frame = 0; frame < item->durationFrames; frame++)
    {
        // Increment global sound counter tick
        (*SOUND_COUNTER)++;

        // 1. Execute authentic Pac-Man sound update subroutines
        soundEffectsAllChannels_2d0c();
        playSongsAllChannels_2cc1();

        // 2. Map RAM frequency/volume output to hardware SOUND registers
        memcpy(&SOUND[0x10], &CH1_FREQ0, 0x10);

        uint8_t w1 = (CH1_SOUND_WAVE->mask != 0) ? CH1_SOUND_WAVE->selected : CH1_SOUND_EFFECT->selected;
        uint8_t w2 = (CH2_SOUND_WAVE->mask != 0) ? CH2_SOUND_WAVE->selected : CH2_SOUND_EFFECT->selected;
        uint8_t w3 = (CH3_SOUND_WAVE->mask != 0) ? CH3_SOUND_WAVE->selected : CH3_SOUND_EFFECT->selected;

        SOUND[0x05] = w1;
        SOUND[0x0a] = w2;
        SOUND[0x0f] = w3;

        // 3. Print current sound parameters for each frame
        printSoundParameters(frame, frame / 60.0);

        // 4. Maintain 1/60 second timing (16.666 ms per frame)
#ifdef _WIN32
        Sleep(16);
#else
        usleep(16666);
#endif
    }

    stopAllSounds();
    printf("--- Finished: %s ---\n\n", item->name);
}

static void printMenu(void)
{
    printf("\n========================================================================================================\n");
    printf("                       Pac-Man Authentic Sound & Music Player\n");
    printf("                       Synthesizer Engine: WinMM waveOut (96kHz)\n");
    printf("========================================================================================================\n");
    printf("   [1] Game Start Intro Theme (3-Channel Song)\n");
    printf("   [2] Intermission / Coffee Break Theme\n\n");

    printf("   --- Channel 1 Sound Effects ---\n");
    printf("   [3] Waka-Waka / Eating Dot\n");
    printf("   [4] Siren 1 (Slow Background Siren)\n\n");

    printf("   --- Channel 2 Sound Effects ---\n");
    printf("   [5] Siren 1 (Ch2 Siren)\n");
    printf("   [6] Siren 2 (Medium Siren)\n");
    printf("   [7] Siren 3 (Fast Siren)\n");
    printf("   [8] Siren 4 (Faster Siren)\n");
    printf("   [9] Siren 5 (Cruise Elroy / Fastest Siren)\n");
    printf("  [10] Frightened Ghosts Siren (Blue Ghost Wawa)\n");
    printf("  [11] Eyes Returning to Ghost House\n");
    printf("  [12] Extra Life / Bonus Sound\n\n");

    printf("   --- Channel 3 Sound Effects ---\n");
    printf("  [13] Pac-Man Death / Dying Crumple Animation\n");
    printf("  [14] Ghost Eaten (200 / 400 / 800 / 1600 Pts)\n");
    printf("  [15] Bonus Fruit Eaten (100-5000 Pts)\n");
    printf("  [16] Credit Inserted (Coin Sound)\n\n");

    printf("  [17] PLAY ALL SOUNDS & SONGS SEQUENTIALLY (Jukebox Mode)\n");
    printf("   [0] Quit Program\n");
    printf("========================================================================================================\n");
    printf("Select a sound option (0-17): ");
}

int main(int argc, char *argv[])
{
    printf("Initializing Pac-Man Sound System...\n");

    // Load ROMs into memory space
    memcpy(&ROM[0x0000], rom_pacman_6e, 0x1000);
    memcpy(&ROM[0x1000], rom_pacman_6f, 0x1000);
    memcpy(&ROM[0x2000], rom_pacman_6h, 0x1000);
    memcpy(&ROM[0x3000], rom_pacman_6j, 0x1000);

    // Initialize WinMM waveOut sound hardware driver
    soundInit();
    printf("Sound driver initialized (winmm waveOut active at 96,000 Hz).\n");

    // Check command line arguments for non-interactive auto-play
    if (argc > 1)
    {
        int choice = atoi(argv[1]);
        if (choice == 17 || strcmp(argv[1], "all") == 0)
        {
            printf("Running Jukebox Mode: Playing all sounds sequentially...\n");
            for (int i = 0; i < catalogSize; i++)
            {
                playSoundItem(&soundCatalog[i]);
#ifdef _WIN32
                Sleep(300);
#else
                usleep(300000);
#endif
            }
        }
        else if (choice >= 1 && choice <= catalogSize)
        {
            playSoundItem(&soundCatalog[choice - 1]);
        }

        soundClose();
        return 0;
    }

    // Interactive Menu Loop
    char inputBuf[64];
    while (true)
    {
        printMenu();
        if (!fgets(inputBuf, sizeof(inputBuf), stdin))
            break;

        int choice = atoi(inputBuf);
        if (choice == 0 && (inputBuf[0] == '0' || inputBuf[0] == 'q' || inputBuf[0] == 'Q'))
        {
            printf("Exiting sound player. Goodbye!\n");
            break;
        }

        if (choice == 17)
        {
            printf("\n--- Starting Jukebox Mode: Playing all %d sounds & songs ---\n", catalogSize);
            for (int i = 0; i < catalogSize; i++)
            {
                playSoundItem(&soundCatalog[i]);
#ifdef _WIN32
                Sleep(300);
#else
                usleep(300000);
#endif
            }
        }
        else if (choice >= 1 && choice <= catalogSize)
        {
            playSoundItem(&soundCatalog[choice - 1]);
        }
        else
        {
            printf("Invalid selection! Please enter a number between 0 and 17.\n");
        }
    }

    soundClose();
    return 0;
}
