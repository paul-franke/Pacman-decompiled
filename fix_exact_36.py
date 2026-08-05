import re

with open('PacmanCS/Pacman.cs', 'r') as f:
    text = f.read()

# 1. Address-of property replacements
addr_replacements = {
    '&P1_SCORE': 'P1_SCORE',
    '&P2_SCORE': 'P2_SCORE',
    '&HIGH_SCORE': 'HIGH_SCORE',
    '&GHOST_ANIMATION': '(byte*)(MEM + 0x4dc0)',
    '&RND_NUM_GEN1': '(byte*)(MEM + 0x4c8b)',
    '&COIN_TIMER': '(byte*)(MEM + 0x4dce)',
    '&ORIENTATION_CHANGE_COUNT': '(ushort*)(MEM + 0x4dc2)',
    '&sound->v1FreqCount[0]': 'sound->v1FreqCount',
    '&sound->v2FreqCount[0]': 'sound->v2FreqCount',
    '&sound->v3FreqCount[0]': 'sound->v3FreqCount',
    '&sound->noiseCount[0]': 'sound->noiseCount',
    '&sound->v1VolCount[0]': 'sound->v1VolCount',
    '&sound->v2VolCount[0]': 'sound->v2VolCount',
    '&sound->v3VolCount[0]': 'sound->v3VolCount',
}
for k, v in addr_replacements.items():
    text = text.replace(k, v)

# Fix sound struct array pointers: &sound->v1FreqCount[0] -> sound->v1FreqCount
text = re.sub(r'&\s*([a-zA-Z0-9_\->\.]+)\s*\[\s*0\s*\]', r'\1', text)

# 2. C functions -> C#
text = re.sub(r'\bexit\s*\(([^)]+)\)', r'Environment.Exit(\1)', text)
text = re.sub(r'\bASSERT\s*\(([^)]+)\)', r'assert(\1, "", 0)', text)

# 3. tableCall_0020
text = text.replace('tableCall_0020 ((byte)(func)', 'tableCall_0020 (func')

# 4. Delegates
text = text.replace(' showBonusLifeScore_26b2,', ' (_) => showBonusLifeScore_26b2(),')

# 5. Labels
text = text.replace('goto random_lbl;', 'goto random_1;')
text = text.replace('jump_2c93:\n\n        jump_2c93:', 'jump_2c93:\n            ;')
text = text.replace('jump_2d6c:\n\n        jump_2d6c:', 'jump_2d6c:\n            ;')

# Ensure jump_2c93 and jump_2d6c exist
if 'jump_2c93:' not in text:
    text = text.replace('addr = effect->freqTable;', 'jump_2c93:\n                addr = effect->freqTable;')
if 'jump_2d6c:' not in text:
    text = text.replace('addr = effect->offset;', 'jump_2d6c:\n                addr = effect->offset;')

with open('PacmanCS/Pacman.cs', 'w') as f:
    f.write(text)

print("Applied fix_exact_36.py!")
