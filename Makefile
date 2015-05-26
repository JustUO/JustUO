#GNU Makefile for JustUO on Linux systems
#  sith@zuluhotel.ca

CC=mcs
LDFLAGS=-r:System.Drawing,OpenUO.Core.dll,OpenUO.Ultima.dll,OpenUO.Ultima.Windows.Forms.dll,SevenZipSharp.dll
OPTFLAGS=-optimize+
CFLAGS=-sdk:4 -d:MONO,NEWTIMERS,NEWPARENT -unsafe -nowarn:219,414 -out:Server.exe
RECURSE=-recurse:'Server/*.cs'
DFLAGS=-debug

all: release

help:
	@echo "Targets:"
	@echo "configure:  sets up client folder directory 'muls/'"
	@echo "release:  compiles Server binary with optimization flags"
	@echo "debug:  compiles Server binary with debugging symbols"
	@echo "clean:  removes Server binary"

configure:
	mkdir muls
	@echo "Folder muls/ created, you should put a copy of the client files here."

release:
	$(CC) $(CFLAGS) $(LDFLAGS) $(OPTFLAGS) $(RECURSE)

debug:
	$(CC) $(CFLAGS) $(LDFLAGS) $(DFLAGS) $(RECURSE)

clean:
	rm Server.exe
