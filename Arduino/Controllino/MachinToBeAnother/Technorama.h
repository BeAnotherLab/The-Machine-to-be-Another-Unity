#include <Arduino.h>
#include <Controllino.h>

extern bool DEBUG;

extern void blink(void);

extern void error(byte);
extern byte Error;

extern void machinerydrive(void);
extern bool MDReleased;

extern void mirror(void);
extern bool FirstRunMirror;
extern bool Mirror;
extern bool MirrorChange;

extern void reset(void);
extern bool Reset;

extern void serial(void);

extern void wall(void);
extern bool FirstRunWall;
extern bool WallStart;
extern bool WallDire;
