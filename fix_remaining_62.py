import re
import subprocess

def fix_all_62():
    with open('PacmanCS/Pacman.cs', 'r') as f:
        text = f.read()

    # 1. Pointers to scores
    text = text.replace('&P1_SCORE', 'P1_SCORE')
    text = text.replace('&P2_SCORE', 'P2_SCORE')
    text = text.replace('&HIGH_SCORE', 'HIGH_SCORE')

    # 2. Pointer comparison to 0 (byte* != 0 -> byte* != null)
    text = re.sub(r'(\b[a-zA-Z0-9_\->\.]+\b)\s*!=\s*0\b', r'\1 != null', text)

    # 3. Address-of memory mapped byte registers
    more_props = {
        'TIMER_SECONDS': '(byte*)(MEM + 0x4c87)',
        'TIMER_MINUTES': '(byte*)(MEM + 0x4c88)',
        'TIMER_HOURS': '(byte*)(MEM + 0x4c89)',
        'P1_LEVEL': '(byte*)(MEM + 0x4e13)',
        'P2_LEVEL': '(byte*)(MEM + 0x4e41)',
        'PLAYER': '(byte*)(MEM + 0x4e09)',
        'MAIN_STATE': '(byte*)(MEM + 0x4e00)',
        'RESET_STATE': '(byte*)(MEM + 0x4e01)',
        'INTRO_STATE': '(byte*)(MEM + 0x4e02)',
        'CREDIT_STATE': '(byte*)(MEM + 0x4e03)',
        'LEVEL_STATE': '(byte*)(MEM + 0x4e04)',
        'SCENE1_STATE': '(byte*)(MEM + 0x4e06)',
        'SCENE2_STATE': '(byte*)(MEM + 0x4e07)',
        'SCENE3_STATE': '(byte*)(MEM + 0x4e08)',
        'P1_FIRST_FRUIT': '(byte*)(MEM + 0x4e0c)',
        'P1_SECOND_FRUIT': '(byte*)(MEM + 0x4e0d)',
        'P2_FIRST_FRUIT': '(byte*)(MEM + 0x4e3a)',
        'P2_SECOND_FRUIT': '(byte*)(MEM + 0x4e3b)',
        'P1_DISPLAY_LIVES': '(byte*)(MEM + 0x4e15)',
        'P2_DISPLAY_LIVES': '(byte*)(MEM + 0x4e43)',
        'P1_REAL_LIVES': '(byte*)(MEM + 0x4e14)',
        'P2_REAL_LIVES': '(byte*)(MEM + 0x4e42)'
    }

    for prop, ptr in more_props.items():
        text = text.replace(f'&{prop}', ptr)

    with open('PacmanCS/Pacman.cs', 'w') as f:
        f.write(text)

    print("Applied fix_remaining_62.py stage 1!")

fix_all_62()
