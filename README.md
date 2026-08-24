Pacman in C (Windows Port)
=========================

This is a C translation of the original Pac-Man arcade ROM code, featuring a fully functional native Windows port. The original code remains as comments in the source files, and translated assembly is boxed off.

The original project is created by **mburkley** and is available at: https://github.com/mburkley/pacman-c.
Special thanks to **mburkley** for his great preliminary work, on which this Windows port is heavily based!

This project does not include the original ROM files, although they are required. You must obtain the ROM files yourself. Once you have them, building and running the game is automated.

Setup & Build Pipeline
----------------------
No manual installation steps required. 

Build the executable by running the following scripts: 
1. **`clean.bat`** (Cleanup Step)
   * Deletes all compiled object files, executables, generated ROM headers, downloaded libraries, the local compiler, and the game subdirectory, restoring the repository to a clean state.

2. **`install.bat`** (Installation & Setup Step)
   * Automatically installs and sets up a local, portable **Tiny C Compiler (TCC)** (only ~2MB total).
   * Installs copies of the required FreeGLUT libraries and standard OpenGL headers.
   * Runs an automatic patching utility (`patch_headers.ps1`) to wrap compiler-specific directives.
   * Creates a `game/` subdirectory and copies the runtime `freeglut.dll` there. Needed for proper behavior.
   * Parses the raw ROM files inside the `rom/` directory and converts them into C arrays inside `include/roms/`.

3. **`build.bat`** (Compilation Step)
   * builds the executable `game\pacman.exe`.
  

Required ROM Files
------------------
You must obtain the original Pac-Man ROM files and place them in the **`rom/`** directory. The following files must be present:
* `pacman.5e` - Ghost graphics
* `pacman.5f` - Pacman graphics
* `pacman.6e` - CPU code 
* `pacman.6f` - CPU code 
* `pacman.6h` - CPU code 
* `pacman.6j` - CPU code 
* `82s123.7f` - Color palette PROM
* `82s126.4a` - Color lookup table PROM
* `82s126.1m` - Sound waveform 1
* `82s126.3m` - Sound waveform 2


Running & Logging
-----------------
* **Running the Game**:
  It is best to run `pacman.exe` from the command line, though it can also be run directly from Windows Explorer (double-clicking `game\pacman.exe`).

* **Frame Rate Control (`-f` flag)**:
  By default, the game runs at 60 FPS. You can override the frames per second by passing the `-f` flag followed by an integer from 1 to 60.
  Example (run at 30 FPS):
  ```cmd
  game\pacman.exe -f 30
  ```

* **Logging Control (`-v` flag)**:
  By default, the game runs in silent mode. If you want to enable debug logs and error messages, run the game with the `-v` flag:
  ```cmd
  game\pacman.exe -v
  ```

Keys (Windows)
--------------
* **Arrow Keys** (or NumPad) = Move Up, Down, Left, Right
* **5** = Insert Coin
* **1** = 1 Player Start
* **2** = 2 Player Start
* **P** = Pause CPU
* **D** = Toggle Target Vector overlays (visualizing target tiles for Blinky, Pinky, Inky, Clyde, and Pac-Man)


Goals
-----
Using a fully functional real Arcade game to help my learning process on the strengths and weaknesses of current AI-tooling. I use AI in this process to help me sift through the assembly and C-code. Also some projects are added in the process.

[Original blog containing more details][1]

[1]: https://pacmanc.blogspot.com/
