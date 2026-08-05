import re

with open('PacmanCS/Pacman.cs', 'r') as f:
    lines = f.readlines()

# 1. Address-of property pointers
map_ptrs = {
    'P1_SCORE': 'P1_SCORE',
    'P2_SCORE': 'P2_SCORE',
    'HIGH_SCORE': 'HIGH_SCORE',
    'GHOST_ANIMATION': '(byte*)(MEM + 0x4dc0)',
    'RND_NUM_GEN1': '(byte*)(MEM + 0x4c8b)',
    'COIN_TIMER': '(byte*)(MEM + 0x4dce)',
    'ORIENTATION_CHANGE_COUNT': '(ushort*)(MEM + 0x4dc2)',
    'BLINKY_SUBSTATE': '(byte*)(MEM + 0x4da0)',
    'PINKY_SUBSTATE': '(byte*)(MEM + 0x4da1)',
    'INKY_SUBSTATE': '(byte*)(MEM + 0x4da2)',
    'CLYDE_SUBSTATE': '(byte*)(MEM + 0x4da3)'
}

for i in range(len(lines)):
    line = lines[i]

    # Replace &PROP
    for prop, ptr in map_ptrs.items():
        if f'&{prop}' in line:
            line = line.replace(f'&{prop}', ptr)

    # Replace &sound->...
    if '&sound->' in line:
        line = re.sub(r'&\s*([a-zA-Z0-9_\->\.]+)\s*\[\s*0\s*\]', r'\1', line)
        line = re.sub(r'&\s*(sound->[a-zA-Z0-9_]+)', r'\1', line)

    # Fix tableCall_0020
    if 'tableCall_0020' in line and '((byte)(func)' in line:
        line = line.replace('((byte)(func)', 'func')

    # Fix boolean operator comparisons
    if '!= 0' in line:
        line = line.replace('(inInterrupt != 0)', 'inInterrupt')
        line = line.replace('(cpuPaused != 0)', 'cpuPaused')

    # Fix null pointer comparison
    if '!= 0' in line and 'byte*' in line:
        line = line.replace('!= 0', '!= null')

    lines[i] = line

text = ''.join(lines)

# Fix labels
if 'random_1:' not in text:
    text = text.replace('goto random_1;', 'random_1:\n                ;')

text = text.replace('jump_2c93:\n\n        jump_2c93:', 'jump_2c93:\n            ;')
text = text.replace('jump_2d6c:\n\n        jump_2d6c:', 'jump_2d6c:\n            ;')

if 'jump_2c93:' not in text:
    text = text.replace('addr = effect->freqTable;', 'jump_2c93:\n                addr = effect->freqTable;')

if 'jump_2d6c:' not in text:
    text = text.replace('addr = effect->offset;', 'jump_2d6c:\n                addr = effect->offset;')

with open('PacmanCS/Pacman.cs', 'w') as f:
    f.write(text)

print("Applied fix_34_final.py!")
