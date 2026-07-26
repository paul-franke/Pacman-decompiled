/*
 * Copyright (c) 2024 Mark Burkley.
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 *
 * Some support functions for Z80 CPU
 */

#ifdef __TINYC__
#define _mm_pause()
#endif

#include <stdio.h>
#include <stdlib.h>
#include <stdbool.h>
#include <unistd.h>

#include "video.h"
#include "kbd.h"
#include "memmap.h"
#include "structs.h"

static void (*intVector) (void);
bool cpuPaused;

void interruptEnable ()
{
}

void interruptDisable()
{
}

void interruptMode (int mode)
{
}

#ifdef _WIN32
#include <windows.h>
static void limit_frame_rate(double target_fps)
{
    static LARGE_INTEGER frequency;
    static LARGE_INTEGER last_time;
    static bool first = true;
    
    if (first) {
        QueryPerformanceFrequency(&frequency);
        QueryPerformanceCounter(&last_time);
        first = false;
        return;
    }
    
    double target_frame_time = 1.0 / target_fps;
    LARGE_INTEGER current_time;
    while (1) {
        QueryPerformanceCounter(&current_time);
        double elapsed = (double)(current_time.QuadPart - last_time.QuadPart) / frequency.QuadPart;
        double remaining = target_frame_time - elapsed;
        if (remaining <= 0.0) {
            break;
        }
        if (remaining > 0.001) {
            Sleep((DWORD)(remaining * 1000.0 - 0.5));
        } else {
            YieldProcessor();
        }
    }
    QueryPerformanceCounter(&last_time);
}
#endif

double target_fps = 60.0;

void interruptHalt (void)
{
    static bool inInterrupt;

    /*  TODO pend on cond var set by video or use a timer */
#ifdef _WIN32
    limit_frame_rate(target_fps);
#else
    usleep((useconds_t)(1000000.0 / target_fps));
#endif

    if (!inInterrupt && !cpuPaused && intVector)
    {
        inInterrupt = true;
        intVector();
    }
    inInterrupt = false;

#if 0
    printf ("sprite-at: ");
    for (int i = 0; i < 16; i++)
        printf ("%02x ", SPRITEATTRIB[i]);
    printf ("\nsprite-co: ");
    for (int i = 0; i < 16; i++)
        printf ("%02x ", SPRITECOORDS[i]);
    printf ("\nstmain=%d in=%d dem=%d cr=%d lv=%d s1=%d s2=%d s3=%d",
            MAIN_STATE, RESET_STATE, 
            INTRO_STATE, CREDIT_STATE, 
            LEVEL_STATE,
            SCENE1_STATE,
            SCENE2_STATE,
            SCENE3_STATE);
    printf (" ghost=[%d,%d,%d,%d]",
            BLINKY_STATE,
            PINKY_STATE,
            INKY_STATE,
            CLYDE_STATE);
    printf (" sub=[%d,%d,%d,%d]",
            BLINKY_SUBSTATE,
            PINKY_SUBSTATE,
            INKY_SUBSTATE,
            CLYDE_SUBSTATE);
    printf (" p1l=%d p2l=%d two=%d\n",
            P1_REAL_LIVES, P2_REAL_LIVES, TWO_PLAYERS);
    printf ("pacman-tile %d,%d blinky-tile %d,%d\n",
            PACMAN_TILE2.x,
            PACMAN_TILE2.y,
            BLINKY_TILE2.x,
            BLINKY_TILE2.y);
    printf ("\n\n=====\n");
#endif
}

void interruptVector (void (*func) (void))
{
    intVector = func;
}

void kickWatchdog (void)
{
// usleep(50000);
}
