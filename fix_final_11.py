import re

with open('PacmanCS/Pacman.cs', 'r') as f:
    text = f.read()

# 1. Line 7117 input
text = text.replace('int input;', 'int input_7117;')
text = re.sub(r'input\b', 'input_7117', text[text.find('int input_7117;'):text.find('int input_7117;') + 800]) + text[text.find('int input_7117;') + 800:]

# 2. Line 9300 & 12729 ushort diff
text = text.replace('ushort diff = (P1_CURR_DIFFICULTY - 1) * 6;', 'ushort diff = (ushort)((P1_CURR_DIFFICULTY - 1) * 6);')
text = text.replace('ushort diff = (P2_CURR_DIFFICULTY - 1) * 6;', 'ushort diff = (ushort)((P2_CURR_DIFFICULTY - 1) * 6);')

# 3. Line 11447 COCKTAIL_MODE
text = re.sub(r'COCKTAIL_MODE\s*=\s*[^;]+;', 'COCKTAIL_MODE = (byte)(IN1_CABINET != 0 ? 0 : 1);', text)

# 4. Line 11701 if statement label
text = text.replace('if (dist < 0x40)\nrandom_1:', 'if (dist < 0x40)\n{\nrandom_1: ;\n}')
text = re.sub(r'random_1:\s*;\s*;\s*;[^\n]+', 'random_1: ;', text)

# 5. jump_2c93 and jump_2d6c labels
text = text.replace('goto jump_2c93;', 'goto jump_2c93_lbl;')
text = text.replace('goto jump_2d6c;', 'goto jump_2d6c_lbl;')
text = text.replace('ushort addr = effect->freqTable;', 'jump_2c93_lbl:\n                ushort addr = effect->freqTable;')
text = text.replace('ushort addr = effect->offset;', 'jump_2d6c_lbl:\n                ushort addr = effect->offset;')

# 6. soundEffectOneChannel_2dee argument 4
text = re.sub(r'soundEffectOneChannel_2dee\s*\(([^,]+),\s*([^,]+),\s*([^,]+),\s*(\d+)\)', r'soundEffectOneChannel_2dee (\1, \2, \3, (byte)\4)', text)

# 7. soundEffectClear_2df4 argument 2
text = text.replace('soundEffectClear_2df4 (effect, (byte)(de));', 'soundEffectClear_2df4 (effect, (byte*)(MEM + 0x4e92));')

# 8. effect->mask assignment
text = re.sub(r'effect->mask\s*&=[^;]+;', 'effect->mask &= unchecked((byte)~effect->current);', text)

# 9. Line 15184 P1START | P2START check
text = re.sub(r'if\s*\(\s*\(P1START[^\)]+\)\s*\{', 'if (((P1START | P2START) & INPUT_ANYSTART) != 0)\n            {', text)

with open('PacmanCS/Pacman.cs', 'w') as f:
    f.write(text)

print("Applied fix_final_11.py!")
