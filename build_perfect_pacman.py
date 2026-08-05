import re
import subprocess

def generate_pacman_cs():
    with open('pacman.c', 'r') as f:
        text = f.read()

    # 1. Replace '#if 0' with '#if false'
    text = re.sub(r'#if\s+0\b', '#if false', text)

    # 2. Remove or comment out all printf calls cleanly (even multi-line)
    text = re.sub(r'printf\s*\([^;]*\);', '/* printf call removed */ ;', text, flags=re.DOTALL)

    # 3. Fix usleep
    text = re.sub(r'\busleep\b', 'Cpu.usleep', text)

    # 4. Fix '(void)' parameter lists in function definitions/calls
    text = re.sub(r'\(\s*void\s*\)', '()', text)

    # 5. Fix __func__
    text = text.replace('__func__', '"func"')

    # 6. Replace C types
    text = re.sub(r'\buint8_t\b', 'byte', text)
    text = re.sub(r'\buint16_t\b', 'ushort', text)
    text = re.sub(r'\buint32_t\b', 'uint', text)
    text = re.sub(r'\bint8_t\b', 'sbyte', text)
    text = re.sub(r'\bint16_t\b', 'short', text)
    text = re.sub(r'\bint32_t\b', 'int', text)
    text = re.sub(r'\bNULL\b', 'null', text)

    # 7. Function pointer array declarations inside functions -> use 'new Type[] { ... }'
    text = text.replace('void (*func[])() =', 'Action[] func = new Action[]')
    text = text.replace('void (*func[])(ushort param) =', 'Action<ushort>[] func = new Action<ushort>[]')
    text = text.replace('void (*func[])(int param) =', 'Action<int>[] func = new Action<int>[]')
    text = text.replace('void (*func[])(SOUND_EFFECT*, byte *) =', 'SoundEffectProc1[] func = new SoundEffectProc1[]')
    text = text.replace('byte (*func[])(SOUND_EFFECT*) =', 'SoundEffectProc2[] func = new SoundEffectProc2[]')
    text = text.replace('void (*func[])(SOUND_EFFECT *, byte *) =', 'SoundEffectProc1[] func = new SoundEffectProc1[]')
    text = text.replace('byte (*func[])(SOUND_EFFECT *) =', 'SoundEffectProc2[] func = new SoundEffectProc2[]')

    # Convert generic array initializers: Action[] func = { -> Action[] func = new Action[] {
    text = re.sub(r'(\b[A-Za-z0-9_<>]+\[\]\s+[A-Za-z0-9_]+\s*=)\s*\{', r'\1 new[] {', text)

    # 8. Convert &pointer[index] -> (pointer + index)
    text = re.sub(r'&\s*([A-Za-z0-9_]+)\[([^\]]+)\]', r'(\1 + \2)', text)

    # 9. Fix array parameter syntax in function signatures like 'byte arr[]' -> 'byte* arr'
    text = re.sub(r'(\b(?:byte|ushort|uint|sbyte|short|int|XYPOS|TASK|SOUND_EFFECT|SOUNDREGS))\s+([a-zA-Z0-9_]+)\[\]', r'\1* \2', text)

    # 10. Comment out includes
    text = re.sub(r'^(#include\s+.*)$', r'// \1', text, flags=re.MULTILINE)

    # 11. Comment out tableCall_0020 definition from pacman.c
    text = text.replace('void tableCall_0020 (void (*func[])(), byte a)', '/* void tableCall_0020 (Action[] func, byte a) */ public static void tableCall_0020_c (Action[] func, byte a)')

    # 12. Handle scoreTable_2b17
    text = re.sub(r'ushort\*\s+scoreTable_2b17\s*=\s*\{[^}]*\};', 'public static ushort* scoreTable_2b17 => (ushort*)(ROM + 0x2b17);', text, flags=re.DOTALL)

    # 13. Add 'public static' to function declarations
    func_types = r'(?:void|byte|ushort|uint|sbyte|short|int|bool|XYPOS|TASK|SOUND_EFFECT|pixel|byte\*|ushort\*|uint\*|XYPOS\*)'
    text = re.sub(r'^(?:static\s+)?(' + func_types + r'\s+\*?[a-zA-Z0-9_]+\s*\([^)]*\)\s*\{)', r'public static \1', text, flags=re.MULTILINE)

    lines = text.splitlines()

    # 14. Comment out prototype declarations (lines ending in semicolon with no body)
    def comment_prototypes(line):
        s = line.strip()
        if re.match(r'^(?:public static\s+)?(?:void|byte|ushort|uint|sbyte|short|int|bool|pixel|XYPOS)\s+\*?[a-zA-Z0-9_]+\s*\([^)]*\)\s*;$', s):
            return '        // ' + line
        return line

    lines = [comment_prototypes(l) for l in lines]

    out = []
    out.append('using System;')
    out.append('using System.Runtime.InteropServices;')
    out.append('using static PacmanCS.MemMap;')
    out.append('using static PacmanCS.Consts;')
    out.append('using static PacmanCS.Data;')
    out.append('using static PacmanCS.Cpu;')
    out.append('using static PacmanCS.Video;')
    out.append('using static PacmanCS.Sound;')
    out.append('')
    out.append('namespace PacmanCS')
    out.append('{')
    out.append('    public static unsafe class Pacman')
    out.append('    {')
    out.append('        // Helpers for C stdlib functions')
    out.append('        public static void memset(void* ptr, int val, int count)')
    out.append('        {')
    out.append('            new Span<byte>(ptr, count).Fill((byte)val);')
    out.append('        }')
    out.append('        public static void memset(void* ptr, int val, uint count)')
    out.append('        {')
    out.append('            new Span<byte>(ptr, (int)count).Fill((byte)val);')
    out.append('        }')
    out.append('        public static void memcpy(void* dest, void* src, int count)')
    out.append('        {')
    out.append('            Buffer.MemoryCopy(src, dest, count, count);')
    out.append('        }')
    out.append('        public static void memcpy(void* dest, void* src, uint count)')
    out.append('        {')
    out.append('            Buffer.MemoryCopy(src, dest, count, count);')
    out.append('        }')
    out.append('        public static int abs(int v) => Math.Abs(v);')
    out.append('        public static bool toBool(int val) => val != 0;')
    out.append('        public static bool toBool(byte val) => val != 0;')
    out.append('        public static bool toBool(ushort val) => val != 0;')
    out.append('        public static bool toBool(void* ptr) => ptr != null;')
    out.append('')
    out.append('        public delegate void SoundEffectProc1(SOUND_EFFECT* effect, byte* frequency);')
    out.append('        public delegate byte SoundEffectProc2(SOUND_EFFECT* effect);')
    out.append('')
    out.append('        public static void tableCall_0020(Action[] func, byte a) => func[a]();')
    out.append('        public static void tableCall_0020(Action<ushort>[] func, byte a, ushort param) => func[a](param);')
    out.append('        public static void tableCall_0020(Action<int>[] func, byte a, int param) => func[a](param);')
    out.append('        public static void tableCall_0020(SoundEffectProc1[] func, byte a, SOUND_EFFECT* effect, byte* frequency) => func[a](effect, frequency);')
    out.append('        public static byte tableCall_0020(SoundEffectProc2[] func, byte a, SOUND_EFFECT* effect) => func[a](effect);')
    out.append('')

    for l in lines:
        out.append('        ' + l)

    out.append('    }')
    out.append('}')

    with open('PacmanCS/Pacman.cs', 'w') as f:
        f.write('\n'.join(out))

generate_pacman_cs()
print("Pacman.cs generated cleanly.")
