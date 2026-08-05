import re

with open('PacmanCS/Pacman.cs', 'r') as f:
    text = f.read()

# 1. exit(x) -> Environment.Exit(x)
text = re.sub(r'\bexit\s*\(([^)]+)\)', r'Environment.Exit(\1)', text)

# 2. ASSERT(cond) -> assert(cond, "", 0)
text = re.sub(r'\bASSERT\s*\(([^)]+)\)', r'assert(\1, "", 0)', text)

# 3. Fix miscasted tableCall_0020 calls: tableCall_0020 ((byte)(func), (byte)(a)) -> tableCall_0020 (func, (byte)(a))
text = re.sub(r'tableCall_0020\s*\(\s*\(byte\)\s*\(\s*func\s*\)', r'tableCall_0020 (func', text)

# 4. Fix byte* to byte mismatch on fetchOffset_0010 calls: fetchOffset_0010 ((byte)(hl), ...) -> fetchOffset_0010 (&hl, ...)
text = re.sub(r'fetchOffset_0010\s*\(\s*\(byte\)\s*\(\s*hl\s*\)', r'fetchOffset_0010 (&hl', text)

# 5. Fix bool != int comparisons like (inInterrupt != 0) -> (inInterrupt)
text = text.replace('(inInterrupt != 0)', 'inInterrupt')
text = text.replace('(cpuPaused != 0)', 'cpuPaused')

with open('PacmanCS/Pacman.cs', 'w') as f:
    f.write(text)

print("Applied fix_final_42.py!")
