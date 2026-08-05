import subprocess
import re

def run_build():
    res = subprocess.run(['dotnet', 'build', 'PacmanCS/PacmanCS.csproj'], capture_output=True, text=True)
    return res.stdout + res.stderr

def fix_errors():
    for iteration in range(10):
        output = run_build()
        if 'Build FAILED.' not in output:
            print("BUILD SUCCEEDED!")
            return True

        errors = re.findall(r'Pacman\.cs\((\d+),(\d+)\):\s*error\s*(CS\d+):\s*(.*)', output)
        print(f"Iteration {iteration}: {len(errors)} errors found.")
        if not errors:
            print(output[:2000])
            break

        with open('PacmanCS/Pacman.cs', 'r') as f:
            lines = f.readlines()

        changed = False
        # Sort errors in reverse line order so edits don't shift line numbers
        for line_str, col_str, code, msg in sorted(errors, key=lambda x: int(x[0]), reverse=True):
            line_num = int(line_str) - 1
            if line_num >= len(lines):
                continue
            line = lines[line_num]

            if code == 'CS0266' or code == 'CS1503': # Cannot implicitly convert / argument type
                # Check if it's an assignment to a byte/ushort variable or array
                if 'to \'byte\'' in msg or 'from \'int\' to \'byte\'' in msg or 'from \'ushort\' to \'byte\'' in msg:
                    # Match assignment: target = expr;
                    m = re.search(r'(=|\+=|-=|\*=|\/=|&=|\|=|\^=)\s*([^;]+);', line)
                    if m:
                        op = m.group(1)
                        expr = m.group(2).strip()
                        if not expr.startswith('(byte)('):
                            new_line = line[:m.start(2)] + f'(byte)({expr});' + line[m.end():]
                            lines[line_num] = new_line
                            changed = True
                            continue
                elif 'to \'ushort\'' in msg or 'from \'int\' to \'ushort\'' in msg:
                    m = re.search(r'(=|\+=|-=|\*=|\/=|&=|\|=|\^=)\s*([^;]+);', line)
                    if m:
                        expr = m.group(2).strip()
                        if not expr.startswith('(ushort)('):
                            new_line = line[:m.start(2)] + f'(ushort)({expr});' + line[m.end():]
                            lines[line_num] = new_line
                            changed = True
                            continue

            elif code == 'CS0029' or code == 'CS0023' or code == 'CS0019': # Cannot convert int/byte to bool / operator !
                # Fix if (expr) -> if ((expr) != 0) or while (expr) -> while ((expr) != 0)
                m = re.search(r'\b(if|while)\s*\(([^)]+)\)', line)
                if m:
                    kw = m.group(1)
                    cond = m.group(2).strip()
                    if not cond.endswith('!= 0') and not cond.endswith('== 0'):
                        new_line = line[:m.start()] + f'{kw} (({cond}) != 0)' + line[m.end():]
                        lines[line_num] = new_line
                        changed = True
                        continue

            elif code == 'CS0103': # The name 'X' does not exist
                if 'CH1_FREQ' in msg or 'CH2_FREQ' in msg or 'CH3_FREQ' in msg:
                    # Fix missing macro or definition
                    pass

        if changed:
            with open('PacmanCS/Pacman.cs', 'w') as f:
                f.writelines(lines)
        else:
            print("No automatic changes could be applied in this step.")
            break

    return False

fix_errors()
