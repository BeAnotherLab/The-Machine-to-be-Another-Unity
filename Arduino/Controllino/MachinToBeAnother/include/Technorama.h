#include <Arduino.h>
#include <Controllino.h>
#include <Adafruit_MCP23008.h>

extern Adafruit_MCP23008 mcp;

extern bool DEBUG;

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
extern byte Language;