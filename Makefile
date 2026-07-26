all: pacman

OBJECTS=\
pacman.o \
cpu.o \
video.o \
sound.o \
kbd.o

LIBS=\
-l m\
-l glut\
-l GL\
-lpulse-simple\
-lpulse

ROMS=\
pacman.5e \
pacman.5f \
pacman.6e \
pacman.6f \
pacman.6h \
pacman.6j \
82s123.7f \
82s126.4a \
82s126.1m \
82s126.3m

FILES=$(patsubst %,rom/%,$(ROMS))
HDRS=$(patsubst %,include/roms/%.h,$(ROMS))

include/roms/%.h: rom/%
	@mkdir -p include/roms
	xxd -i $< > $@

CFLAGS=-Wall -ggdb3 -Wincompatible-pointer-types -Iinclude -Iinclude/roms

pacman: $(OBJECTS) harness.o
	@echo "\t[LD] $@..."
	@$(CC) -ggdb3 -Wall -o $@ $^ $(LIBS)

test: $(OBJECTS) test.o
	@echo "\t[LD] $@..."
	@$(CC) -ggdb3 -Wall -o $@ $^ $(LIBS)

%.o: %.c $(HDRS)
	@echo "\t[CC] $<..."
	@$(CC) -c $(CFLAGS) $< -o $@

clean:
	@echo "Cleaning build artifacts, ROM headers, and libraries..."
	rm -f $(OBJECTS) harness.o test.o pacman test
	rm -rf include/roms
	rm -f freeglut.dll libs/freeglut.dll libs/freeglut.lib libs/glut32.lib
	rm -rf include/GL
	rm -rf compiler
