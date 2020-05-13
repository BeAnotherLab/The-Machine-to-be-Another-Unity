#include <Arduino.h>
#include <Controllino.h>

extern bool DEBUG;

extern void blink(void);

extern void curtain(void);
extern bool Curtain;
extern bool CurtainChange;

extern void error(byte);

extern void machinerydrive(void);
extern bool MDReleased;
extern bool MDReady;
extern bool MDOk;

extern void mirror(void);
extern bool Mirror;
extern bool MirrorChange;

extern void serial(void);

extern void wall(void);
extern bool Wall;
extern bool WallChange;

extern void language(void);
extern byte LngState;
extern bool LngChange;