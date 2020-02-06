#include <Arduino.h>
#include <Controllino.h>

extern bool DEBUG;

extern void serial(void);
extern bool SystemReleased;

extern void wall(void);
extern bool FirstRunWall;
extern bool WallStart;
extern bool WallDire;
extern bool WallReleased;

extern void mirror(void);
extern bool FirstRunMirror;
extern bool Mirror;
extern bool MirrorChange;

extern void blink(void);

extern void fu(void);
extern bool FuStoerung;
extern bool FuReleased;

extern void reset(void);
extern bool Reset;

extern void notstop(void);
