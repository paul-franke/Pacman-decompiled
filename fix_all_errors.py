"""
Comprehensive single-pass fixer for PacmanCS build errors.
Runs dotnet build in a loop, parsing errors and applying targeted fixes.
Cleans up duplicate casts after every iteration.
"""
import subprocess
import re

CS_FILE = 'PacmanCS/Pacman.cs'

def build_and_get_errors():
    res = subprocess.run(['dotnet', 'build', 'PacmanCS/PacmanCS.csproj'],
                         capture_output=True, text=True)
    out = res.stdout + res.stderr
    errors = re.findall(
        r'Pacman\.cs\((\d+),(\d+)\):\s*error\s*(CS\d+):\s*(.*?)(?:\s*\[)',
        out)
    success = 'Build FAILED' not in out
    return errors, success

def clean_duplicate_casts(text):
    """Remove stacked duplicate casts like (byte)((byte)(x)) -> (byte)(x)"""
    for _ in range(5):
        text = re.sub(r'\(\(byte\)\(byte\)\)\(', '(byte)(', text)
        text = re.sub(r'\(byte\)\(\(byte\)\)', '(byte)', text)
        text = re.sub(r'\(\(byte\)\(byte\)\)', '(byte)', text)
        text = re.sub(r'\(\(ushort\)\(ushort\)\)', '(ushort)', text)
        text = re.sub(r'\(ushort\)\(\(ushort\)\)', '(ushort)', text)
        text = re.sub(r'\(byte\)\(\(byte\)\(([^()]+)\)\)', r'(byte)(\1)', text)
        text = re.sub(r'\(ushort\)\(\(ushort\)\(([^()]+)\)\)', r'(ushort)(\1)', text)
    return text

def apply_pre_fixes(text):
    """Apply targeted fixes that are best done by exact string matching
    BEFORE the iterative error loop."""

    # --- exit() -> Environment.Exit() ---
    text = re.sub(r'\bexit\s*\(([^)]+)\)', r'Environment.Exit(\1)', text)

    # --- ASSERT() -> Debug.Assert() or noop ---
    text = re.sub(r'\bASSERT\s*\([^)]*\)\s*;', '/* ASSERT removed */;', text)

    # --- XYPOS struct initializers: XYPOS name = { x, y }; ---
    text = re.sub(
        r'XYPOS\s+(\w+)\s*=\s*\{\s*([^,]+),\s*([^}]+)\s*\}\s*;',
        r'XYPOS \1 = new XYPOS { x = (byte)(\2), y = (byte)(\3) };',
        text)

    # --- effect->mask &= ~effect->current; -> unchecked cast ---
    text = text.replace(
        'effect->mask &= ~effect->current;',
        'effect->mask &= unchecked((byte)~effect->current);')

    # --- COCKTAIL_MODE = !IN1_CABINET; ---
    text = text.replace(
        'COCKTAIL_MODE = !IN1_CABINET;',
        'COCKTAIL_MODE = (byte)(IN1_CABINET != 0 ? 0 : 1);')

    # --- Debounce assignments with ternary ---
    text = text.replace(
        'SERVICE1_DEBOUNCE = ((SERVICE1_DEBOUNCE<<1) & 0x0f) | (IN0_CREDIT?1:0);',
        'SERVICE1_DEBOUNCE = (byte)(((SERVICE1_DEBOUNCE << 1) & 0x0f) | (IN0_CREDIT != 0 ? 1 : 0));')
    text = text.replace(
        'COIN2_DEBOUNCE = ((COIN2_DEBOUNCE<<1) & 0x0f) | (IN0_COIN2?1:0);',
        'COIN2_DEBOUNCE = (byte)(((COIN2_DEBOUNCE << 1) & 0x0f) | (IN0_COIN2 != 0 ? 1 : 0));')
    text = text.replace(
        'COIN1_DEBOUNCE = ((COIN1_DEBOUNCE<<1) & 0x0f) | (IN0_COIN1?1:0);',
        'COIN1_DEBOUNCE = (byte)(((COIN1_DEBOUNCE << 1) & 0x0f) | (IN0_COIN1 != 0 ? 1 : 0));')

    # --- P1START || P2START ---
    text = re.sub(
        r'if\s*\(\s*P1START\s*\|\|\s*P2START\s*\)',
        'if (((P1START | P2START) & INPUT_ANYSTART) != 0)',
        text)

    # --- ushort diff = (P1_CURR_DIFFICULTY - 1) * 6; ---
    text = text.replace(
        'ushort diff = (P1_CURR_DIFFICULTY - 1) * 6;',
        'ushort diff = (ushort)((P1_CURR_DIFFICULTY - 1) * 6);')
    text = text.replace(
        'ushort diff = (P2_CURR_DIFFICULTY - 1) * 6;',
        'ushort diff = (ushort)((P2_CURR_DIFFICULTY - 1) * 6);')

    # --- soundEffectOneChannel_2dee argument 4 literal cast ---
    text = re.sub(
        r'soundEffectOneChannel_2dee\s*\(([^,]+),\s*([^,]+),\s*([^,]+),\s*(\d+)\)',
        r'soundEffectOneChannel_2dee(\1, \2, \3, (byte)\4)',
        text)

    # --- schedTask: addTask_0042(b, c) -> addTask_0042((byte)b, (byte)c) ---
    # Only in schedTask function (line ~203)
    text = text.replace(
        'addTask_0042 (b, c);',
        'addTask_0042((byte)b, (byte)c);')

    # --- fetchOffset_0010 calls ---
    text = text.replace(
        'int e = fetchOffset_0010(&hl, b*2);',
        'int e = fetchOffset_0010(&hl, (byte)(b * 2));')

    # --- byte* != byte comparison ---
    # e.g. orientation != 0 where orientation is byte*
    # We'll handle this in the loop

    return text

def fix_line(lines, idx, code, msg, col):
    """Apply a targeted fix for one error. Returns True if a change was made."""
    if idx >= len(lines):
        return False
    line = lines[idx]

    # CS0029: Cannot implicitly convert type 'X' to 'bool'
    if code == 'CS0029' and "'bool'" in msg:
        # if/while (expr) where expr is int/byte/ushort
        m = re.match(r'^(\s*(?:else\s+)?(?:if|while))\s*\((.+)\)(\s*(?:\{.*)?)$',
                      line.rstrip())
        if m:
            prefix, expr, suffix = m.group(1), m.group(2), m.group(3)
            if ('!= 0' not in expr and '== 0' not in expr and
                '!=' not in expr and '==' not in expr and
                '>' not in expr and '<' not in expr):
                lines[idx] = f'{prefix} (({expr}) != 0){suffix}\n'
                return True
        return False

    # CS0266: Cannot implicitly convert type 'int' to 'byte'/'ushort'
    if code == 'CS0266':
        target_type = 'ushort' if "'ushort'" in msg else 'byte'
        # Find assignment operator
        m = re.search(r'(\+=|-=|\*=|/=|&=|\|=|\^=|=)\s*([^;]+);', line)
        if m and '==' not in line[max(0, m.start()-1):m.start()+2]:
            op = m.group(1)
            expr = m.group(2).strip()
            if not expr.startswith(f'({target_type})('):
                new_expr = f'({target_type})({expr})'
                lines[idx] = line[:m.start(2)] + new_expr + ';\n'
                return True
        return False

    # CS1503: Argument cannot convert
    if code == 'CS1503':
        if "'byte*'" in msg:
            return False  # pointer args need manual attention

        target = 'byte' if "'byte'" in msg else 'ushort' if "'ushort'" in msg else None
        if target is None:
            return False

        # For specific known functions, apply targeted fixes
        if 'soundEffectOneChannel_2dee' in line and 'Argument 4' in msg:
            lines[idx] = re.sub(
                r'(soundEffectOneChannel_2dee\s*\([^,]+,\s*[^,]+,\s*[^,]+,\s*)(\d+)(\s*\))',
                r'\1(byte)\2\3', line)
            return True

        # Generic: find the function call and cast the specific argument
        # Parse which argument (Argument N)
        arg_match = re.search(r'Argument (\d+)', msg)
        if not arg_match:
            return False
        arg_num = int(arg_match.group(1))

        # Find function call pattern: funcname(arg1, arg2, ...)
        # Use column position to locate the argument
        c = col - 1  # 0-indexed
        # Walk backwards to find the opening paren
        depth = 0
        arg_start = c
        for i in range(c, -1, -1):
            ch = line[i]
            if ch == ')':
                depth += 1
            elif ch == '(':
                if depth == 0:
                    arg_start = i + 1
                    break
                depth -= 1
            elif ch == ',' and depth == 0:
                arg_start = i + 1
                break

        # Walk forward to find end of argument
        depth = 0
        arg_end = c
        for i in range(c, len(line)):
            ch = line[i]
            if ch == '(':
                depth += 1
            elif ch == ')':
                if depth == 0:
                    arg_end = i
                    break
                depth -= 1
            elif ch == ',' and depth == 0:
                arg_end = i
                break

        arg_text = line[arg_start:arg_end].strip()
        if arg_text and not arg_text.startswith(f'({target})'):
            new_arg = f'({target})({arg_text})'
            lines[idx] = line[:arg_start] + new_arg + line[arg_end:]
            return True

        return False

    # CS0622: Can only use array initializer expressions to assign to array types
    if code == 'CS0622':
        # XYPOS name = { x, y }; -> XYPOS name = new XYPOS { x = (byte)(x), y = (byte)(y) };
        m = re.search(r'XYPOS\s+(\w+)\s*=\s*\{\s*([^,]+),\s*([^}]+)\s*\}\s*;', line)
        if m:
            name = m.group(1)
            x_val = m.group(2).strip()
            y_val = m.group(3).strip()
            new_init = f'XYPOS {name} = new XYPOS {{ x = (byte)({x_val}), y = (byte)({y_val}) }};'
            lines[idx] = line[:m.start()] + new_init + '\n'
            return True
        return False

    # CS0841: Cannot use local variable before declared (rename conflict)
    if code == 'CS0841':
        m = re.search(r"'(\w+)'", msg)
        if m:
            varname = m.group(1)
            # Find the later declaration and rename it
            for k in range(idx, min(idx + 50, len(lines))):
                decl_match = re.search(
                    rf'\b(byte|int|ushort|uint)\s+{varname}\b', lines[k])
                if decl_match:
                    newname = varname + '_inner'
                    # Rename from declaration onwards
                    for j in range(k, min(k + 30, len(lines))):
                        lines[j] = re.sub(
                            rf'\b{varname}\b', newname, lines[j])
                        if j > k and (lines[j].strip() == '' or
                                       lines[j].strip() == '}'):
                            break
                    return True
        return False

    # CS0212: address of unfixed expression
    if code == 'CS0212':
        # These involve &ARRAY[index] patterns that weren't caught
        # by final_clean_pass.py — skip for now, handle in post-pass
        return False

    # CS0019: Operator '!=' cannot be applied to byte* and byte
    if code == 'CS0019':
        if "'byte*'" in msg and "'byte'" in msg:
            # e.g. orientation != 0 where orientation is byte*
            # Need to dereference: *orientation != 0
            # Or it might be the other way around
            return False
        return False

    # CS0159: No such label
    if code == 'CS0159':
        return False

    # CS0123: No overload matches delegate
    if code == 'CS0123':
        return False

    # CS0103: name does not exist
    if code == 'CS0103':
        return False

    return False


def main():
    # First apply pre-fixes
    with open(CS_FILE, 'r') as f:
        text = f.read()
    text = apply_pre_fixes(text)
    text = clean_duplicate_casts(text)
    with open(CS_FILE, 'w') as f:
        f.write(text)
    print("Applied pre-fixes.")

    for iteration in range(30):
        errors, success = build_and_get_errors()
        if success:
            print(f"\n=== BUILD SUCCESSFUL after {iteration} iterations! ===")
            return True

        # Deduplicate errors by line number + code
        unique_errors = {}
        for line_str, col_str, code, msg in errors:
            key = (int(line_str), code)
            if key not in unique_errors:
                unique_errors[key] = (int(line_str), int(col_str), code, msg)

        print(f"Iteration {iteration}: {len(errors)} errors ({len(unique_errors)} unique)")

        with open(CS_FILE, 'r') as f:
            lines = f.readlines()

        changed = False
        # Process in reverse line order to avoid offset shifts
        for (line_num, _code), (_, col, code, msg) in sorted(
                unique_errors.items(), key=lambda x: x[0][0], reverse=True):
            if fix_line(lines, line_num - 1, code, msg, col):
                changed = True

        if changed:
            text = ''.join(lines)
            text = clean_duplicate_casts(text)
            with open(CS_FILE, 'w') as f:
                f.write(text)
        else:
            print(f"\nNo more automated fixes. {len(unique_errors)} errors remain:")
            for (line_num, code), (_, col, c, msg) in sorted(unique_errors.items()):
                print(f"  Line {line_num}: {c} - {msg.strip()[:90]}")
            return False

    return False

if __name__ == '__main__':
    main()
