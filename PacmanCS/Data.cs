using System;

namespace PacmanCS
{
    public static class Data
    {
        public const int OFFSET_0219 = 0x0219;
        public const int OFFSET_0796 = 0x0796;
        public const int OFFSET_0843 = 0x0843;
        public const int OFFSET_084f = 0x084f;
        public const int OFFSET_0861 = 0x0861;
        public const int OFFSET_0873 = 0x0873;
        public const int FRUIT_DATA_0efd_OFFSET = 0x0efd;
        public const int BONUS_LIFE_DATA_OFFSET = 0x2728;
        public const int DIFFICULTY_DATA_OFFSET = 0x272c;
        public const int MOVE_VECTOR_DATA_OFFSET = 0x32ff;
        public const int MOVE_VECTOR_RIGHT_OFFSET = 0x32ff;
        public const int MOVE_VECTOR_DOWN_OFFSET = 0x3301;
        public const int MOVE_VECTOR_LEFT_OFFSET = 0x3303;
        public const int MOVE_VECTOR_UP_OFFSET = 0x3305;
        public const int MOVE_DATA_330f_OFFSET = 0x330f;
        public const int DATA_3435_OFFSET = 0x3435;
        public const int DATA_35b5_OFFSET = 0x35b5;
        public const int EFFECT_TABLE_CH3_3b80_OFFSET = 0x3b80;
        public const int EFFECT_TABLE_CH1_3b30_OFFSET = 0x3b30;
        public const int EFFECT_TABLE_CH2_3b40_OFFSET = 0x3b40;
        public const int FRUIT_TABLE_OFFSET = 0x3b08;
        public const int DATA_MSG_TABLE_36a5_OFFSET = 0x36a5;
        public const int SONG_TABLE_CH1_3bc8_OFFSET = 0x3bc8;
        public const int POWER_OF_2_3bb0_OFFSET = 0x3bb0;
        public const int FREQ_TABLE_3bb8_OFFSET = 0x3bb8;
        public const int SONG_TABLE_CH2_3bcc_OFFSET = 0x3bcc;
        public const int SONG_TABLE_CH3_3bd0_OFFSET = 0x3bd0;
        public const int DATA_3154_OFFSET = 0x3154;
        public const int BAD_ROM_316c_OFFSET = 0x316c;
        public const int BAD_W_RAM_316e_OFFSET = 0x316e;
        public const int BAD_V_RAM_3170_OFFSET = 0x3170;
        public const int BAD_C_RAM_3172_OFFSET = 0x3172;
        public const int DATA_32f9_OFFSET = 0x32f9;
        public const int DATA_3ae2_OFFSET = 0x3ae2;

        public static unsafe byte* DATA_0219 => MemMap.ROM + OFFSET_0219;
        public static unsafe byte* DATA_0796 => MemMap.ROM + OFFSET_0796;
        public static unsafe byte* DATA_0843 => MemMap.ROM + OFFSET_0843;
        public static unsafe byte* DATA_084f => MemMap.ROM + OFFSET_084f;
        public static unsafe byte* DATA_0861 => MemMap.ROM + OFFSET_0861;
        public static unsafe byte* DATA_0873 => MemMap.ROM + OFFSET_0873;
        public static unsafe byte* FRUIT_DATA_0efd => MemMap.ROM + FRUIT_DATA_0efd_OFFSET;
        public static unsafe byte* BONUS_LIFE_DATA => MemMap.ROM + BONUS_LIFE_DATA_OFFSET;
        public static unsafe ushort* DIFFICULTY_DATA => (ushort*)(MemMap.ROM + DIFFICULTY_DATA_OFFSET);
        public static unsafe XYPOS* MOVE_VECTOR_DATA => (XYPOS*)(MemMap.ROM + MOVE_VECTOR_DATA_OFFSET);
        public static unsafe XYPOS* MOVE_VECTOR_RIGHT => (XYPOS*)(MemMap.ROM + MOVE_VECTOR_RIGHT_OFFSET);
        public static unsafe XYPOS* MOVE_VECTOR_DOWN => (XYPOS*)(MemMap.ROM + MOVE_VECTOR_DOWN_OFFSET);
        public static unsafe XYPOS* MOVE_VECTOR_LEFT => (XYPOS*)(MemMap.ROM + MOVE_VECTOR_LEFT_OFFSET);
        public static unsafe XYPOS* MOVE_VECTOR_UP => (XYPOS*)(MemMap.ROM + MOVE_VECTOR_UP_OFFSET);
        public static unsafe byte* MOVE_DATA_330f => MemMap.ROM + MOVE_DATA_330f_OFFSET;
        public static unsafe byte* DATA_3435 => MemMap.ROM + DATA_3435_OFFSET;
        public static unsafe byte* DATA_35b5 => MemMap.ROM + DATA_35b5_OFFSET;
        public static unsafe byte* EFFECT_TABLE_CH3_3b80 => MemMap.ROM + EFFECT_TABLE_CH3_3b80_OFFSET;
        public static unsafe byte* EFFECT_TABLE_CH1_3b30 => MemMap.ROM + EFFECT_TABLE_CH1_3b30_OFFSET;
        public static unsafe byte* EFFECT_TABLE_CH2_3b40 => MemMap.ROM + EFFECT_TABLE_CH2_3b40_OFFSET;
        public static unsafe byte* FRUIT_TABLE => MemMap.ROM + FRUIT_TABLE_OFFSET;
        public static unsafe byte* DATA_MSG_TABLE_36a5 => MemMap.ROM + DATA_MSG_TABLE_36a5_OFFSET;
        public static unsafe byte* SONG_TABLE_CH1_3bc8 => MemMap.ROM + SONG_TABLE_CH1_3bc8_OFFSET;
        public static unsafe byte* POWER_OF_2_3bb0 => MemMap.ROM + POWER_OF_2_3bb0_OFFSET;
        public static unsafe byte* FREQ_TABLE_3bb8 => MemMap.ROM + FREQ_TABLE_3bb8_OFFSET;
        public static unsafe byte* SONG_TABLE_CH2_3bcc => MemMap.ROM + SONG_TABLE_CH2_3bcc_OFFSET;
        public static unsafe byte* SONG_TABLE_CH3_3bd0 => MemMap.ROM + SONG_TABLE_CH3_3bd0_OFFSET;
        public static unsafe ushort* DATA_3154 => (ushort*)(MemMap.ROM + DATA_3154_OFFSET);
        public static unsafe byte* BAD_ROM_316c => MemMap.ROM + BAD_ROM_316c_OFFSET;
        public static unsafe byte* BAD_W_RAM_316e => MemMap.ROM + BAD_W_RAM_316e_OFFSET;
        public static unsafe byte* BAD_V_RAM_3170 => MemMap.ROM + BAD_V_RAM_3170_OFFSET;
        public static unsafe byte* BAD_C_RAM_3172 => MemMap.ROM + BAD_C_RAM_3172_OFFSET;
        public static unsafe byte* DATA_32f9 => MemMap.ROM + DATA_32f9_OFFSET;
        public static unsafe ushort* DATA_3ae2 => (ushort*)(MemMap.ROM + DATA_3ae2_OFFSET);
    }
}
