with open('PacmanCS/Pacman.cs', 'r') as f:
    text = f.read()

# 1. Fix line 3151 & 10549
text = text.replace('tableCall_0020 (((byte)(byte)(func)), (byte)(a));', 'tableCall_0020 (func, (byte)(a));')
text = text.replace('tableCall_0020 (((byte)(byte)(func)), (byte)(param));', 'tableCall_0020 (func, (byte)(param));')

# 2. Fix line 7117 input
text = text.replace('int input_7117;', 'int input;')

# 3. Fix jump_2c93_lbl and jump_2d6c_lbl
text = text.replace('goto jump_2c93_lbl;', 'goto jump_2c93;')
text = text.replace('goto jump_2d6c_lbl;', 'goto jump_2d6c;')

# 4. Fix soundEffectClear_2df4 calls
text = text.replace('soundEffectClear_2df4 (effect, (byte)(frequency));', 'soundEffectClear_2df4 (effect, frequency);')
text = text.replace('soundEffectClear_2df4(effect, (byte)(frequency))', 'soundEffectClear_2df4(effect, frequency)')
text = text.replace('soundEffectClear_2df4(effect, (byte)(de))', 'soundEffectClear_2df4(effect, de)')

with open('PacmanCS/Pacman.cs', 'w') as f:
    f.write(text)

print("Applied fix_final_19_exact.py!")
