import re
import subprocess

with open('PacmanCS/Pacman.cs', 'r') as f:
    text = f.read()

# 1. Replace substate address-of
text = text.replace('&BLINKY_SUBSTATE', '(byte*)(MEM + 0x4da0)')
text = text.replace('&PINKY_SUBSTATE', '(byte*)(MEM + 0x4da1)')
text = text.replace('&INKY_SUBSTATE', '(byte*)(MEM + 0x4da2)')
text = text.replace('&CLYDE_SUBSTATE', '(byte*)(MEM + 0x4da3)')

# 2. Replace other memory mapped property address-of
more_props2 = {
    'RND_NUM_GEN1': '(byte*)(MEM + 0x4c8b)',
    'RND_NUM_GEN2': '(byte*)(MEM + 0x4c8c)',
    'COUNTER_LIMITS_CHANGES': '(byte*)(MEM + 0x4c8a)',
    'SERVICE1_DEBOUNCE': '(byte*)(MEM + 0x4e66)',
    'COIN2_DEBOUNCE': '(byte*)(MEM + 0x4e67)',
    'COIN1_DEBOUNCE': '(byte*)(MEM + 0x4e68)',
    'COIN_COUNTER': '(byte*)(MEM + 0x4e69)',
    'COIN_COUNTER_TIMEOUT': '(byte*)(MEM + 0x4e6a)',
    'COINS_PER_CREDIT': '(byte*)(MEM + 0x4e6b)',
    'PARTIAL_CREDIT': '(byte*)(MEM + 0x4e6c)',
    'CREDITS_PER_COIN': '(byte*)(MEM + 0x4e6d)',
    'LIVES_PER_GAME': '(byte*)(MEM + 0x4e6f)',
    'TWO_PLAYERS': '(byte*)(MEM + 0x4e70)',
    'BONUS_LIFE': '(byte*)(MEM + 0x4e71)',
    'COCKTAIL_MODE': '(byte*)(MEM + 0x4e72)',
    'GHOST_NAMES_MODE': '(byte*)(MEM + 0x4e75)'
}

for prop, ptr in more_props2.items():
    text = text.replace(f'&{prop}', ptr)

with open('PacmanCS/Pacman.cs', 'w') as f:
    f.write(text)

# 3. Process exact build error lines from dotnet build
def run_build():
    res = subprocess.run(['dotnet', 'build', 'PacmanCS/PacmanCS.csproj'], capture_output=True, text=True)
    out = res.stdout + res.stderr
    errors = re.findall(r'Pacman\.cs\((\d+),(\d+)\):\s*error\s*(CS\d+):\s*(.*)', out)
    return errors, ('Build FAILED.' not in out)

for _ in range(10):
    errors, success = run_build()
    if success:
        print("BUILD SUCCESSFUL!")
        break

    with open('PacmanCS/Pacman.cs', 'r') as f:
        lines = f.readlines()

    changed = False
    for line_str, col_str, code, msg in sorted(errors, key=lambda x: int(x[0]), reverse=True):
        idx = int(line_str) - 1
        if idx >= len(lines):
            continue
        line = lines[idx]

        if code == 'CS1503': # cannot convert from int to byte in argument
            m = re.search(r'([a-zA-Z0-9_]+)\s*\(([^)]+)\)', line)
            if m:
                func_name = m.group(1)
                args = [a.strip() for a in m.group(2).split(',')]
                # cast all non-casted arguments to byte
                new_args = []
                for arg in args:
                    if not arg.startswith('(byte)') and not arg.startswith('(ushort)') and not arg.startswith('(byte*)') and not arg.startswith('(ushort*)') and not arg.startswith('ref ') and not arg.startswith('out '):
                        new_args.append(f'(byte)({arg})')
                    else:
                        new_args.append(arg)
                lines[idx] = line[:m.start(2)] + ', '.join(new_args) + line[m.end(2):]
                changed = True
                continue

        elif code == 'CS0023': # Operator ! cannot be applied to int
            m = re.search(r'!([a-zA-Z0-9_\->\.]+)', line)
            if m:
                lines[idx] = line[:m.start()] + f'({m.group(1)} == 0)' + line[m.end():]
                changed = True
                continue

    if changed:
        with open('PacmanCS/Pacman.cs', 'w') as f:
            f.writelines(lines)
    else:
        break

print("Finished fix_54_exact.py!")
