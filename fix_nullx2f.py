with open('PacmanCS/Pacman.cs', 'r') as f:
    text = f.read()

text = text.replace('nullx2f', '0x2f')

with open('PacmanCS/Pacman.cs', 'w') as f:
    f.write(text)

print("Fixed nullx2f typo!")
