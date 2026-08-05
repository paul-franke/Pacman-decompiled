with open('PacmanCS/Pacman.cs', 'r') as f:
    text = f.read()

import re

# 1. Fix fetchOffset_0010 calls
text = text.replace('fetchOffset_0010 ((byte)(hl),', 'fetchOffset_0010 (&hl,')

# 2. Fix tableCall_0020 calls
text = text.replace('tableCall_0020 ((byte)(func),', 'tableCall_0020 (func,')

# 3. Fix XYPOS target = { x, y }
text = re.sub(r'XYPOS\s+([A-Za-z0-9_]+)\s*=\s*\{\s*([^,]+),\s*([^}]+)\s*\};', r'XYPOS \1 = new XYPOS { x = (byte)(\2), y = (byte)(\3) };', text)

# 4. Fix int/byte condition expressions
text = text.replace('(inInterrupt != 0)', 'inInterrupt')
text = text.replace('(cpuPaused != 0)', 'cpuPaused')
text = text.replace('!INTENABLE', '(INTENABLE == 0)')

# 5. Fix specific lines
text = text.replace('int e = (byte)(fetchOffset_0010(((byte)(byte))(&hl), (byte)(b*2)));', 'int e = fetchOffset_0010(&hl, (byte)(b * 2));')
text = text.replace('addTask_0042 (((byte)(byte))(((byte))(byte))(b), (byte)(c));', 'addTask_0042((byte)(b), (byte)(c));')
text = text.replace('addTask_0042 ((byte)(((byte))(byte))(b), (byte)(c));', 'addTask_0042((byte)(b), (byte)(c));')
text = text.replace('FRUIT_SPRITE = (byte)(fetchOffset_0010 (((byte)(byte))(&hl), (byte)(a)));', 'FRUIT_SPRITE = fetchOffset_0010(&hl, (byte)(a));')
text = text.replace('ushort hl = (byte)(calcSquare_2a12((byte)((byte))(b)));', 'ushort hl = calcSquare_2a12((byte)(b));')
text = text.replace('hl += (byte)(calcSquare_2a12((byte)((byte))(b)));', 'hl += calcSquare_2a12((byte)(b));')
text = text.replace('badRam_30b5 ((byte)((byte))(de & 0xff), (byte)(testValue));', 'badRam_30b5((byte)(de & 0xff), (byte)(testValue));')
text = text.replace('effect->mask &= unchecked((byte)~effec)t->current;', 'effect->mask &= unchecked((byte)~effect->current);')

with open('PacmanCS/Pacman.cs', 'w') as f:
    f.write(text)

print("Applied apply_final_6_fixes.py!")
