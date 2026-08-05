import re

def clean_pacman():
    with open('PacmanCS/Pacman.cs', 'r') as f:
        text = f.read()

    # Direct line replacement for effect->mask
    text = text.replace('effect->mask &= unchecked((byte)~effec)t->current;', 'effect->mask &= unchecked((byte)~effect->current);')
    text = text.replace('effect->mask &= ~effect->current;', 'effect->mask &= unchecked((byte)~effect->current);')

    # Unchecked bitwise NOT on byte/struct assignments
    text = re.sub(r'&= \s*~([0-9xX0-9a-fA-F]+)', r'&= unchecked((byte)~\1)', text)
    text = re.sub(r'\|= \s*~([0-9xX0-9a-fA-F]+)', r'|= unchecked((byte)~\1)', text)

    # Fix array initializers inside functions
    text = re.sub(r'(\b[A-Za-z0-9_<>]+\[\]\s+[A-Za-z0-9_]+\s*=)\s*\{', r'\1 new[] {', text)

    # Pointer address replacements for Memory Mapped Properties
    map_pointers = {
        'BLINKY_POS': '(XYPOS*)(MEM + 0x4d00)',
        'PINKY_POS': '(XYPOS*)(MEM + 0x4d02)',
        'INKY_POS': '(XYPOS*)(MEM + 0x4d04)',
        'CLYDE_POS': '(XYPOS*)(MEM + 0x4d06)',
        'PACMAN_POS': '(XYPOS*)(MEM + 0x4d08)',
        'BLINKY_TILE': '(XYPOS*)(MEM + 0x4d0a)',
        'PINKY_TILE': '(XYPOS*)(MEM + 0x4d0c)',
        'INKY_TILE': '(XYPOS*)(MEM + 0x4d0e)',
        'CLYDE_TILE': '(XYPOS*)(MEM + 0x4d10)',
        'PACMAN_TILE': '(XYPOS*)(MEM + 0x4d12)',
        'BLINKY_VECTOR': '(XYPOS*)(MEM + 0x4d14)',
        'PINKY_VECTOR': '(XYPOS*)(MEM + 0x4d16)',
        'INKY_VECTOR': '(XYPOS*)(MEM + 0x4d18)',
        'CLYDE_VECTOR': '(XYPOS*)(MEM + 0x4d1a)',
        'PACMAN_VECTOR': '(XYPOS*)(MEM + 0x4d1c)',
        'BLINKY_VECTOR2': '(XYPOS*)(MEM + 0x4d1e)',
        'PINKY_VECTOR2': '(XYPOS*)(MEM + 0x4d20)',
        'INKY_VECTOR2': '(XYPOS*)(MEM + 0x4d22)',
        'CLYDE_VECTOR2': '(XYPOS*)(MEM + 0x4d24)',
        'PACMAN_VECTOR2': '(XYPOS*)(MEM + 0x4d26)',
        'FRUIT_POS': '(XYPOS*)(MEM + 0x4dd2)',
        'CURRENT_TILE_POS': '(XYPOS*)(MEM + 0x4d3e)',
        'DEST_TILE_POS': '(XYPOS*)(MEM + 0x4d40)',
        'TMP_RESULT_POS': '(XYPOS*)(MEM + 0x4d42)',
        'BLINKY_ORIENTATION': '(byte*)(MEM + 0x4d2c)',
        'PINKY_ORIENTATION': '(byte*)(MEM + 0x4d2d)',
        'INKY_ORIENTATION': '(byte*)(MEM + 0x4d2e)',
        'CLYDE_ORIENTATION': '(byte*)(MEM + 0x4d2f)',
        'PACMAN_ORIENTATION': '(byte*)(MEM + 0x4d30)',
        'BLINKY_SUBSTATE': '(byte*)(MEM + 0x4da0)',
        'PINKY_SUBSTATE': '(byte*)(MEM + 0x4da1)',
        'INKY_SUBSTATE': '(byte*)(MEM + 0x4da2)',
        'CLYDE_SUBSTATE': '(byte*)(MEM + 0x4da3)',
        'CREDITS': '(byte*)(MEM + 0x4e6e)',
        'P1_PILLS_EATEN_LEVEL': '(byte*)(MEM + 0x4e0e)',
        'P2_PILLS_EATEN_LEVEL': '(byte*)(MEM + 0x4e3c)',
        'P1_DIED_IN_LEVEL': '(byte*)(MEM + 0x4e12)',
        'P2_DIED_IN_LEVEL': '(byte*)(MEM + 0x4e40)',
        'P1_PINKY_LEAVE_HOME_COUNTER': '(byte*)(MEM + 0x4e0f)',
        'P1_INKY_LEAVE_HOME_COUNTER': '(byte*)(MEM + 0x4e10)',
        'P1_CLYDE_LEAVE_HOME_COUNTER': '(byte*)(MEM + 0x4e11)',
        'P2_PINKY_LEAVE_HOME_COUNTER': '(byte*)(MEM + 0x4e3d)',
        'P2_INKY_LEAVE_HOME_COUNTER': '(byte*)(MEM + 0x4e3e)',
        'P2_CLYDE_LEAVE_HOME_COUNTER': '(byte*)(MEM + 0x4e3f)',
        'P1_CURR_DIFFICULTY': '(ushort*)(MEM + 0x4e0a)',
        'P2_CURR_DIFFICULTY': '(ushort*)(MEM + 0x4e38)',
        'DIFFICULTY_PTR': '(ushort*)(MEM + 0x4e73)',
        'RND_VAL_PTR': '(ushort*)(MEM + 0x4dc9)',
        'EDIBLE_REMAIN_COUNT': '(ushort*)(MEM + 0x4dcb)',
        'ORIENTATION_CHANGE_COUNT': '(ushort*)(MEM + 0x4dc2)',
        'COUNT_SINCE_PAC_KILLED': '(ushort*)(MEM + 0x4dc5)',
        'MIN_DISTANCE_FOUND': '(ushort*)(MEM + 0x4d44)',
        'TASK_LIST_BEGIN': '(ushort*)(MEM + 0x4c82)',
        'TASK_LIST_END': '(ushort*)(MEM + 0x4c80)',
        'PINKY_MOVE_PAT_EDIBLE': '(uint*)(MEM + 0x4d66)',
        'BLINKY_MOVE_PAT_EDIBLE': '(uint*)(MEM + 0x4d5a)',
        'INKY_MOVE_PAT_EDIBLE': '(uint*)(MEM + 0x4d72)',
        'CLYDE_MOVE_PAT_EDIBLE': '(uint*)(MEM + 0x4d7e)',
        'PACMAN_MOVE_PAT_POWERUP': '(uint*)(MEM + 0x4d4a)',
        'PACMAN_MOVE_PAT_NORMAL': '(uint*)(MEM + 0x4d46)',
        'BLINKY_MOVE_PAT_DIFF2': '(uint*)(MEM + 0x4d4e)',
        'BLINKY_MOVE_PAT_DIFF1': '(uint*)(MEM + 0x4d52)',
        'BLINKY_MOVE_PAT_NORMAL': '(uint*)(MEM + 0x4d56)',
        'BLINKY_MOVE_PAT_TUNNEL': '(uint*)(MEM + 0x4d5e)',
        'PINKY_MOVE_PAT_NORMAL': '(uint*)(MEM + 0x4d62)',
        'PINKY_MOVE_PAT_TUNNEL': '(uint*)(MEM + 0x4d6a)',
        'INKY_MOVE_PAT_NORMAL': '(uint*)(MEM + 0x4d6e)',
        'INKY_MOVE_PAT_TUNNEL': '(uint*)(MEM + 0x4d76)',
        'CLYDE_MOVE_PAT_NORMAL': '(uint*)(MEM + 0x4d7a)',
        'CLYDE_MOVE_PAT_TUNNEL': '(uint*)(MEM + 0x4d82)',
        'REGSWRITE': '(byte*)(MEM + 0x5040)',
        'INTENABLE': '(byte*)(MEM + 0x5000)',
        'SOUNDENABLE': '(byte*)(MEM + 0x5001)',
        'AUXENABLE': '(byte*)(MEM + 0x5002)',
        'FLIPSCREEN': '(byte*)(MEM + 0x5003)',
        'P1START': '(byte*)(MEM + 0x5004)',
        'P2START': '(byte*)(MEM + 0x5005)',
        'COINLOCKOUT': '(byte*)(MEM + 0x5006)',
        'COINCOUNTER': '(byte*)(MEM + 0x5007)',
        'GHOST_EDIBLE_TIME': '(ushort*)(MEM + 0x4dbd)',
        'GHOST_HOUSE_MOVE_COUNT': '(byte*)(MEM + 0x4d94)',
        'EATEN_SINCE_MOVE': '(byte*)(MEM + 0x4d9e)',
        'EATEN_PILLS_COUNT': '(byte*)(MEM + 0x4d9f)',
        'GHOST_STATE': '(byte*)(MEM + 0x4dab)',
        'BLINKY_STATE': '(byte*)(MEM + 0x4dac)',
        'PINKY_STATE': '(byte*)(MEM + 0x4dad)',
        'INKY_STATE': '(byte*)(MEM + 0x4dae)',
        'CLYDE_STATE': '(byte*)(MEM + 0x4daf)',
        'PACMAN_POWEREDUP': '(byte*)(MEM + 0x4da6)',
        'BLINKY_EDIBLE': '(byte*)(MEM + 0x4da7)',
        'PINKY_EDIBLE': '(byte*)(MEM + 0x4da8)',
        'INKY_EDIBLE': '(byte*)(MEM + 0x4da9)',
        'CLYDE_EDIBLE': '(byte*)(MEM + 0x4daa)',
        'BLINKY_IN_TUNNEL': '(byte*)(MEM + 0x4d99)',
        'PINKY_IN_TUNNEL': '(byte*)(MEM + 0x4d9a)',
        'INKY_IN_TUNNEL': '(byte*)(MEM + 0x4d9b)',
        'CLYDE_IN_TUNNEL': '(byte*)(MEM + 0x4d9c)',
        'PACMAN_IN_TUNNEL': '(byte*)(MEM + 0x4dbf)',
        'TIMER_SECONDS': '(byte*)(MEM + 0x4c87)',
        'TIMER_MINUTES': '(byte*)(MEM + 0x4c88)',
        'TIMER_HOURS': '(byte*)(MEM + 0x4c89)',
        'P1_LEVEL': '(byte*)(MEM + 0x4e13)',
        'P2_LEVEL': '(byte*)(MEM + 0x4e41)',
        'PLAYER': '(byte*)(MEM + 0x4e09)',
        'MAIN_STATE': '(byte*)(MEM + 0x4e00)',
        'RESET_STATE': '(byte*)(MEM + 0x4e01)',
        'INTRO_STATE': '(byte*)(MEM + 0x4e02)',
        'CREDIT_STATE': '(byte*)(MEM + 0x4e03)',
        'LEVEL_STATE': '(byte*)(MEM + 0x4e04)',
        'SCENE1_STATE': '(byte*)(MEM + 0x4e06)',
        'SCENE2_STATE': '(byte*)(MEM + 0x4e07)',
        'SCENE3_STATE': '(byte*)(MEM + 0x4e08)',
        'P1_FIRST_FRUIT': '(byte*)(MEM + 0x4e0c)',
        'P1_SECOND_FRUIT': '(byte*)(MEM + 0x4e0d)',
        'P2_FIRST_FRUIT': '(byte*)(MEM + 0x4e3a)',
        'P2_SECOND_FRUIT': '(byte*)(MEM + 0x4e3b)',
        'P1_DISPLAY_LIVES': '(byte*)(MEM + 0x4e15)',
        'P2_DISPLAY_LIVES': '(byte*)(MEM + 0x4e43)',
        'P1_REAL_LIVES': '(byte*)(MEM + 0x4e14)',
        'P2_REAL_LIVES': '(byte*)(MEM + 0x4e42)',
        'RND_NUM_GEN1': '(byte*)(MEM + 0x4c8b)',
        'RND_NUM_GEN2': '(byte*)(MEM + 0x4c8c)',
        'COUNTER_LIMITS_CHANGES': '(byte*)(MEM + 0x4c8a)',
        'SERVICE1_DEBOUNCE': '(byte*)(MEM + 0x4e66)',
        'COIN2_DEBOUNCE': '(byte*)(MEM + 0x4e67)',
        'COIN1_DEBOUNCE': '(byte*)(MEM + 0x4e68)',
        'COIN_COUNTER': '(byte*)(MEM + 0x4e69)',
        'COIN_COUNTER_TIMEOUT': '(byte*)(MEM + 0x4e6a)',
        'COINS_PER_CREDIT': '(byte*)(MEM + 0x4e6b)',
        'PARTIAL_CREDIT': '(byte*)(MEM + 0x4e6c)',
        'CREDITS_PER_COIN': '(byte*)(MEM + 0x4e6d)',
        'LIVES_PER_GAME': '(byte*)(MEM + 0x4e6f)',
        'TWO_PLAYERS': '(byte*)(MEM + 0x4e70)',
        'BONUS_LIFE': '(byte*)(MEM + 0x4e71)',
        'COCKTAIL_MODE': '(byte*)(MEM + 0x4e72)',
        'GHOST_NAMES_MODE': '(byte*)(MEM + 0x4e75)'
    }

    for prop, ptr in map_pointers.items():
        text = text.replace(f'&{prop}', ptr)

    # Score pointers
    text = text.replace('&P1_SCORE', 'P1_SCORE')
    text = text.replace('&P2_SCORE', 'P2_SCORE')
    text = text.replace('&HIGH_SCORE', 'HIGH_SCORE')

    # Fix sound struct array pointers: &sound->v1FreqCount[0] -> sound->v1FreqCount
    text = re.sub(r'&\s*([a-zA-Z0-9_\->\.]+)\s*\[\s*0\s*\]', r'\1', text)

    # Delegate array fix
    funcs_no_param = [
        'resetGameState_2698', 'homeOrRandomBlinky_283b', 'homeOrRandomPinky_2865',
        'homeOrRandomInky_288f', 'homeOrRandomClyde_28b9', 'clearGhostState_26a2',
        'clearPillArrays_24c9', 'clearPillsScreen_2a35', 'pacmanOrientationDemo_28e3',
        'clearScores_2ae0', 'displayCredits_2ba1', 'resetPositions_2675',
        'showBonusLifeScore_26b2'
    ]
    for fn in funcs_no_param:
        text = text.replace(f' {fn},', f' (_) => {fn}(),')
        text = text.replace(f' {fn}\n', f' (_) => {fn}()\n')

    # Labels
    if 'jump_2d6c:' not in text:
        text = text.replace('addr = effect->offset;', 'jump_2d6c:\n                addr = effect->offset;')
    text = text.replace('jump_2c93:\n\n        jump_2c93:', 'jump_2c93:\n            ;')
    text = text.replace('jump_2d6c:\n\n        jump_2d6c:', 'jump_2d6c:\n            ;')

    # Scope variables
    lines = text.splitlines()
    target_count = 0
    input_count = 0
    for i in range(len(lines)):
        if 'XYPOS target =' in lines[i]:
            target_count += 1
            lines[i] = lines[i].replace('XYPOS target =', f'XYPOS target_{target_count} =')
            for j in range(i+1, min(i+6, len(lines))):
                lines[j] = re.sub(r'\btarget\b', f'target_{target_count}', lines[j])
        elif 'byte input =' in lines[i] or 'int input =' in lines[i]:
            input_count += 1
            lines[i] = lines[i].replace('input =', f'input_{input_count} =')
            for j in range(i+1, min(i+10, len(lines))):
                lines[j] = re.sub(r'\binput\b', f'input_{input_count}', lines[j])

    text = '\n'.join(lines)

    with open('PacmanCS/Pacman.cs', 'w') as f:
        f.write(text)

    print("Applied final_clean_pass.py!")

clean_pacman()
