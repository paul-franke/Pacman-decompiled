import re

with open('PacmanCS/Pacman.cs', 'r') as f:
    text = f.read()

# 1. Fix unchecked bitwise NOT assignment on byte
# e.g., &= ~0x80 -> &= unchecked((byte)~0x80)
text = re.sub(r'&= \s*~([0-9xX0-9a-fA-F]+)', r'&= unchecked((byte)~\1)', text)
text = re.sub(r'\|= \s*~([0-9xX0-9a-fA-F]+)', r'|= unchecked((byte)~\1)', text)

# 2. Fix XYPOS property address-of
xypos_props = {
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
    'TMP_RESULT_POS': '(XYPOS*)(MEM + 0x4d42)'
}

for prop, ptr in xypos_props.items():
    text = text.replace(f'&{prop}', ptr)

# 3. Delegate array fix
text = text.replace('showBonusLifeScore_26b2,', '(_) => showBonusLifeScore_26b2(),')

# 4. Shadowing variable 'input'
lines = text.splitlines()
for i in range(len(lines)):
    if 'byte input =' in lines[i] or 'int input =' in lines[i]:
        lines[i] = lines[i].replace('input =', 'input_local =')
        for j in range(i+1, min(i+10, len(lines))):
            lines[j] = re.sub(r'\binput\b', 'input_local', lines[j])

text = '\n'.join(lines)

with open('PacmanCS/Pacman.cs', 'w') as f:
    f.write(text)

print("Applied fix_final_66.py!")
