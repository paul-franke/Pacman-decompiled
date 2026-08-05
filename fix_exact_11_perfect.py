import re

with open('PacmanCS/Pacman.cs', 'r') as f:
    text = f.read()

# 1. Pointer replacements for registers & memory mapped properties
pointers = {
    '&CH1_FREQ0': '(byte*)(MEM + 0x4e8c)',
    '&CH2_FREQ0': '(byte*)(MEM + 0x4e92)',
    '&CH3_FREQ0': '(byte*)(MEM + 0x4e97)',
    '&BLINKY_SPRITE': '(byte*)(MEM + 0x4c02)',
    '&PINKY_LEAVE_HOME_COUNTER': '(byte*)(MEM + 0x4e0f)',
    '&BLINKY_PREV_ORIENTATION': '(byte*)(MEM + 0x4d28)',
    '&PACMAN_DESIRED_ORIENTATION': '(byte*)(MEM + 0x4d3c)',
    '&P1_SCORE': 'P1_SCORE',
    '&P2_SCORE': 'P2_SCORE',
    '&HIGH_SCORE': 'HIGH_SCORE',
    '&GHOST_ANIMATION': '(byte*)(MEM + 0x4dc0)',
    '&RND_NUM_GEN1': '(byte*)(MEM + 0x4c8b)',
    '&COIN_TIMER': '(byte*)(MEM + 0x4dce)',
    '&ORIENTATION_CHANGE_COUNT': '(ushort*)(MEM + 0x4dc2)'
}
for k, v in pointers.items():
    text = text.replace(k, v)

# Fix sound struct array pointers: &sound->v1FreqCount[0] -> sound->v1FreqCount
text = re.sub(r'&\s*([a-zA-Z0-9_\->\.]+)\s*\[\s*0\s*\]', r'\1', text)

# 2. C functions -> C#
text = re.sub(r'\bexit\s*\(([^)]+)\)', r'Environment.Exit(\1)', text)
text = re.sub(r'\bASSERT\s*\(([^)]+)\)', r'assert(\1, "", 0)', text)

# 3. Struct initializers XYPOS target = { x, y }
text = re.sub(r'XYPOS\s+([A-Za-z0-9_]+)\s*=\s*\{\s*([^,]+),\s*([^}]+)\s*\};', r'XYPOS \1 = new XYPOS { x = (byte)(\2), y = (byte)(\3) };', text)

# 4. Debounce assignments
text = text.replace('SERVICE1_DEBOUNCE = ((SERVICE1_DEBOUNCE<<1) & 0x0f) | (IN0_CREDIT?1:0);', 'SERVICE1_DEBOUNCE = (byte)(((SERVICE1_DEBOUNCE << 1) & 0x0f) | (IN0_CREDIT != 0 ? 1 : 0));')
text = text.replace('COIN2_DEBOUNCE = ((COIN2_DEBOUNCE<<1) & 0x0f) | (IN0_COIN2?1:0);', 'COIN2_DEBOUNCE = (byte)(((COIN2_DEBOUNCE << 1) & 0x0f) | (IN0_COIN2 != 0 ? 1 : 0));')
text = text.replace('COIN1_DEBOUNCE = ((COIN1_DEBOUNCE<<1) & 0x0f) | (IN0_COIN1?1:0);', 'COIN1_DEBOUNCE = (byte)(((COIN1_DEBOUNCE << 1) & 0x0f) | (IN0_COIN1 != 0 ? 1 : 0));')

# 5. Delegate overload
text = text.replace(' showBonusLifeScore_26b2,', ' (_) => showBonusLifeScore_26b2(),')

# 6. ushort difficulty casts
text = text.replace('ushort diff = (P1_CURR_DIFFICULTY - 1) * 6;', 'ushort diff = (ushort)((P1_CURR_DIFFICULTY - 1) * 6);')
text = text.replace('ushort diff = (P2_CURR_DIFFICULTY - 1) * 6;', 'ushort diff = (ushort)((P2_CURR_DIFFICULTY - 1) * 6);')

# 7. COCKTAIL_MODE
text = text.replace('COCKTAIL_MODE = !IN1_CABINET;', 'COCKTAIL_MODE = (byte)(IN1_CABINET != 0 ? 0 : 1);')

# 8. soundEffectOneChannel_2dee argument 4
text = re.sub(r'soundEffectOneChannel_2dee\s*\(([^,]+),\s*([^,]+),\s*([^,]+),\s*(\d+)\)', r'soundEffectOneChannel_2dee (\1, \2, \3, (byte)\4)', text)

# 9. effect->mask bitwise NOT
text = text.replace('effect->mask &= ~effect->current;', 'effect->mask &= unchecked((byte)~effect->current);')

# 10. Label definitions for jump_2c93 and jump_2d6c
text = text.replace('jump_2c93:\n\n        jump_2c93:', 'jump_2c93:\n            ;')
text = text.replace('jump_2d6c:\n\n        jump_2d6c:', 'jump_2d6c:\n            ;')

if 'jump_2c93:' not in text:
    text = text.replace('addr = effect->freqTable;', 'jump_2c93:\n                addr = effect->freqTable;')
if 'jump_2d6c:' not in text:
    text = text.replace('addr = effect->offset;', 'jump_2d6c:\n                addr = effect->offset;')

# 11. Fix embedded statement label in random_1
text = text.replace('if (dist < 0x40)\n        random_1:', 'if (dist < 0x40)\n        {\n            goto random_1;\n        }\n        random_1:')

with open('PacmanCS/Pacman.cs', 'w') as f:
    f.write(text)

print("Applied fix_exact_11_perfect.py!")
