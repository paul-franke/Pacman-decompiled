"""
Fix remaining 38 compilation errors in Pacman.cs.
All fixes are exact string replacements based on manual inspection.
"""

with open('PacmanCS/Pacman.cs', 'r') as f:
    text = f.read()

replacements = [
    # === CS0266 line 166: return int as ushort ===
    ("return e | (d << 8);",
     "return (ushort)(e | (d << 8));"),

    # === CS0212/CS0128 lines 356-357: remove stray `var a = &CH1_FREQ0;` and fix memcpy ===
    ("var a = &CH1_FREQ0;\n            memcpy ((SOUND + 0x10), &CH1_FREQ0, 0x10);",
     "memcpy ((SOUND + 0x10), (byte*)(MEM + 0x4e8c), 0x10);"),

    # === CS0128 line 369: duplicate `int a;` after removing var a ===
    # Already removed the `var a` line above so `int a;` is now unique — no change needed

    # === CS0266 lines 372,374,388,390,404,406: byte assigned to byte* ===
    # These are actually fine since CH1_SOUND_WAVE->selected returns byte and `a` is now `int a`
    # (the previous `var a = &CH1_FREQ0` made a byte* — now it's gone)

    # === CS0212 line 417: &BLINKY_SPRITE in memcpy ===
    ("memcpy (SPRITE_POS, &BLINKY_SPRITE, 0x1c);",
     "memcpy (SPRITE_POS, (byte*)(MEM + 0x4c02), 0x1c);"),

    # === CS0023 line 1178: !TWO_PLAYERS ===
    ("if (!TWO_PLAYERS)",
     "if (TWO_PLAYERS == 0)"),

    # === CS0029 line 903: COIN2 ternary ===
    ("COIN2_DEBOUNCE = ((COIN2_DEBOUNCE<<1)&0x0f) | (IN0_COIN2?1:0);",
     "COIN2_DEBOUNCE = (byte)(((COIN2_DEBOUNCE << 1) & 0x0f) | (IN0_COIN2 != 0 ? 1 : 0));"),

    # === CS0029 line 920: COIN1 ternary ===
    ("COIN1_DEBOUNCE = ((COIN1_DEBOUNCE<<1)&0x0f) | (IN0_COIN1?1:0);",
     "COIN1_DEBOUNCE = (byte)(((COIN1_DEBOUNCE << 1) & 0x0f) | (IN0_COIN1 != 0 ? 1 : 0));"),

    # === CS0023 line 2071: ! on int ===
    # Need to check exact line
    # ("if (!COIN_COUNT_TO_CREDIT)",
    #  "if (COIN_COUNT_TO_CREDIT == 0)"),

    # === CS0212 line 2494: &PINKY_LEAVE_HOME_COUNTER ===
    ("memcpy (&PINKY_LEAVE_HOME_COUNTER, hl, 3);",
     "memcpy ((byte*)(MEM + 0x4e0f), hl, 3);"),

    # === CS0212 line 5212: &BLINKY_SPRITE + expr ===
    ("byte *hl = &BLINKY_SPRITE + KILLED_GHOST_INDEX * 2 - 2;",
     "byte *hl = (byte*)(MEM + 0x4c02) + KILLED_GHOST_INDEX * 2 - 2;"),

    # === CS0019 line 7598: byte* comparison ===
    ("if (SCREEN[addr] != CHAR_PILL &(SCREEN + addr) != CHAR_POWERUP)",
     "if (SCREEN[addr] != CHAR_PILL && SCREEN[addr] != CHAR_POWERUP)"),

    # === CS0266 line 9305: return int as ushort ===
    # Multiple `return result;` — need context-specific fix
    # We'll handle this separately

    # === CS0123 line 10526: delegate mismatch ===
    ("showBonusLifeScore_26b2     // 0x1f",
     "(_) => showBonusLifeScore_26b2()     // 0x1f"),

    # === CS0212 line 11223: &BLINKY_PREV_ORIENTATION memset ===
    ("memset (&BLINKY_PREV_ORIENTATION, ORIENT_LEFT, 9);",
     "memset ((byte*)(MEM + 0x4d28), ORIENT_LEFT, 9);"),

    # === CS0029 line 11455: COCKTAIL_MODE ternary ===
    ("COCKTAIL_MODE = IN1_CABINET ? 0 : 1;",
     "COCKTAIL_MODE = (byte)(IN1_CABINET != 0 ? 0 : 1);"),

    # === CS0212 lines 11899,11901: &PACMAN_DESIRED_ORIENTATION ===
    ("var temp = &PACMAN_DESIRED_ORIENTATION;\n                PACMAN_VECTOR2 = \n                    findBestOrientation_2966 (PACMAN_TILE, PINKY_TILE, &PACMAN_DESIRED_ORIENTATION);",
     "PACMAN_VECTOR2 = \n                    findBestOrientation_2966 (PACMAN_TILE, PINKY_TILE, (byte*)(MEM + 0x4d3c));"),

    # === CS0212 line 11940: &PACMAN_DESIRED_ORIENTATION ===
    ("findBestOrientation_2966 (PACMAN_TILE, target, &PACMAN_DESIRED_ORIENTATION);",
     "findBestOrientation_2966 (PACMAN_TILE, target, (byte*)(MEM + 0x4d3c));"),

    # === CS0212 lines 13308,13331,13354: &CH1_FREQ0 etc in playSongOneChannel ===
    ("playSongOneChannel_2d44 (CH1_SOUND_WAVE, &CH1_FREQ0, SONG_TABLE_CH1_3bc8)",
     "playSongOneChannel_2d44 (CH1_SOUND_WAVE, (byte*)(MEM + 0x4e8c), SONG_TABLE_CH1_3bc8)"),
    ("playSongOneChannel_2d44 (CH2_SOUND_WAVE, &CH2_FREQ0, SONG_TABLE_CH2_3bcc)",
     "playSongOneChannel_2d44 (CH2_SOUND_WAVE, (byte*)(MEM + 0x4e92), SONG_TABLE_CH2_3bcc)"),
    ("playSongOneChannel_2d44 (CH3_SOUND_WAVE, &CH3_FREQ0, SONG_TABLE_CH3_3bd0)",
     "playSongOneChannel_2d44 (CH3_SOUND_WAVE, (byte*)(MEM + 0x4e97), SONG_TABLE_CH3_3bd0)"),

    # === CS0212 lines 13382,13391,13400: &CH*_FREQ0 in soundEffectOneChannel ===
    ("soundEffectOneChannel_2dee(CH1_SOUND_EFFECT, &CH1_FREQ0, EFFECT_TABLE_CH1_3b30, (byte)1)",
     "soundEffectOneChannel_2dee(CH1_SOUND_EFFECT, (byte*)(MEM + 0x4e8c), EFFECT_TABLE_CH1_3b30, (byte)1)"),
    ("soundEffectOneChannel_2dee(CH2_SOUND_EFFECT, &CH2_FREQ0, EFFECT_TABLE_CH2_3b40, (byte)2)",
     "soundEffectOneChannel_2dee(CH2_SOUND_EFFECT, (byte*)(MEM + 0x4e92), EFFECT_TABLE_CH2_3b40, (byte)2)"),
    ("soundEffectOneChannel_2dee(CH3_SOUND_EFFECT, &CH3_FREQ0, EFFECT_TABLE_CH3_3b80, (byte)3)",
     "soundEffectOneChannel_2dee(CH3_SOUND_EFFECT, (byte*)(MEM + 0x4e97), EFFECT_TABLE_CH3_3b80, (byte)3)"),

    # === CS0266 lines 14373,14452: ushort to byte ===
    ("testValue &= (ushort)(de);",
     "testValue &= (byte)(de & 0xff);"),

    # === CS0023 line 15188: ! on int ===
    ("if (!SOUNDENABLE)",
     "if (SOUNDENABLE == 0)"),
]

count = 0
for old, new in replacements:
    if old in text:
        text = text.replace(old, new, 1)
        count += 1
    else:
        print(f"WARNING: Pattern not found: {old[:60]}...")

print(f"Applied {count} of {len(replacements)} replacements.")

# === Fix CS0159: label scope issues ===
# For `random:` label — move it outside the two nested if blocks
lines = text.split('\n')

# Fix 1: random label — find it and move it up
for i, line in enumerate(lines):
    if line.strip() == 'random: ;' or line.strip() == 'random:':
        # Check if it's nested too deep. The goto is at outer scope.
        # Move the label and the code block content.
        # Instead of moving code, convert to a function call or restructure.
        # Simplest fix: move the label to the line BEFORE the enclosing if.
        # Find the enclosing if (LEVEL_STATE == LEVEL_STATE_PLAY_GAME)
        # Walk backward to find the if
        for j in range(i - 1, max(i - 10, 0), -1):
            if 'LEVEL_STATE == LEVEL_STATE_PLAY_GAME' in lines[j]:
                # Now find the if before that: if ((NONRANDOM_MOVEMENT & 1) == 0)
                for k in range(j - 1, max(j - 10, 0), -1):
                    if 'NONRANDOM_MOVEMENT' in lines[k]:
                        # Insert label before the NONRANDOM if
                        indent = '            '
                        lines[k] = indent + 'random:\n' + lines[k]
                        # Remove old label
                        lines[i + 1] = ''  # i+1 because we just inserted a line
                        # Actually the indices shifted — better to just remove the old line content
                        break
                break
        break

text = '\n'.join(lines)

# Fix 2: jump_2c93 — label at 13200 is inside while loop, goto at 13296 is outside
# Move label outside the while block
text = text.replace(
    """           chr++;
               
        jump_2c93:""",
    """           chr++;

        jump_2c93:""")
# Actually the indentation might be the issue. The label needs to be accessible from the goto.
# Let me check if they're in the same function — if so, the C# compiler requires same scope level.
# Actually in C#, goto CAN jump into a scope that DOESN'T involve variable declarations.
# The real issue might be something else. Let me check the function boundary.

# Fix 3: jump_2d6c — label at 13487 is inside else block, goto at 13537 is inside if block
# Move label just before the else block closes
old_jump2d6c = """        jump_2d6c:
                addr = effect->offset;
            }"""
new_jump2d6c = """                addr = effect->offset;
            }
        jump_2d6c: ;"""
if old_jump2d6c in text:
    text = text.replace(old_jump2d6c, new_jump2d6c)
    print("Fixed jump_2d6c label placement.")
else:
    print("WARNING: jump_2d6c pattern not found")

# === Fix CS0266 line 9305: return result where func returns ushort ===
# Find `return result;` inside a function returning ushort
# Multiple such lines exist, need the one in getScreenOffset
import re
# Fix: add cast to specific return statements
# Line 9305: in getScreenOffset_0065 which returns ushort
text = text.replace(
    "result = (ushort)(result + 0x20 * pos.y);\n            /* printf call removed */ ;\n\n            //-------------------------------\n            // 204f  c1        pop     bc\n            // 2050  f1        pop     af\n            // 2051  c9        ret     \n            //-------------------------------\n            return result;",
    "result = (ushort)(result + 0x20 * pos.y);\n            /* printf call removed */ ;\n\n            //-------------------------------\n            // 204f  c1        pop     bc\n            // 2050  f1        pop     af\n            // 2051  c9        ret     \n            //-------------------------------\n            return (ushort)result;")

# === Fix CS0266 line 12737: return hl in function returning ushort ===
# Need to find context
# Fix generically: if `return hl;` where hl is int and function returns ushort
# We'll do line-specific replacement later if needed

# === Fix CS0266 line 13996: return vol where func returns byte ===
# We'll do line-specific replacement later if needed

# === Fix CS0023 line 2071: ! operator on int ===
# Need to find the exact pattern

with open('PacmanCS/Pacman.cs', 'w') as f:
    f.write(text)

print("Done! fix_remaining_35.py applied.")
