with open('PacmanCS/Pacman.cs', 'r') as f:
    text = f.read()

import re

# Fix double cast parentheses created by regex matching
text = re.sub(r'fetchOffset_0010\s*\([^\)]+&hl[^\)]*,\s*\(byte\)\(b\*2\)\)', 'fetchOffset_0010(&hl, (byte)(b * 2))', text)
text = re.sub(r'addTask_0042\s*\([^\)]*b[^\)]*,\s*\(byte\)\(c\)\)', 'addTask_0042((byte)(b), (byte)(c))', text)
text = re.sub(r'FRUIT_SPRITE\s*=\s*\(byte\)\(fetchOffset_0010[^\);]+\);', 'FRUIT_SPRITE = fetchOffset_0010(&hl, (byte)(a));', text)
text = re.sub(r'ushort hl = \(byte\)\(calcSquare_2a12[^\);]+\);', 'ushort hl = calcSquare_2a12((byte)(b));', text)
text = re.sub(r'hl \+= \(byte\)\(calcSquare_2a12[^\);]+\);', 'hl += calcSquare_2a12((byte)(b));', text)
text = re.sub(r'badRam_30b5\s*\([^\);]+\);', 'badRam_30b5((byte)(de & 0xff), (byte)(testValue));', text)

with open('PacmanCS/Pacman.cs', 'w') as f:
    f.write(text)

print("Applied fix_syntax_errors.py!")
