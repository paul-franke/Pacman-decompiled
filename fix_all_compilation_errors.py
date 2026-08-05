import re

with open('PacmanCS/Pacman.cs', 'r') as f:
    code = f.read()

# 1. Fix missing labels jump_2d6c and jump_2c93
if 'jump_2d6c:' not in code:
    code = code.replace('addr = effect->offset;', 'jump_2d6c:\n                addr = effect->offset;')

if 'jump_2c93:' not in code:
    code = code.replace('void* dummy_jump_2c93', 'jump_2c93:')

# 2. Fix array initializers: Action[] func = { ... }; -> Action[] func = new Action[] { ... };
code = re.sub(r'(\b[A-Za-z0-9_<>]+\[\]\s+[A-Za-z0-9_]+\s*=)\s*\{', r'\1 new[] {', code)

# 3. Fix &pointer[idx] -> (pointer + idx)
code = re.sub(r'&\s*([A-Za-z0-9_]+)\[([^\]]+)\]', r'(\1 + \2)', code)

lines = code.splitlines()
out_lines = []

for line in lines:
    l = line

    # Fix operator ! on integer expressions: e.g. !IN0_UP -> (IN0_UP == 0)
    # Fix if (--effect->duration) -> if (--effect->duration != 0)
    if 'if (--effect->duration)' in l:
        l = l.replace('if (--effect->duration)', 'if (--effect->duration != 0)')
    if 'if (!inInterrupt' in l:
        pass # inInterrupt is bool
    elif 'if (!cpuPaused' in l:
        pass # cpuPaused is bool
    elif 'if (!verbose' in l:
        pass # verbose is bool
    else:
        # replace if (!expr) with if (expr == 0) for common integer expressions
        l = re.sub(r'\bif\s*\(!([a-zA-Z0-9_\->\.]+)\)', r'if (\1 == 0)', l)

    # replace if (expr) where expr is bitwise or byte/int variable with if ((expr) != 0)
    m = re.search(r'\bif\s*\(([a-zA-Z0-9_\->\.\s\&\|\^\~]+)\)\s*$', l)
    if m:
        cond = m.group(1).strip()
        if cond not in ['true', 'false', 'paused', 'cpuPaused', 'verbose', 'inInterrupt', 'first', 'redrawEnable', 'drawTargetEnable', 'audioThreadRunning', 'soundThreadRunning']:
            if not cond.startswith('(') and not '!=' in cond and not '==' in cond and not '<' in cond and not '>' in cond:
                l = l[:m.start(1)] + f'({cond}) != 0' + l[m.end(1):]

    out_lines.append(l)

with open('PacmanCS/Pacman.cs', 'w') as f:
    f.write('\n'.join(out_lines))

print("Applied fix_all_compilation_errors.py!")
