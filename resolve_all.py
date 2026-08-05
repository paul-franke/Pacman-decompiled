import subprocess
import re

def get_build_errors():
    res = subprocess.run(['dotnet', 'build', 'PacmanCS/PacmanCS.csproj'], capture_output=True, text=True)
    out = res.stdout + res.stderr
    errors = re.findall(r'Pacman\.cs\((\d+),(\d+)\):\s*error\s*(CS\d+):\s*(.*)', out)
    return errors, ('Build FAILED.' not in out)

def fix_all():
    for iteration in range(30):
        errors, success = get_build_errors()
        if success:
            print("SUCCESS! Build succeeded with 0 errors!")
            return True

        print(f"Iteration {iteration}: {len(errors)} build errors remain.")
        with open('PacmanCS/Pacman.cs', 'r') as f:
            lines = f.readlines()

        changed = False
        for line_str, col_str, code, msg in sorted(errors, key=lambda x: int(x[0]), reverse=True):
            line_idx = int(line_str) - 1
            col_idx = int(col_str) - 1
            if line_idx >= len(lines):
                continue
            line = lines[line_idx]

            if code == 'CS0622':
                # Action[] func = { ... } -> Action[] func = new Action[] { ... }
                if '{' in line:
                    lines[line_idx] = re.sub(r'(\b[A-Za-z0-9_<>]+\[\]\s+[A-Za-z0-9_]+\s*=)\s*\{', r'\1 new[] {', line)
                    changed = True
                    continue

            elif code == 'CS0212':
                # &pointer[index] -> (pointer + index)
                m = re.search(r'&\s*([A-Za-z0-9_\->\.]+)\s*\[([^\]]+)\]', line)
                if m:
                    lines[line_idx] = line[:m.start()] + f'({m.group(1)} + {m.group(2)})' + line[m.end():]
                    changed = True
                    continue

            elif code == 'CS0136':
                # Local variable 'b' shadows outer scope
                lines[line_idx] = re.sub(r'\b(int|byte|ushort)\s+b\b', r'\1 b_local', line)
                lines[line_idx] = lines[line_idx].replace(' b)', ' b_local)')
                changed = True
                continue

            elif code in ['CS0158', 'CS0140']:
                # Duplicate label 'random'
                if 'random:' in line:
                    lines[line_idx] = line.replace('random:', f'random_{line_idx}:')
                    changed = True
                    continue
                elif 'goto random;' in line:
                    lines[line_idx] = line.replace('goto random;', f'goto random_{line_idx};')
                    changed = True
                    continue

            elif code in ['CS0266', 'CS1503']:
                # Cannot convert int to byte / ushort
                if "to 'byte'" in msg or "to 'sbyte'" in msg:
                    if '=' in line:
                        m = re.search(r'(=|\+=|-=|\*=|\/=|&=|\|=|\^=)\s*([^;]+);', line)
                        if m:
                            expr = m.group(2).strip()
                            if not expr.startswith('(byte)('):
                                lines[line_idx] = line[:m.start(2)] + f'(byte)({expr});' + line[m.end():]
                                changed = True
                                continue
                elif "to 'ushort'" in msg:
                    if '=' in line:
                        m = re.search(r'(=|\+=|-=|\*=|\/=|&=|\|=|\^=)\s*([^;]+);', line)
                        if m:
                            expr = m.group(2).strip()
                            if not expr.startswith('(ushort)('):
                                lines[line_idx] = line[:m.start(2)] + f'(ushort)({expr});' + line[m.end():]
                                changed = True
                                continue

            elif code in ['CS0029', 'CS0023']:
                # Cannot convert int/byte to bool or ! on int
                m = re.search(r'\b(if|while)\s*\((.+)\)', line)
                if m:
                    kw = m.group(1)
                    cond = m.group(2).strip()
                    if not cond.endswith('!= 0') and not cond.endswith('== 0'):
                        lines[line_idx] = line[:m.start()] + f'{kw} (({cond}) != 0)' + line[m.end():]
                        changed = True
                        continue

            elif code == 'CS0019':
                if '||' in line:
                    lines[line_idx] = re.sub(r'(\b[a-zA-Z0-9_\->\.]+\b)\s*\|\|\s*(\b[a-zA-Z0-9_\->\.]+\b)', r'(\1 != 0 || \2 != 0)', line)
                    changed = True
                    continue
                elif '&&' in line:
                    lines[line_idx] = re.sub(r'(\b[a-zA-Z0-9_\->\.]+\b)\s*&&\s*(\b[a-zA-Z0-9_\->\.]+\b)', r'(\1 != 0 && \2 != 0)', line)
                    changed = True
                    continue
                elif '!=' in line:
                    m = re.search(r'\(([^)]+)\)\s*!=\s*0', line)
                    if m:
                        lines[line_idx] = line.replace(m.group(0), f'{m.group(1)}')
                        changed = True
                        continue

        if changed:
            with open('PacmanCS/Pacman.cs', 'w') as f:
                f.writelines(lines)
        else:
            print("No more automated changes could be applied.")
            break

fix_all()
