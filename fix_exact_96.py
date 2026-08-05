import re

with open('PacmanCS/Pacman.cs', 'r') as f:
    text = f.read()

# 1. Delegate fix
text = text.replace('showBonusLifeScore_26b2,', '(_) => showBonusLifeScore_26b2(),')

# 2. Fix property address-of expressions to direct pointer expressions
props = {
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
    'PACMAN_IN_TUNNEL': '(byte*)(MEM + 0x4dbf)'
}

for prop, ptr_expr in props.items():
    text = text.replace(f'&{prop}', ptr_expr)

# 3. Sound struct field array pointers: &sound->v1FreqCount[0] -> sound->v1FreqCount
text = re.sub(r'&\s*([a-zA-Z0-9_\->\.]+)\s*\[\s*0\s*\]', r'\1', text)

# 4. Target variable shadowing inside functions
lines = text.splitlines()
target_count = 0
for i in range(len(lines)):
    if 'XYPOS target =' in lines[i]:
        target_count += 1
        lines[i] = lines[i].replace('XYPOS target =', f'XYPOS target_{target_count} =')
        # replace usage of target in next 5 lines
        for j in range(i+1, min(i+6, len(lines))):
            lines[j] = re.sub(r'\btarget\b', f'target_{target_count}', lines[j])

text = '\n'.join(lines)

with open('PacmanCS/Pacman.cs', 'w') as f:
    f.write(text)

print("Applied fix_exact_96.py!")
