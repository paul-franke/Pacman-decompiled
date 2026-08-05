import re

with open('PacmanCS/Pacman.cs', 'r') as f:
    lines = f.readlines()

# Line 903 & 920
lines[902] = lines[902].replace('(IN0_COIN2?1:0)', '(IN0_COIN2 != 0 ? (byte)1 : (byte)0)')
lines[919] = lines[919].replace('(IN0_COIN1?1:0)', '(IN0_COIN1 != 0 ? (byte)1 : (byte)0)')

# Line 1173 & 2066
lines[1172] = lines[1172].replace('!= 0', '')
lines[2065] = lines[2065].replace('!= 0', '')

# Line 3151 & 10549
lines[3150] = lines[3150].replace('((byte)(func)', 'func')
lines[10548] = lines[10548].replace('((byte)(func)', 'func')

# Line 7117
lines[7116] = lines[7116].replace('byte input =', 'byte input_7117 =').replace('input', 'input_7117')

# Line 9300 & 12729
lines[9299] = re.sub(r'=\s*([^;]+);', r'= (ushort)(\1);', lines[9299])
lines[12728] = re.sub(r'=\s*([^;]+);', r'= (ushort)(\1);', lines[12728])

# Line 10521
lines[10520] = lines[10520].replace('showBonusLifeScore_26b2', '(_) => showBonusLifeScore_26b2()')

# Line 11447
lines[11446] = re.sub(r'if\s*\(([^)]+)\)', r'if ((\1) != 0)', lines[11446])

# Line 11701
lines[11700] = 'random_1: ;\n'

# Line 13288 & 13529
for i in range(13280, 13300):
    if 'addr = effect->freqTable;' in lines[i]:
        lines[i] = 'jump_2c93:\n' + lines[i]

for i in range(13520, 13540):
    if 'addr = effect->offset;' in lines[i]:
        lines[i] = 'jump_2d6c:\n' + lines[i]

# Line 13411, 13638, 14183 (soundEffectClear_2df4)
lines[13410] = lines[13410].replace('&CH1_FREQ0', '(byte*)(MEM + 0x4e8c)')
lines[13637] = lines[13637].replace('&CH2_FREQ0', '(byte*)(MEM + 0x4e92)')
lines[14182] = lines[14182].replace('&CH3_FREQ0', '(byte*)(MEM + 0x4e97)')

# Line 13625, 13914 (soundEffectOneChannel_2dee argument 4)
lines[13624] = re.sub(r'2dee\s*\(([^,]+),\s*([^,]+),\s*([^,]+),\s*([^\)]+)\)', r'2dee (\1, \2, \3, (byte)(\4))', lines[13624])
lines[13913] = re.sub(r'2dee\s*\(([^,]+),\s*([^,]+),\s*([^,]+),\s*([^\)]+)\)', r'2dee (\1, \2, \3, (byte)(\4))', lines[13913])

# Line 13985
lines[13984] = re.sub(r'=\s*([^;]+);', r'= (byte)(\1);', lines[13984])

# Line 15177 & 15184
lines[15176] = lines[15176].replace('!= 0', '')
lines[15183] = lines[15183].replace('inInterrupt != 0', 'inInterrupt').replace('cpuPaused != 0', 'cpuPaused')

with open('PacmanCS/Pacman.cs', 'w') as f:
    f.writelines(lines)

print("Applied fix_exact_22_lines.py!")
