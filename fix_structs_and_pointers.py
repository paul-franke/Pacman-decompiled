import re

with open('PacmanCS/Pacman.cs', 'r') as f:
    text = f.read()

# 1. Struct initializers: XYPOS target = { a, b }; -> XYPOS target = new XYPOS { x = (byte)(a), y = (byte)(b) };
text = re.sub(r'XYPOS\s+([A-Za-z0-9_]+)\s*=\s*\{\s*([^,]+),\s*([^}]+)\s*\};', r'XYPOS \1 = new XYPOS { x = (byte)(\2), y = (byte)(\3) };', text)

# 2. Orientation address of properties -> MEM + offset
text = text.replace('&BLINKY_ORIENTATION', '(MEM + 0x4d2c)')
text = text.replace('&PINKY_ORIENTATION', '(MEM + 0x4d2d)')
text = text.replace('&INKY_ORIENTATION', '(MEM + 0x4d2e)')
text = text.replace('&CLYDE_ORIENTATION', '(MEM + 0x4d2f)')
text = text.replace('&PACMAN_ORIENTATION', '(MEM + 0x4d30)')

# 3. Task list addresses
text = text.replace('&TASK_LIST_BEGIN', '(ushort*)(MEM + 0x4c82)')
text = text.replace('&TASK_LIST_END', '(ushort*)(MEM + 0x4c80)')

# 4. Other ref byte property address-of
text = text.replace('&IO_INPUT0', '(byte*)Unsafe.AsPointer(ref IO_INPUT0)')
text = text.replace('&IO_INPUT1', '(byte*)Unsafe.AsPointer(ref IO_INPUT1)')
text = text.replace('&DIP_INPUT', '(byte*)Unsafe.AsPointer(ref DIP_INPUT)')

with open('PacmanCS/Pacman.cs', 'w') as f:
    f.write(text)

print("Updated struct initializers and orientation pointers!")
