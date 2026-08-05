with open('PacmanCS/Pacman.cs', 'r') as f:
    text = f.read()

text = text.replace('int e = fetchOffset_0010(((byte)(byte))(&hl), (byte)(b*2));', 'int e = fetchOffset_0010(&hl, (byte)(b * 2));')
text = text.replace('FRUIT_SPRITE = fetchOffset_0010 (((byte)(byte))(&hl), (byte)(a));', 'FRUIT_SPRITE = fetchOffset_0010(&hl, (byte)a);')
text = text.replace('ushort hl = calcSquare_2a12((byte)((byte))(b));', 'ushort hl = calcSquare_2a12((byte)b);')
text = text.replace('hl += calcSquare_2a12((byte)((byte))(b));', 'hl += calcSquare_2a12((byte)b);')

with open('PacmanCS/Pacman.cs', 'w') as f:
    f.write(text)

print("Applied fix_final_4_exact.py!")
