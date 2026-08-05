import re

with open('PacmanCS/Pacman.cs', 'r') as f:
    text = f.read()

# 1. Fix Action<int> delegate table assignments for 0-param functions
funcs_no_param = [
    'resetGameState_2698', 'homeOrRandomBlinky_283b', 'homeOrRandomPinky_2865',
    'homeOrRandomInky_288f', 'homeOrRandomClyde_28b9', 'clearGhostState_26a2',
    'clearPillArrays_24c9', 'clearPillsScreen_2a35', 'pacmanOrientationDemo_28e3',
    'clearScores_2ae0', 'displayCredits_2ba1', 'resetPositions_2675',
    'showBonusLifeScore_26b2'
]

for fn in funcs_no_param:
    text = text.replace(f' {fn},', f' (_) => {fn}(),')
    text = text.replace(f' {fn}\n', f' (_) => {fn}()\n')

# 2. Fix sound struct field array pointers: &sound->v1FreqCount[0] -> sound->v1FreqCount
text = re.sub(r'&\s*([a-zA-Z0-9_\->\.]+)\s*\[0\]', r'\1', text)

with open('PacmanCS/Pacman.cs', 'w') as f:
    f.write(text)

print("Updated delegate matches and struct pointers!")
