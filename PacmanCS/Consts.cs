using System;

namespace PacmanCS
{
    public static class Consts
    {
        public const byte ISRTASK_INC_LEVEL_STATE = 0;
        public const byte ISRTASK_INC_MAIN_SUB2 = 1;
        public const byte ISRTASK_INC_INTRO_STATE = 2;
        public const byte ISRTASK_INC_KILLED_STATE = 3;
        public const byte ISRTASK_RESET_FRUIT = 4;
        public const byte ISRTASK_DISPLAY_READY = 6;
        public const byte ISRTASK_INC_SCENE1_STATE = 7;
        public const byte ISRTASK_INC_SCENE2_STATE = 8;
        public const byte ISRTASK_INC_SCENE3_STATE = 9;

        public const byte TASK_CLEAR_SCREEN = 0x00;
        public const byte TASK_MAZE_COLOURS = 0x01;
        public const byte TASK_DRAW_MAZE = 0x02;
        public const byte TASK_DRAW_PILLS = 0x03;
        public const byte TASK_INIT_POSITIONS = 0x04;
        public const byte TASK_BLINKY_SUBSTATE = 0x05;
        public const byte TASK_CLEAR_COLOUR = 0x06;
        public const byte TASK_RESET_GAME_STATE = 0x07;
        public const byte TASK_SCATTER_CHASE_BLINKY = 0x08;
        public const byte TASK_SCATTER_CHASE_PINKY = 0x09;
        public const byte TASK_SCATTER_CHASE_INKY = 0x0a;
        public const byte TASK_SCATTER_CHASE_CLYDE = 0x0b;
        public const byte TASK_HOME_RANDOM_BLINKY = 0x0c;
        public const byte TASK_HOME_RANDOM_PINKY = 0x0d;
        public const byte TASK_HOME_RANDOM_INKY = 0x0e;
        public const byte TASK_HOME_RANDOM_CLYDE = 0x0f;
        public const byte TASK_SETUP_GHOST_TIMERS = 0x10;
        public const byte TASK_CLEAR_GHOST_STATE = 0x11;
        public const byte TASK_CLEAR_PILLS = 0x12;
        public const byte TASK_CLEAR_PILLS_SCREEN = 0x13;
        public const byte TASK_CONFIGURE_GAME = 0x14;
        public const byte TASK_UPDATE_PILLS = 0x15;
        public const byte TASK_PACMAN_ORIENT = 0x17;
        public const byte TASK_CLEAR_SCORES = 0x18;
        public const byte TASK_ADD_TO_SCORE = 0x19;
        public const byte TASK_BOTTOM_COLOUR = 0x1a;
        public const byte TASK_FRUIT_HISTORY = 0x1b;
        public const byte TASK_DISPLAY_MSG = 0x1c;
        public const byte TASK_DISPLAY_CREDITS = 0x1d;
        public const byte TASK_RESET_POSITIONS = 0x1e;
        public const byte TASK_SHOW_BONUS_LIFE_SCORE = 0x1f;

        public const byte MSG_HIGHSCORE = 0;
        public const byte MSG_CREDIT = 1;
        public const byte MSG_FREEPLAY = 2;
        public const byte MSG_PLAYER1 = 3;
        public const byte MSG_PLAYER2 = 4;
        public const byte MSG_GAMEOVER = 5;
        public const byte MSG_READY = 6;
        public const byte MSG_PUSHSTART = 7;
        public const byte MSG_ONEPLAYER = 8;
        public const byte MSG_ONEORTWOPLAYER = 9;
        public const byte MSG_BONUS_PACMAN = 0x0a;
        public const byte MSG_COPYRIGHT1 = 0x0b;
        public const byte MSG_NICKNAME = 0x0c;
        public const byte MSG_10PTS = 0x11;
        public const byte MSG_50PTS = 0x12;
        public const byte MSG_COPYRIGHT2 = 0x0b;
        public const byte MSG_100PTS = 0x1b;
        public const byte MSG_5000PTS = 0x22;
        public const byte MSG_BADROMRAM = 0x24;
        public const byte MSG_NOCOINS = 0x25;
        public const byte MSG_PACMAN = 0x29;
        public const byte MSG_BONUS_NONE = 0x2a;
        public const byte MSG_BONUS = 0x2b;
        public const byte MSG_TABLE = 0x2c;
        public const byte MSG_000 = 0x2e;

        public const byte CHAR_PILL = 0x10;
        public const byte CHAR_POWERUP = 0x14;
        public const byte CHAR_SPACE = 0x40;
        public const byte CHAR_MAZE_MASK = 0xc0;

        public const byte GHOST_STATE_ALIVE = 0;
        public const byte GHOST_STATE_DEAD = 1;
        public const byte GHOST_STATE_ENTER_HOUSE = 2;
        public const byte GHOST_STATE_HOUSE_MOVE = 3;

        public const int GHOST_BLINKY = 1;
        public const int GHOST_PINKY = 2;
        public const int GHOST_INKY = 3;
        public const int GHOST_CLYDE = 4;
        public const int DEMO_PACMAN = 5;

        public const byte ORIENT_RIGHT = 0;
        public const byte ORIENT_DOWN = 1;
        public const byte ORIENT_LEFT = 2;
        public const byte ORIENT_UP = 3;

        public const byte ORIENT_VERTICAL = 1;

        public const byte IMG_MIRROR = 0x80;
        public const byte IMG_INVERT = 0x40;

        public const byte SUBSTATE_IN_HOUSE = 0;
        public const byte SUBSTATE_CHASE = 1;
        public const byte SUBSTATE_LEAVING_HOUSE = 2;
        public const byte SUBSTATE_HOUSE_MOVE = 3;

        public const byte MAIN_STATE_INIT = 0;
        public const byte MAIN_STATE_DEMO = 1;
        public const byte MAIN_STATE_CREDIT = 2;
        public const byte MAIN_STATE_PLAY = 3;

        public const byte RESET_STATE_RESET = 0;
        public const byte RESET_STATE_DONE = 1;

        public const byte CREDIT_STATE_PUSH_START_MSG = 0;
        public const byte CREDIT_STATE_CHECK_START = 1;
        public const byte CREDIT_STATE_PLAYER1_READY = 2;
        public const byte CREDIT_STATE_RESET = 4;

        public const byte LEVEL_STATE_PLAY_GAME = 3;
        public const byte LEVEL_STATE_GAME_OVER = 9;
        public const byte LEVEL_STATE_x = 12;
        public const byte LEVEL_STATE_x2 = 14;

        public const byte TIMER_TENTHS = 0x40;
    }
}
