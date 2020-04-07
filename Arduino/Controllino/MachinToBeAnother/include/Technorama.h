#include <Arduino.h>
#include <Controllino.h>

extern bool DEBUG;

extern void blink(void);

extern void curtain(void);
extern bool ResetCurtain;
extern bool DuringResetCurtain;
extern bool Curtain;
extern bool CurtainChange;

extern void error(byte);
extern byte Error;

extern void machinerydrive(void);
extern bool MDReleased;

extern void mirror(void);
extern bool ResetMirror;
extern bool DuringResetMirror;
extern bool Mirror;
extern bool MirrorChange;

extern void reset(void);
extern bool Reset;

extern void serial(void);

extern void wall(void);
extern bool ResetWall;
extern bool DuringResetWall;
extern bool Wall;
extern bool WallChange;

extern void language(void);
extern bool LngChange;
extern byte LngState;