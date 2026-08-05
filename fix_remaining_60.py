import re

with open('PacmanCS/Pacman.cs', 'r') as f:
    text = f.read()

# Address-of memory mapped byte/ushort registers
more_props3 = {
    'GHOST_ANIMATION': '(byte*)(MEM + 0x4dc0)',
    'RND_NUM_GEN1': '(byte*)(MEM + 0x4c8b)',
    'COIN_TIMER': '(byte*)(MEM + 0x4dce)',
    'ORIENTATION_CHANGE_COUNT': '(ushort*)(MEM + 0x4dc2)',
    'GHOST_ANIMATION_COUNTER': '(byte*)(MEM + 0x4dc4)',
    'NONRANDOM_MOVEMENT': '(byte*)(MEM + 0x4dc1)',
    'TRIAL_ORIENTATION': '(byte*)(MEM + 0x4dc7)',
    'GHOST_COL_POWERUP_COUNTER': '(byte*)(MEM + 0x4dc8)',
    'PILL_CHANGE_COUNTER': '(byte*)(MEM + 0x4dcf)',
    'KILLED_COUNT': '(byte*)(MEM + 0x4dd0)',
    'KILLED_STATE': '(byte*)(MEM + 0x4dd1)',
    'FRUIT_POINTS': '(byte*)(MEM + 0x4dd4)',
    'WAIT_START_BUTTON': '(byte*)(MEM + 0x4dd6)'
}

for prop, ptr in more_props3.items():
    text = text.replace(f'&{prop}', ptr)

# Fix sound struct array pointers: &sound->v1FreqCount[0] -> sound->v1FreqCount
text = re.sub(r'&\s*([a-zA-Z0-9_\->\.]+)\s*\[\s*0\s*\]', r'\1', text)

with open('PacmanCS/Pacman.cs', 'w') as f:
    f.write(text)

print("Applied fix_remaining_60.py stage 2!")
