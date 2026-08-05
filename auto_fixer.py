import subprocess
import re

def build():
    res = subprocess.run(['dotnet', 'build', 'PacmanCS/PacmanCS.csproj'], capture_output=True, text=True)
    out = res.stdout + res.stderr
    errors = re.findall(r'Pacman\.cs\((\d+),(\d+)\):\s*error\s*(CS\d+):\s*(.*)', out)
    return errors, ('Build FAILED.' not in out)

for loop in range(50):
    errors, success = build()
    if success:
        print("=== SUCCESS! 0 BUILD ERRORS! ===")
        break

    print(f"Loop {loop}: {len(errors)} build errors remaining.")

    with open('PacmanCS/Pacman.cs', 'r') as f:
        lines = f.readlines()

    changed = False
    for line_str, col_str, code, msg in sorted(errors, key=lambda x: int(x[0]), reverse=True):
        idx = int(line_str) - 1
        if idx >= len(lines):
            continue
        line = lines[idx]

        if code == 'CS0029': # int/byte to bool conversion
            # if (expr) -> if (expr != 0)
            m = re.search(r'if\s*\(([^)]+)\)', line)
            if m and '!=' not in m.group(1) and '==' not in m.group(1) and '>' not in m.group(1) and '<' not in m.group(1):
                expr = m.group(1).strip()
                lines[idx] = line[:m.start(1)] + f'({expr}) != 0' + line[m.end(1):]
                changed = True
                continue

        elif code == 'CS0266': # int to ushort/byte
            m = re.search(r'(=|\+=|-=|\*=|\/=|&=|\|=|\^=)\s*([^;]+);', line)
            if m:
                target_type = 'byte' if "to 'byte'" in msg else 'ushort'
                expr = m.group(2).strip()
                if not expr.startswith(f'({target_type})('):
                    lines[idx] = line[:m.start(2)] + f'({target_type})({expr});' + line[m.end():]
                    changed = True
                    continue

        elif code == 'CS1503': # argument type mismatch
            col = int(col_str) - 1
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

        elif code == 'CS0159': # missing label
            if 'jump_2c93' in msg:
                lines[idx] = '            goto jump_2c93;\n'
                for k in range(max(0, idx-100), idx):
                    if 'ushort addr = effect->freqTable;' in lines[k]:
                        lines[k] = '            jump_2c93:\n' + lines[k]
                changed = True
            elif 'jump_2d6c' in msg:
                lines[idx] = '            goto jump_2d6c;\n'
                for k in range(max(0, idx-100), idx):
                    if 'ushort addr = effect->offset;' in lines[k]:
                        lines[k] = '            jump_2d6c:\n' + lines[k]
                changed = True

        elif code == 'CS0136': # duplicate local variable scope
            if 'int input;' in line or 'byte input =' in line or 'int input =' in line:
                lines[idx] = line.replace('input', 'input_local')
                for k in range(idx+1, min(idx+10, len(lines))):
                    lines[k] = re.sub(r'\binput\b', 'input_local', lines[k])
                changed = True

        elif code == 'CS0019': # Operator != or || cannot be applied to int/bool/byte
            if 'P1START' in line and 'P2START' in line:
                lines[idx] = '            if (((P1START | P2START) & INPUT_ANYSTART) != 0)\n'
                changed = True
            elif '!= 0' in line:
                lines[idx] = line.replace('!= 0', '')
                changed = True

        elif code == 'CS0030': # Action[] to byte
            lines[idx] = lines[idx].replace('((byte)(func)', 'func').replace('(byte)(func)', 'func')
            changed = True

        elif code == 'CS0123': # delegate overload
            lines[idx] = lines[idx].replace('showBonusLifeScore_26b2', '(_) => showBonusLifeScore_26b2()')
            changed = True

        elif code == 'CS1023': # embedded statement label
            lines[idx] = line.replace('random_1:', 'random_1: ;')
            changed = True

    if changed:
        with open('PacmanCS/Pacman.cs', 'w') as f:
            f.writelines(lines)
    else:
        print("No automated progress on remaining errors.")
        break
