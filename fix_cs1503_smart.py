import subprocess
import re

def build_and_get_errors():
    res = subprocess.run(['dotnet', 'build', 'PacmanCS/PacmanCS.csproj'], capture_output=True, text=True)
    out = res.stdout + res.stderr
    errors = re.findall(r'Pacman\.cs\((\d+),(\d+)\):\s*error\s*(CS\d+):\s*(.*)', out)
    return errors, ('Build FAILED.' not in out)

for iteration in range(20):
    errors, success = build_and_get_errors()
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

        if code == 'CS0159': # No such label
            if 'jump_2c93' in msg:
                lines[idx] = '            goto jump_2c93_lbl;\n'
                for k in range(max(0, idx-50), idx):
                    if 'ushort addr = effect->freqTable;' in lines[k]:
                        lines[k] = '            jump_2c93_lbl:\n' + lines[k]
                changed = True
            elif 'jump_2d6c' in msg:
                lines[idx] = '            goto jump_2d6c_lbl;\n'
                for k in range(max(0, idx-50), idx):
                    if 'ushort addr = effect->offset;' in lines[k]:
                        lines[k] = '            jump_2d6c_lbl:\n' + lines[k]
                changed = True

        elif code == 'CS0841' or code == 'CS0136': # input variable duplicate
            lines[idx] = lines[idx].replace('input_7117', 'input')

        elif code == 'CS0030': # Action[] to byte
            lines[idx] = lines[idx].replace('((byte)(func)', 'func')

        elif code == 'CS0029': # int to bool
            if '=' in line:
                m = re.search(r'(=|\+=|-=)\s*([^;]+);', line)
                if m:
                    lines[idx] = line[:m.start(2)] + f'(byte)({m.group(2)});' + line[m.end():]
                    changed = True

        elif code == 'CS0266': # int to ushort/byte
            m = re.search(r'(=|\+=|-=)\s*([^;]+);', line)
            if m:
                target_type = 'byte' if "to 'byte'" in msg else 'ushort'
                lines[idx] = line[:m.start(2)] + f'({target_type})({m.group(2)});' + line[m.end():]
                changed = True

        elif code == 'CS1503': # argument type conversion
            if '&CH1_FREQ0' in line:
                lines[idx] = line.replace('&CH1_FREQ0', '(byte*)(MEM + 0x4e8c)')
                changed = True
            elif '&CH2_FREQ0' in line:
                lines[idx] = line.replace('&CH2_FREQ0', '(byte*)(MEM + 0x4e92)')
                changed = True
            elif '&CH3_FREQ0' in line:
                lines[idx] = line.replace('&CH3_FREQ0', '(byte*)(MEM + 0x4e97)')
                changed = True
            elif 'soundEffectOneChannel_2dee' in line:
                lines[idx] = re.sub(r'2dee\s*\(([^,]+),\s*([^,]+),\s*([^,]+),\s*([^\)]+)\)', r'2dee (\1, \2, \3, (byte)(\4))', line)
                changed = True

        elif code == 'CS0019': # Operator != cannot be applied to int and bool
            if '!= 0' in line:
                lines[idx] = line.replace('!= 0', '')
                changed = True

        elif code == 'CS1023': # Embedded statement cannot be label
            lines[idx] = line.replace('random_1:', 'random_1: ;')
            changed = True

    if changed:
        with open('PacmanCS/Pacman.cs', 'w') as f:
            f.writelines(lines)
    else:
        print("No more changes could be made automatically.")
        break
