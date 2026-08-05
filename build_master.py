import subprocess

print("Step 1: Running build_perfect_pacman.py...")
subprocess.run(['python', 'build_perfect_pacman.py'], check=True)

print("Step 2: Running final_clean_pass.py...")
subprocess.run(['python', 'final_clean_pass.py'], check=True)

print("Step 3: Running fix_exact_11_perfect.py...")
subprocess.run(['python', 'fix_exact_11_perfect.py'], check=True)

print("Step 4: Running fix_cs1503.py...")
subprocess.run(['python', 'fix_cs1503.py'], check=True)

print("Step 5: Running fix_final_4_exact.py...")
subprocess.run(['python', 'fix_final_4_exact.py'], check=True)

print("Step 6: Running fix_final_4_lines.py...")
subprocess.run(['python', 'fix_final_4_lines.py'], check=True)

print("Step 7: Running dotnet build...")
res = subprocess.run(['dotnet', 'build', 'PacmanCS/PacmanCS.csproj'], capture_output=True, text=True)
out = res.stdout + res.stderr
print(out[-1500:])

if 'Build FAILED' not in out:
    print("\n=======================================================")
    print("=== SUCCESS! 0 BUILD ERRORS! PacmanCS BUILT CLEANLY! ===")
    print("=======================================================\n")
else:
    print("Build Failed. Check errors above.")
