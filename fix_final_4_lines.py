import re

with open('PacmanCS/Pacman.cs', 'r') as f:
    text = f.read()

# Fix multi-cast parentheses cleanly
text = re.sub(r'\(\s*\(byte\)\s*\(byte\)\s*([^;\),]+)\)', r'(byte)(\1)', text)
text = re.sub(r'\(\s*\(ushort\)\s*\(ushort\)\s*([^;\),]+)\)', r'(ushort)(\1)', text)

# Explicit target fixes
text = text.replace('addTask_0042 (((byte)(byte))(((byte))(byte))(b), (byte)(c));', 'addTask_0042((byte)b, (byte)c);')
text = text.replace('ushort msgDataAddr = tableLookup_0018(DATA_MSG_TABLE_36a5, (byte)((byte))(msg));', 'ushort msgDataAddr = tableLookup_0018(DATA_MSG_TABLE_36a5, (byte)msg);')
text = text.replace('addr = tableLookup_0018(table, (byte)((byte))(bit));', 'addr = tableLookup_0018(table, (byte)bit);')
text = text.replace('badRam_30b5 ((byte)((byte))(de & 0xff), (byte)(testValue));', 'badRam_30b5((byte)(de & 0xff), (byte)testValue);')

with open('PacmanCS/Pacman.cs', 'w') as f:
    f.write(text)

print("Applied fix_final_4_lines.py!")
