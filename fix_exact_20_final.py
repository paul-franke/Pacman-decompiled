import re

with open('PacmanCS/Pacman.cs', 'r') as f:
    text = f.read()

# 1. Line 7593 typo
text = text.replace('if (SCREEN[addr] != CHAR_PILL &(SCREEN + addr) != CHAR_POWERUP)', 'if (SCREEN[addr] != CHAR_PILL && SCREEN[addr] != CHAR_POWERUP)')

# 2. Debounce expressions
text = text.replace('COIN2_DEBOUNCE = ((COIN2_DEBOUNCE<<1) & 0x0f) | (IN0_COIN2?1:0);', 'COIN2_DEBOUNCE = (byte)(((COIN2_DEBOUNCE << 1) & 0x0f) | (IN0_COIN2 != 0 ? 1 : 0));')
text = text.replace('COIN1_DEBOUNCE = ((COIN1_DEBOUNCE<<1) & 0x0f) | (IN0_COIN1?1:0);', 'COIN1_DEBOUNCE = (byte)(((COIN1_DEBOUNCE << 1) & 0x0f) | (IN0_COIN1 != 0 ? 1 : 0));')

# 3. Boolean comparisons
text = text.replace('if (inInterrupt != 0)', 'if (inInterrupt)')
text = text.replace('if (cpuPaused != 0)', 'if (cpuPaused)')
text = text.replace('!inInterrupt != 0', '!inInterrupt')
text = text.replace('!cpuPaused != 0', '!cpuPaused')

# 4. tableCall_0020
text = text.replace('tableCall_0020 ((byte)(func),', 'tableCall_0020 (func,')

# 5. Overload delegate
text = text.replace(' showBonusLifeScore_26b2,', ' (_) => showBonusLifeScore_26b2(),')

# 6. ushort casts
text = text.replace('ushort diff = (P1_CURR_DIFFICULTY - 1) * 6;', 'ushort diff = (ushort)((P1_CURR_DIFFICULTY - 1) * 6);')
text = text.replace('ushort diff = (P2_CURR_DIFFICULTY - 1) * 6;', 'ushort diff = (ushort)((P2_CURR_DIFFICULTY - 1) * 6);')

# 7. Label statement fix
text = text.replace('random_1:\n                {}', 'random_1:\n                ;')

# 8. Add jump_2c93 and jump_2d6c labels
if 'jump_2c93:' not in text:
    text = text.replace('ushort addr = effect->freqTable;', 'jump_2c93:\n            ushort addr = effect->freqTable;')
if 'jump_2d6c:' not in text:
    text = text.replace('ushort addr = effect->offset;', 'jump_2d6c:\n            ushort addr = effect->offset;')

# 9. Parameter casts for sound functions
text = re.sub(r'soundEffectClear_2df4\s*\(\s*effect\s*,\s*([^\)]+)\)', r'soundEffectClear_2df4(effect, (byte)(\1))', text)

with open('PacmanCS/Pacman.cs', 'w') as f:
    f.write(text)

print("Applied fix_exact_20_final.py!")
