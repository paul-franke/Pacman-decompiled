import re

with open('PacmanCS/Pacman.cs', 'r') as f:
    text = f.read()

# 1. Debounce expressions
text = text.replace('SERVICE1_DEBOUNCE = (byte)(((SERVICE1_DEBOUNCE<<1) & 0x0f) | (IN0_CREDIT?1:0));', 'SERVICE1_DEBOUNCE = (byte)(((SERVICE1_DEBOUNCE << 1) & 0x0f) | (IN0_CREDIT != 0 ? 1 : 0));')
text = text.replace('COIN2_DEBOUNCE = (byte)(((COIN2_DEBOUNCE<<1) & 0x0f) | (IN0_COIN2?1:0));', 'COIN2_DEBOUNCE = (byte)(((COIN2_DEBOUNCE << 1) & 0x0f) | (IN0_COIN2 != 0 ? 1 : 0));')
text = text.replace('COIN1_DEBOUNCE = (byte)(((COIN1_DEBOUNCE<<1) & 0x0f) | (IN0_COIN1?1:0));', 'COIN1_DEBOUNCE = (byte)(((COIN1_DEBOUNCE << 1) & 0x0f) | (IN0_COIN1 != 0 ? 1 : 0));')

# 2. Boolean interrupt/paused comparisons
text = text.replace('if (inInterrupt != 0)', 'if (inInterrupt)')
text = text.replace('if (cpuPaused != 0)', 'if (cpuPaused)')

# 3. tableCall_0020 miscast
text = text.replace('tableCall_0020 ((byte)(func)', 'tableCall_0020 (func')

# 4. Pointer null check
text = text.replace('chr != 0', 'chr != null')

# 5. Overload delegate
text = text.replace(' showBonusLifeScore_26b2,', ' (_) => showBonusLifeScore_26b2(),')

# 6. Fix labels jump_2c93 and jump_2d6c
if 'jump_2c93:' not in text:
    text = text.replace('ushort addr = effect->freqTable;', 'jump_2c93:\n                ushort addr = effect->freqTable;')
if 'jump_2d6c:' not in text:
    text = text.replace('ushort addr = effect->offset;', 'jump_2d6c:\n                ushort addr = effect->offset;')

text = text.replace('jump_2c93:\n\n        jump_2c93:', 'jump_2c93:\n            ;')
text = text.replace('jump_2d6c:\n\n        jump_2d6c:', 'jump_2d6c:\n            ;')

with open('PacmanCS/Pacman.cs', 'w') as f:
    f.write(text)

print("Applied fix_final_21_exact.py!")
