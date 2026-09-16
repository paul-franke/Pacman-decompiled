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
 */

#ifndef __DATA_H
#define __DATA_H

#include "memmap.h"

#define DATA_0219 (&ROM[0x0219])

extern const uint8_t LEVEL_DIFFICULTY_PARAMS[21][6]; // #define DATA_0796 (&ROM[0x0796])
extern const uint8_t LEAVE_HOME_COUNTERS[4][3]; // #define DATA_0843 (&ROM[0x0843])
extern const uint8_t CRUISE_ELROY_THRESHOLDS[9][2]; // #define DATA_084f (&ROM[0x084f])
extern const uint16_t GHOST_EDIBLE_TIMES[9]; // #define DATA_0861 (&ROM[0x0861])
extern const uint16_t GLOBAL_LEAVE_HOME_TIMERS[3]; // #define DATA_0873 (&ROM[0x0873])
extern const FruitData FRUIT_DATA_0efd[21]; // #define DATA_0efd (&ROM[0x0efd])
extern const uint8_t BONUS_LIFE_DATA[4]; //#define BONUS_LIFE_DATA (&ROM[0x2728])
extern const uint16_t DIFFICULTY_DATA[2]; // #define DIFFICULTY_DATA (&ROM[0x272c])
extern YXPOS MOVE_VECTOR_DATA[8]; //#define MOVE_VECTOR_DATA (&ROM[0x32ff])
#define MOVE_VECTOR_RIGHT ((YXPOS*)(&MOVE_VECTOR_DATA[0])) 
#define MOVE_VECTOR_DOWN ((YXPOS*)(&MOVE_VECTOR_DATA[1]))
#define MOVE_VECTOR_LEFT ((YXPOS*)(&MOVE_VECTOR_DATA[2]))
#define MOVE_VECTOR_UP ((YXPOS*)(&MOVE_VECTOR_DATA[3]))
extern const uint8_t MOVE_DATA_BLOCKS[7][42]; // #define MOVE_DATA_330f (&ROM[0x330f])
extern const uint8_t MAZE_DRAW_DATA[384]; //#define DATA_3435 &ROM[0x3435]
extern const uint8_t PILLS_DELTA_ADDRESS_ENCODING[240]; // #define DATA_35b5 (&ROM[0x35b5])
#define EFFECT_TABLE_CH3_3b80 (&ROM[0x3b80])
#define EFFECT_TABLE_CH1_3b30 (&ROM[0x3b30])
#define EFFECT_TABLE_CH2_3b40 (&ROM[0x3b40])
#define FRUIT_TABLE (&ROM[0x3b08])
//#define DATA_MSG_TABLE_36a5 (&ROM[0x36a5])
#define SONG_TABLE_CH1_3bc8 (&ROM[0x3bc8])
#define POWER_OF_2_3bb0 (&ROM[0x3bb0])
#define FREQ_TABLE_3bb8 (&ROM[0x3bb8])
#define SONG_TABLE_CH2_3bcc (&ROM[0x3bcc])
#define SONG_TABLE_CH3_3bd0 (&ROM[0x3bd0])
#define DATA_3154 ((uint16_t *)(&ROM[0x3154]))
#define BAD_ROM_316c (&ROM[0x316c])
#define BAD_W_RAM_316e (&ROM[0x316e])
#define BAD_V_RAM_3170 (&ROM[0x3170])
#define BAD_C_RAM_3172 (&ROM[0x3172])
#define DATA_32f9 (&ROM[0x32f9])
#define DATA_3ae2 ((uint16_t *)(&ROM[0x3ae2]))

extern const uint8_t* msgTable_36a5[55];
extern const uint8_t msg_3713[];
extern const uint8_t msg_3723[]; // 1	   CREDIT
extern const uint8_t msg_3732[]; // 2	   FREE PLAY
extern const uint8_t msg_3741[]; // 3      PLAYER ONE
extern const uint8_t msg_375a[]; // 4      PLAYER TWO
extern const uint8_t msg_376a[]; // 5      GAME  OVER
extern const uint8_t msg_377a[]; // 6      READY?
extern const uint8_t msg_3786[]; // 7      PUSH START BUTTON
extern const uint8_t msg_379d[]; // 8      1 PLAYER ONLY
extern const uint8_t msg_37b1[]; // 9      1 OR 2 PLAYERS
extern const uint8_t msg_3d00[]; // a      BONUS PAC-MAN FOR   000 Pts
extern const uint8_t msg_3d21[]; // b      @ 1980 MIDWAY MFG.CO.
extern const uint8_t msg_37fd[]; // c      CHARACTER / NICKNAME
extern const uint8_t msg_3d67[]; // d      "BLINKY"
extern const uint8_t msg_3de3[]; // e      "BBBBBBBB"
extern const uint8_t msg_3d86[]; // f      "PINKY"
extern const uint8_t msg_3e02[]; //10      "DDDDDDDD"
extern const uint8_t msg_384c[]; //11      . 10 Pts
extern const uint8_t msg_385a[]; //12      o 50 Pts
extern const uint8_t msg_3d3c[]; //13      @ 1980 MIDWAY MFG.CO.
extern const uint8_t msg_3d57[]; //14      -SHADOW
extern const uint8_t msg_3dd3[]; //15      "AAAAAAAA"
extern const uint8_t msg_3d76[]; //16      -SPEEDY
extern const uint8_t msg_3df2[]; //17      "CCCCCCCC"
extern const uint8_t msg_EMPTY[]; //18,19,1a ----
extern const uint8_t msg_38bc[]; //1b      100
extern const uint8_t msg_38c4[]; //1c      300
extern const uint8_t msg_38ce[]; //1d      500
extern const uint8_t msg_38d8[]; //1e      700
extern const uint8_t msg_38e2[]; //1f      1000
extern const uint8_t msg_38ec[]; //20      2000
extern const uint8_t msg_38f6[]; //21      3000
extern const uint8_t msg_3900[]; //22      5000
extern const uint8_t msg_390a[]; //23      MEMORY  OK
extern const uint8_t msg_391a[]; //24      BAD    R M
extern const uint8_t msg_396f[]; //25      FREE  PLAY
extern const uint8_t msg_392a[]; //26      1 COIN  1 CREDIT
extern const uint8_t msg_3958[]; //27      1 COIN  2 CREDITS
extern const uint8_t msg_3941[]; //28      2 COINS 1 CREDIT
extern const uint8_t msg_3e4f[]; //29      PAC-MAN
extern const uint8_t msg_3986[]; //2a      BONUS  NONE
extern const uint8_t msg_3997[]; //2b      BONUS
extern const uint8_t msg_39b0[]; //2c      TABLE
extern const uint8_t msg_39bd[]; //2d      UPRIGHT
extern const uint8_t msg_39ca[]; //2e      000
extern const uint8_t msg_3da5[]; //2f      "INKY"
extern const uint8_t msg_3e21[]; //30      "FFFFFFFF"
extern const uint8_t msg_3dc4[]; //31      "CLYDE"
extern const uint8_t msg_3e40[]; //32      "HHHHHHHH"
extern const uint8_t msg_3d95[]; //33      -BASHFUL
extern const uint8_t msg_3e11[]; //34      "EEEEEEEE"
extern const uint8_t msg_3db4[]; //35      -POKEY
extern const uint8_t msg_3e30[]; //36      "GGGGGGGG"

#endif
