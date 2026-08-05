import subprocess
import re

def get_errors():
    res = subprocess.run(['dotnet', 'build', 'PacmanCS/PacmanCS.csproj'], capture_output=True, text=True)
    out = res.stdout + res.stderr
    errors = re.findall(r'Pacman\.cs\((\d+),(\d+)\):\s*error\s*(CS\d+):\s*(.*)', out)
    return errors, ('Build FAILED.' not in out)

for iteration in range(15):
    errors, success = get_errors()
    if success:
        print("BUILD SUCCESSFUL!")
        break

    print(f"Iteration {iteration}: {len(errors)} errors remaining.")

    with open('PacmanCS/Pacman.cs', 'r') as f:
        lines = f.readlines()

    changed = False
    for line_str, col_str, code, msg in sorted(errors, key=lambda x: int(x[0]), reverse=True):
        idx = int(line_str) - 1
        if idx >= len(lines):
            continue
        line = lines[idx]

        if code == 'CS1503': # cannot convert from int to byte
            if 'tableLookup_0018' in line:
                m = re.search(r'tableLookup_0018\s*\([^\)]+,\s*([^\)]+)\)', line)
                if m:
                    lines[idx] = re.sub(r'tableLookup_0018\s*\(\s*([^,]+)\s*,\s*([^\)]+)\)', r'tableLookup_0018(\1, (byte)(\2))', line)
                    changed = True
                    continue

            col = int(col_str) - 1
            start_paren = line.rfind('(', 0, col)
            if start_paren != -1:
                end_paren = line.find(')', col)
                if end_paren != -1:
                    call_str = line[start_paren+1:end_paren]
                    args = call_str.split(',')
                    new_args = []
                    for arg in args:
                        a = arg.strip()
                        if a and not a.startswith('(byte)') and not a.startswith('(ushort)') and not a.startswith('(byte*)') and not a.startswith('(ushort*)') and not a.startswith('ref ') and not a.startswith('out '):
                            new_args.append(f'(byte)({a})')
                        else:
                            new_args.append(a)
                    lines[idx] = line[:start_paren+1] + ', '.join(new_args) + line[end_paren:]
                    changed = True
                    continue

        elif code == 'CS0266': # cannot implicitly convert type int/ushort to byte/ushort
            if '=' in line:
                m = re.search(r'(=|\+=|-=|\*=|\/=|&=|\|=|\^=)\s*([^;]+);', line)
                if m:
                    expr = m.group(2).strip()
                    cast_type = 'byte' if "to 'byte'" in msg else 'ushort'
                    if not expr.startswith(f'({cast_type})('):
                        lines[idx] = line[:m.start(2)] + f'({cast_type})({expr});' + line[m.end():]
                        changed = True
                        continue

        elif code == 'CS0023':
            m = re.search(r'!([a-zA-Z0-9_\->\.]+)', line)
            if m:
                lines[idx] = line[:m.start()] + f'({m.group(1)} == 0)' + line[m.end():]
                changed = True
                continue

    if changed:
        with open('PacmanCS/Pacman.cs', 'w') as f:
            f.writelines(lines)
    else:
        print("No more changes could be made automatically.")
        break
