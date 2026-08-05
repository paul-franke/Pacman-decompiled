using System;
using System.Runtime.InteropServices;

namespace PacmanCS
{
    public static unsafe class MemMap
    {
        public static readonly byte[] memArray = new byte[0x5100];
        public static readonly byte[] charsetArray = new byte[0x2000];

        public static byte input0;
        public static byte input1;
        public static byte dipSwitches;

        private static GCHandle memHandle;
        private static GCHandle charsetHandle;

        public static byte* MEM { get; private set; }
        public static byte* charset { get; private set; }

        static MemMap()
        {
            memHandle = GCHandle.Alloc(memArray, GCHandleType.Pinned);
            charsetHandle = GCHandle.Alloc(charsetArray, GCHandleType.Pinned);

            MEM = (byte*)memHandle.AddrOfPinnedObject();
            charset = (byte*)charsetHandle.AddrOfPinnedObject();
        }

        public static byte* ROM => MEM + 0x0000;
        public static byte* SCREEN => MEM + 0x4000;
        public static byte* COLOUR => MEM + 0x4400;
        public static byte* RAM => MEM + 0x4800;
        public static byte* SPRITEATTRIB => MEM + 0x4ff0;
        public static byte* SOUND => MEM + 0x5040;
        public static byte* SPRITECOORDS => MEM + 0x5060;

        public static ref byte INTENABLE => ref MEM[0x5000];
        public static ref byte SOUNDENABLE => ref MEM[0x5001];
        public static ref byte AUXENABLE => ref MEM[0x5002];
        public static ref byte FLIPSCREEN => ref MEM[0x5003];
        public static ref byte P1START => ref MEM[0x5004];
        public static ref byte P2START => ref MEM[0x5005];
        public static ref byte COINLOCKOUT => ref MEM[0x5006];
        public static ref byte COINCOUNTER => ref MEM[0x5007];
        public static ref byte REGSWRITE => ref MEM[0x5040];

        public static ref byte IO_INPUT0 => ref input0;
        public static ref byte IO_INPUT1 => ref input1;
        public static ref byte DIP_INPUT => ref dipSwitches;

        public const byte INPUT_UP = 0x01;
        public const byte INPUT_LEFT = 0x02;
        public const byte INPUT_RIGHT = 0x04;
        public const byte INPUT_DOWN = 0x08;
        public const byte INPUT_ANYCOIN = 0xe0;
        public const byte INPUT_ANYSTART = 0x60;

        public static int IN0_UP => IO_INPUT0 & INPUT_UP;
        public static int IN0_LEFT => IO_INPUT0 & INPUT_LEFT;
        public static int IN0_RIGHT => IO_INPUT0 & INPUT_RIGHT;
        public static int IN0_DOWN => IO_INPUT0 & INPUT_DOWN;
        public static int IN0_RACKADV => IO_INPUT0 & 0x10;
        public static int IN0_COIN1 => IO_INPUT0 & 0x20;
        public static int IN0_COIN2 => IO_INPUT0 & 0x40;
        public static int IN0_CREDIT => IO_INPUT0 & 0x80;

        public static int IN1_UP => IO_INPUT1 & INPUT_UP;
        public static int IN1_LEFT => IO_INPUT1 & INPUT_LEFT;
        public static int IN1_RIGHT => IO_INPUT1 & INPUT_RIGHT;
        public static int IN1_DOWN => IO_INPUT1 & INPUT_DOWN;
        public static int IN1_SERVICE => IO_INPUT1 & 0x10;
        public static int IN1_START1 => IO_INPUT1 & 0x20;
        public static int IN1_START2 => IO_INPUT1 & 0x40;
        public static int IN1_CABINET => IO_INPUT1 & 0x80;

        public static int DIP_SWITCH_COINS => DIP_INPUT & 0x03;
        public static int DIP_SWITCH_LIVES => (DIP_INPUT & 0x0c) >> 2;
        public static int DIP_SWITCH_BONUS => (DIP_INPUT & 0x30) >> 4;
        public static int DIP_SWITCH_DIFFICULTY => (DIP_INPUT & 0x40) >> 6;
        public static int DIP_SWITCH_NAMES => (DIP_INPUT & 0x80) >> 7;

        public static ref byte BLINKY_SPRITE => ref MEM[0x4c02];
        public static ref byte BLINKY_COLOUR => ref MEM[0x4c03];
        public static ref byte PINKY_SPRITE => ref MEM[0x4c04];
        public static ref byte PINKY_COLOUR => ref MEM[0x4c05];
        public static ref byte INKY_SPRITE => ref MEM[0x4c06];
        public static ref byte INKY_COLOUR => ref MEM[0x4c07];
        public static ref byte CLYDE_SPRITE => ref MEM[0x4c08];
        public static ref byte CLYDE_COLOUR => ref MEM[0x4c09];
        public static ref byte PACMAN_SPRITE => ref MEM[0x4c0a];
        public static ref byte PACMAN_COLOUR => ref MEM[0x4c0b];
        public static ref byte FRUIT_SPRITE => ref MEM[0x4c0c];
        public static ref byte FRUIT_COLOUR => ref MEM[0x4c0d];

        public static byte* SPRITE_POS => MEM + 0x4c22;
        public static byte* SPRITE_DATA => MEM + 0x4c32;

        public static ref ushort TASK_LIST_END => ref *(ushort*)(MEM + 0x4c80);
        public static ref ushort TASK_LIST_BEGIN => ref *(ushort*)(MEM + 0x4c82);

        public static byte* SOUND_COUNTER => MEM + 0x4c84;
        public static byte* TIMER_SIXTIETHS => MEM + 0x4c86;
        public static ref byte TIMER_SECONDS => ref MEM[0x4c87];
        public static ref byte TIMER_MINUTES => ref MEM[0x4c88];
        public static ref byte TIMER_HOURS => ref MEM[0x4c89];
        public static ref byte COUNTER_LIMITS_CHANGES => ref MEM[0x4c8a];
        public static ref byte RND_NUM_GEN1 => ref MEM[0x4c8b];
        public static ref byte RND_NUM_GEN2 => ref MEM[0x4c8c];

        public static byte* ISR_TASKS => MEM + 0x4c90;
        public static byte* NONISR_TASKS => MEM + 0x4cc0;

        public static ref XYPOS BLINKY_POS => ref *(XYPOS*)(MEM + 0x4d00);
        public static ref XYPOS PINKY_POS => ref *(XYPOS*)(MEM + 0x4d02);
        public static ref XYPOS INKY_POS => ref *(XYPOS*)(MEM + 0x4d04);
        public static ref XYPOS CLYDE_POS => ref *(XYPOS*)(MEM + 0x4d06);
        public static ref XYPOS PACMAN_POS => ref *(XYPOS*)(MEM + 0x4d08);

        public static ref XYPOS BLINKY_TILE => ref *(XYPOS*)(MEM + 0x4d0a);
        public static ref XYPOS PINKY_TILE => ref *(XYPOS*)(MEM + 0x4d0c);
        public static ref XYPOS INKY_TILE => ref *(XYPOS*)(MEM + 0x4d0e);
        public static ref XYPOS CLYDE_TILE => ref *(XYPOS*)(MEM + 0x4d10);
        public static ref XYPOS PACMAN_TILE => ref *(XYPOS*)(MEM + 0x4d12);

        public static ref XYPOS BLINKY_VECTOR => ref *(XYPOS*)(MEM + 0x4d14);
        public static ref XYPOS PINKY_VECTOR => ref *(XYPOS*)(MEM + 0x4d16);
        public static ref XYPOS INKY_VECTOR => ref *(XYPOS*)(MEM + 0x4d18);
        public static ref XYPOS CLYDE_VECTOR => ref *(XYPOS*)(MEM + 0x4d1a);
        public static ref XYPOS PACMAN_VECTOR => ref *(XYPOS*)(MEM + 0x4d1c);

        public static ref XYPOS BLINKY_VECTOR2 => ref *(XYPOS*)(MEM + 0x4d1e);
        public static ref XYPOS PINKY_VECTOR2 => ref *(XYPOS*)(MEM + 0x4d20);
        public static ref XYPOS INKY_VECTOR2 => ref *(XYPOS*)(MEM + 0x4d22);
        public static ref XYPOS CLYDE_VECTOR2 => ref *(XYPOS*)(MEM + 0x4d24);
        public static ref XYPOS PACMAN_VECTOR2 => ref *(XYPOS*)(MEM + 0x4d26);

        public static ref byte BLINKY_PREV_ORIENTATION => ref MEM[0x4d28];
        public static ref byte PINKY_PREV_ORIENTATION => ref MEM[0x4d29];
        public static ref byte INKY_PREV_ORIENTATION => ref MEM[0x4d2a];
        public static ref byte CLYDE_PREV_ORIENTATION => ref MEM[0x4d2b];

        public static ref byte BLINKY_ORIENTATION => ref MEM[0x4d2c];
        public static ref byte PINKY_ORIENTATION => ref MEM[0x4d2d];
        public static ref byte INKY_ORIENTATION => ref MEM[0x4d2e];
        public static ref byte CLYDE_ORIENTATION => ref MEM[0x4d2f];

        public static ref byte PACMAN_ORIENTATION => ref MEM[0x4d30];

        public static ref XYPOS BLINKY_TILE2 => ref *(XYPOS*)(MEM + 0x4d31);
        public static ref XYPOS PINKY_TILE2 => ref *(XYPOS*)(MEM + 0x4d33);
        public static ref XYPOS INKY_TILE2 => ref *(XYPOS*)(MEM + 0x4d35);
        public static ref XYPOS CLYDE_TILE2 => ref *(XYPOS*)(MEM + 0x4d37);
        public static ref XYPOS PACMAN_TILE2 => ref *(XYPOS*)(MEM + 0x4d39);

        public static ref byte BEST_ORIENTATION_FOUND => ref MEM[0x4d3b];
        public static ref byte PACMAN_DESIRED_ORIENTATION => ref MEM[0x4d3c];
        public static ref byte OPPOSITE_ORIENTATION => ref MEM[0x4d3d];
        public static ref XYPOS CURRENT_TILE_POS => ref *(XYPOS*)(MEM + 0x4d3e);
        public static ref XYPOS DEST_TILE_POS => ref *(XYPOS*)(MEM + 0x4d40);
        public static ref XYPOS TMP_RESULT_POS => ref *(XYPOS*)(MEM + 0x4d42);
        public static ref ushort MIN_DISTANCE_FOUND => ref *(ushort*)(MEM + 0x4d44);
        public static ref uint PACMAN_MOVE_PAT_NORMAL => ref *(uint*)(MEM + 0x4d46);
        public static ref uint PACMAN_MOVE_PAT_POWERUP => ref *(uint*)(MEM + 0x4d4a);
        public static ref uint BLINKY_MOVE_PAT_DIFF2 => ref *(uint*)(MEM + 0x4d4e);
        public static ref uint BLINKY_MOVE_PAT_DIFF1 => ref *(uint*)(MEM + 0x4d52);
        public static ref uint BLINKY_MOVE_PAT_NORMAL => ref *(uint*)(MEM + 0x4d56);
        public static ref uint BLINKY_MOVE_PAT_EDIBLE => ref *(uint*)(MEM + 0x4d5a);
        public static ref uint BLINKY_MOVE_PAT_TUNNEL => ref *(uint*)(MEM + 0x4d5e);
        public static ref uint PINKY_MOVE_PAT_NORMAL => ref *(uint*)(MEM + 0x4d62);
        public static ref uint PINKY_MOVE_PAT_EDIBLE => ref *(uint*)(MEM + 0x4d66);
        public static ref uint PINKY_MOVE_PAT_TUNNEL => ref *(uint*)(MEM + 0x4d6a);
        public static ref uint INKY_MOVE_PAT_NORMAL => ref *(uint*)(MEM + 0x4d6e);
        public static ref uint INKY_MOVE_PAT_EDIBLE => ref *(uint*)(MEM + 0x4d72);
        public static ref uint INKY_MOVE_PAT_TUNNEL => ref *(uint*)(MEM + 0x4d76);
        public static ref uint CLYDE_MOVE_PAT_NORMAL => ref *(uint*)(MEM + 0x4d7a);
        public static ref uint CLYDE_MOVE_PAT_EDIBLE => ref *(uint*)(MEM + 0x4d7e);
        public static ref uint CLYDE_MOVE_PAT_TUNNEL => ref *(uint*)(MEM + 0x4d82);
        public static ushort* DIFFICULTY_TABLE => (ushort*)(MEM + 0x4d86);

        public static ref byte GHOST_HOUSE_MOVE_COUNT => ref MEM[0x4d94];
        public static ref ushort UNITS_B4_GHOST_LEAVES_HOME => ref *(ushort*)(MEM + 0x4d95);
        public static ref ushort UNITS_INACTIVITY_COUNTER => ref *(ushort*)(MEM + 0x4d97);
        public static ref byte BLINKY_IN_TUNNEL => ref MEM[0x4d99];
        public static ref byte PINKY_IN_TUNNEL => ref MEM[0x4d9a];
        public static ref byte INKY_IN_TUNNEL => ref MEM[0x4d9b];
        public static ref byte CLYDE_IN_TUNNEL => ref MEM[0x4d9c];
        public static ref byte PACMAN_MOVE_DELAY => ref MEM[0x4d9d];
        public static ref byte EATEN_SINCE_MOVE => ref MEM[0x4d9e];
        public static ref byte EATEN_PILLS_COUNT => ref MEM[0x4d9f];

        public static ref byte BLINKY_SUBSTATE => ref MEM[0x4da0];
        public static ref byte PINKY_SUBSTATE => ref MEM[0x4da1];
        public static ref byte INKY_SUBSTATE => ref MEM[0x4da2];
        public static ref byte CLYDE_SUBSTATE => ref MEM[0x4da3];

        public static ref byte KILLED_GHOST_INDEX => ref MEM[0x4da4];
        public static ref byte PAC_DEAD_ANIM_STATE => ref MEM[0x4da5];
        public static ref byte PACMAN_POWEREDUP => ref MEM[0x4da6];
        public static ref byte BLINKY_EDIBLE => ref MEM[0x4da7];
        public static ref byte PINKY_EDIBLE => ref MEM[0x4da8];
        public static ref byte INKY_EDIBLE => ref MEM[0x4da9];
        public static ref byte CLYDE_EDIBLE => ref MEM[0x4daa];
        public static ref byte GHOST_STATE => ref MEM[0x4dab];
        public static ref byte BLINKY_STATE => ref MEM[0x4dac];
        public static ref byte PINKY_STATE => ref MEM[0x4dad];
        public static ref byte INKY_STATE => ref MEM[0x4dae];
        public static ref byte CLYDE_STATE => ref MEM[0x4daf];

        public static ref byte REL_DIFF => ref MEM[0x4db0];
        public static ref byte BLINKY_ORIENT_CHG_FLAG => ref MEM[0x4db1];
        public static ref byte PINKY_ORIENT_CHG_FLAG => ref MEM[0x4db2];
        public static ref byte INKY_ORIENT_CHG_FLAG => ref MEM[0x4db3];
        public static ref byte CLYDE_ORIENT_CHG_FLAG => ref MEM[0x4db4];
        public static ref byte PACMAN_ORIENT_CHG_FLAG => ref MEM[0x4db5];
        public static ref byte CRUISE_ELROY_MODE_1 => ref MEM[0x4db6];
        public static ref byte CRUISE_ELROY_MODE_2 => ref MEM[0x4db7];
        public static ref byte PINKY_LEAVE_HOME_COUNTER => ref MEM[0x4db8];
        public static ref byte INKY_LEAVE_HOME_COUNTER => ref MEM[0x4db9];
        public static ref byte CLYDE_LEAVE_HOME_COUNTER => ref MEM[0x4dba];
        public static ref byte PILLS_REM_DIFF_1 => ref MEM[0x4dbb];
        public static ref byte PILLS_REM_DIFF_2 => ref MEM[0x4dbc];
        public static ref ushort GHOST_EDIBLE_TIME => ref *(ushort*)(MEM + 0x4dbd);
        public static ref byte PACMAN_IN_TUNNEL => ref MEM[0x4dbf];

        public static ref byte GHOST_ANIMATION => ref MEM[0x4dc0];
        public static ref byte NONRANDOM_MOVEMENT => ref MEM[0x4dc1];
        public static ref ushort ORIENTATION_CHANGE_COUNT => ref *(ushort*)(MEM + 0x4dc2);
        public static ref byte GHOST_ANIMATION_COUNTER => ref MEM[0x4dc4];
        public static ref ushort COUNT_SINCE_PAC_KILLED => ref *(ushort*)(MEM + 0x4dc5);
        public static ref byte TRIAL_ORIENTATION => ref MEM[0x4dc7];
        public static ref byte GHOST_COL_POWERUP_COUNTER => ref MEM[0x4dc8];
        public static ref ushort RND_VAL_PTR => ref *(ushort*)(MEM + 0x4dc9);
        public static ref ushort EDIBLE_REMAIN_COUNT => ref *(ushort*)(MEM + 0x4dcb);
        public static ref byte COIN_TIMER => ref MEM[0x4dce];

        public static ref byte PILL_CHANGE_COUNTER => ref MEM[0x4dcf];
        public static ref byte KILLED_COUNT => ref MEM[0x4dd0];
        public static ref byte KILLED_STATE => ref MEM[0x4dd1];
        public static ref XYPOS FRUIT_POS => ref *(XYPOS*)(MEM + 0x4dd2);

        public static ref byte FRUIT_POINTS => ref MEM[0x4dd4];
        public static ref byte WAIT_START_BUTTON => ref MEM[0x4dd6];

        public static ref byte MAIN_STATE => ref MEM[0x4e00];
        public static ref byte RESET_STATE => ref MEM[0x4e01];
        public static ref byte INTRO_STATE => ref MEM[0x4e02];
        public static ref byte CREDIT_STATE => ref MEM[0x4e03];
        public static ref byte LEVEL_STATE => ref MEM[0x4e04];
        public static ref byte SCENE1_STATE => ref MEM[0x4e06];
        public static ref byte SCENE2_STATE => ref MEM[0x4e07];
        public static ref byte SCENE3_STATE => ref MEM[0x4e08];

        public static ref byte PLAYER => ref MEM[0x4e09];

        public static ref ushort P1_CURR_DIFFICULTY => ref *(ushort*)(MEM + 0x4e0a);
        public static ref byte P1_FIRST_FRUIT => ref MEM[0x4e0c];
        public static ref byte P1_SECOND_FRUIT => ref MEM[0x4e0d];
        public static ref byte P1_PILLS_EATEN_LEVEL => ref MEM[0x4e0e];
        public static ref byte P1_PINKY_LEAVE_HOME_COUNTER => ref MEM[0x4e0f];
        public static ref byte P1_INKY_LEAVE_HOME_COUNTER => ref MEM[0x4e10];
        public static ref byte P1_CLYDE_LEAVE_HOME_COUNTER => ref MEM[0x4e11];
        public static ref byte P1_DIED_IN_LEVEL => ref MEM[0x4e12];
        public static ref byte P1_LEVEL => ref MEM[0x4e13];
        public static ref byte P1_REAL_LIVES => ref MEM[0x4e14];
        public static ref byte P1_DISPLAY_LIVES => ref MEM[0x4e15];
        public static byte* P1_PILL_ARRAY => MEM + 0x4e16;
        public static byte* P1_POWERUP_ARRAY => MEM + 0x4e34;

        public static ref ushort P2_CURR_DIFFICULTY => ref *(ushort*)(MEM + 0x4e38);
        public static ref byte P2_FIRST_FRUIT => ref MEM[0x4e3a];
        public static ref byte P2_SECOND_FRUIT => ref MEM[0x4e3b];
        public static ref byte P2_PILLS_EATEN_LEVEL => ref MEM[0x4e3c];
        public static ref byte P2_PINKY_LEAVE_HOME_COUNTER => ref MEM[0x4e3d];
        public static ref byte P2_INKY_LEAVE_HOME_COUNTER => ref MEM[0x4e3e];
        public static ref byte P2_CLYDE_LEAVE_HOME_COUNTER => ref MEM[0x4e3f];
        public static ref byte P2_DIED_IN_LEVEL => ref MEM[0x4e40];
        public static ref byte P2_LEVEL => ref MEM[0x4e41];
        public static ref byte P2_REAL_LIVES => ref MEM[0x4e42];
        public static ref byte P2_DISPLAY_LIVES => ref MEM[0x4e43];
        public static byte* P2_PILL_ARRAY => MEM + 0x4e44;
        public static byte* P2_POWERUP_ARRAY => MEM + 0x4e42;

        public static ref byte SERVICE1_DEBOUNCE => ref MEM[0x4e66];
        public static ref byte COIN2_DEBOUNCE => ref MEM[0x4e67];
        public static ref byte COIN1_DEBOUNCE => ref MEM[0x4e68];
        public static ref byte COIN_COUNTER => ref MEM[0x4e69];
        public static ref byte COIN_COUNTER_TIMEOUT => ref MEM[0x4e6a];
        public static ref byte COINS_PER_CREDIT => ref MEM[0x4e6b];
        public static ref byte PARTIAL_CREDIT => ref MEM[0x4e6c];
        public static ref byte CREDITS_PER_COIN => ref MEM[0x4e6d];
        public static ref byte CREDITS => ref MEM[0x4e6e];
        public static ref byte LIVES_PER_GAME => ref MEM[0x4e6f];
        public static ref byte TWO_PLAYERS => ref MEM[0x4e70];
        public static ref byte BONUS_LIFE => ref MEM[0x4e71];
        public static ref byte COCKTAIL_MODE => ref MEM[0x4e72];
        public static ref ushort DIFFICULTY_PTR => ref *(ushort*)(MEM + 0x4e73);
        public static ref byte GHOST_NAMES_MODE => ref MEM[0x4e75];
        public static byte* P1_SCORE => MEM + 0x4e80;
        public static byte* P2_SCORE => MEM + 0x4e84;
        public static byte* HIGH_SCORE => MEM + 0x4e88;

        public static ref byte CH1_FREQ0 => ref MEM[0x4e8c];
        public static ref byte CH1_FREQ1 => ref MEM[0x4e8d];
        public static ref byte CH1_FREQ2 => ref MEM[0x4e8e];
        public static ref byte CH1_FREQ3 => ref MEM[0x4e8f];
        public static ref byte CH1_FREQ4 => ref MEM[0x4e90];
        public static ref byte CH1_VOL => ref MEM[0x4e91];
        public static ref byte CH2_FREQ0 => ref MEM[0x4e92];
        public static ref byte CH2_FREQ1 => ref MEM[0x4e93];
        public static ref byte CH2_FREQ2 => ref MEM[0x4e94];
        public static ref byte CH2_FREQ3 => ref MEM[0x4e95];
        public static ref byte CH2_VOL => ref MEM[0x4e96];
        public static ref byte CH3_FREQ0 => ref MEM[0x4e97];
        public static ref byte CH3_FREQ1 => ref MEM[0x4e98];
        public static ref byte CH3_FREQ2 => ref MEM[0x4e99];
        public static ref byte CH3_FREQ3 => ref MEM[0x4e9a];
        public static ref byte CH3_VOL => ref MEM[0x4e9b];

        public static SOUND_EFFECT* CH1_SOUND_EFFECT => (SOUND_EFFECT*)(MEM + 0x4e9c);
        public static SOUND_EFFECT* CH2_SOUND_EFFECT => (SOUND_EFFECT*)(MEM + 0x4eac);
        public static SOUND_EFFECT* CH3_SOUND_EFFECT => (SOUND_EFFECT*)(MEM + 0x4ebc);
        public static SOUND_EFFECT* CH1_SOUND_WAVE => (SOUND_EFFECT*)(MEM + 0x4ecc);
        public static SOUND_EFFECT* CH2_SOUND_WAVE => (SOUND_EFFECT*)(MEM + 0x4edc);
        public static SOUND_EFFECT* CH3_SOUND_WAVE => (SOUND_EFFECT*)(MEM + 0x4eec);

        public static void swap16(byte* a, byte* b)
        {
            byte tmp;
            tmp = a[0]; a[0] = b[0]; b[0] = tmp;
            tmp = a[1]; a[1] = b[1]; b[1] = tmp;
        }

        public static int bcdAdjust(byte* value)
        {
            if ((*value & 0xf) > 9)
                *value += 6;

            if ((*value & 0xf0) > 0x90)
            {
                *value -= 0xa0;
                return 1;
            }

            return 0;
        }

        public static int bcdAdjust(ref byte value)
        {
            if ((value & 0xf) > 9)
                value += 6;

            if ((value & 0xf0) > 0x90)
            {
                value -= 0xa0;
                return 1;
            }

            return 0;
        }

        public static void rotate8(byte* value, int count)
        {
            *value = (byte)((*value << count) | (*value >> (8 - count)));
        }

        public static void rotate8(ref byte value, int count)
        {
            value = (byte)((value << count) | (value >> (8 - count)));
        }

        public static void rotate32(uint* value, int count)
        {
            *value = (*value << count) | (*value >> (32 - count));
        }

        public static void rotate32(ref uint value, int count)
        {
            value = (value << count) | (value >> (32 - count));
        }

        public static void assert(bool cond, string file, int line)
        {
            if (!cond)
            {
                Console.Error.WriteLine($"ASSERT {file}:{line}");
                Environment.Exit(1);
            }
        }
    }
}
