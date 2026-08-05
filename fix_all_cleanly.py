import re

with open('PacmanCS/Pacman.cs', 'r') as f:
    text = f.read()

# 1. Fix line 1815
text = text.replace('addTask_0042((byte)b, (byte)c);', 'addTask_0042((byte)TASK_DISPLAY_MSG, (byte)c);')

# 2. Fix CS0266 (int -> ushort) on line 166: return (ushort)(e | (d << 8));
text = text.replace('return e | (d << 8);', 'return (ushort)(e | (d << 8));')

# 3. Address-of property pointers
props = {
    'GHOST_ANIMATION': '(byte*)(MEM + 0x4dc0)',
    'RND_NUM_GEN1': '(byte*)(MEM + 0x4c8b)',
    'COIN_TIMER': '(byte*)(MEM + 0x4dce)',
    'ORIENTATION_CHANGE_COUNT': '(ushort*)(MEM + 0x4dc2)'
}
for p, v in props.items():
    text = text.replace(f'&{p}', v)

# 4. Shadowing variable input
lines = text.splitlines()
for i in range(len(lines)):
    if 'byte input =' in lines[i] or 'int input =' in lines[i]:
        lines[i] = lines[i].replace('input =', 'input_local =')
        for j in range(i+1, min(i+10, len(lines))):
            lines[j] = re.sub(r'\binput\b', 'input_local', lines[j])
text = '\n'.join(lines)

# 5. Fix byte* != byte comparison on line 7593 (byte* != null)
text = text.replace('byte *chr = (ROM + msgDataAddr+1);', 'byte *chr = (byte*)(ROM + msgDataAddr+1);')

# 6. Fix goto random / random label duplication
text = text.replace('random:', 'random_lbl:')
text = text.replace('goto random;', 'goto random_lbl;')

with open('PacmanCS/Pacman.cs', 'w') as f:
    f.write(text)

print("Applied fix_all_cleanly.py!")
