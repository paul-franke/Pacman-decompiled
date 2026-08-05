import re

with open('PacmanCS/Pacman.cs', 'r') as f:
    text = f.read()

# 1. Address-of property fixes
text = text.replace('&CH1_FREQ0', '(byte*)(MEM + 0x4e8c)')
text = text.replace('&CH2_FREQ0', '(byte*)(MEM + 0x4e92)')
text = text.replace('&CH3_FREQ0', '(byte*)(MEM + 0x4e97)')
text = text.replace('&BLINKY_SPRITE', '(byte*)(MEM + 0x4c02)')
text = text.replace('&PINKY_LEAVE_HOME_COUNTER', '(byte*)(MEM + 0x4e0f)')
text = text.replace('&BLINKY_PREV_ORIENTATION', '(byte*)(MEM + 0x4d28)')
text = text.replace('&PACMAN_DESIRED_ORIENTATION', '(byte*)(MEM + 0x4d3c)')

# 2. Debounce byte assignments
text = text.replace('SERVICE1_DEBOUNCE = ((SERVICE1_DEBOUNCE<<1) & 0x0f) | (IN0_CREDIT?1:0);', 'SERVICE1_DEBOUNCE = (byte)(((SERVICE1_DEBOUNCE<<1) & 0x0f) | (IN0_CREDIT?1:0));')
text = text.replace('COIN2_DEBOUNCE = ((COIN2_DEBOUNCE<<1) & 0x0f) | (IN0_COIN2?1:0);', 'COIN2_DEBOUNCE = (byte)(((COIN2_DEBOUNCE<<1) & 0x0f) | (IN0_COIN2?1:0));')
text = text.replace('COIN1_DEBOUNCE = ((COIN1_DEBOUNCE<<1) & 0x0f) | (IN0_COIN1?1:0);', 'COIN1_DEBOUNCE = (byte)(((COIN1_DEBOUNCE<<1) & 0x0f) | (IN0_COIN1?1:0));')

# 3. Label embedded statement fixes
text = text.replace('random_1:\n                ;', 'random_1:\n                {}')

with open('PacmanCS/Pacman.cs', 'w') as f:
    f.write(text)

print("Applied fix_all_34_exact.py!")
