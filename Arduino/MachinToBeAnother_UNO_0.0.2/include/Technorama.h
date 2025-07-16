#include <Arduino.h>
#include <Controllino.h>

//INPUTS
#define IN_ENDPOINT_ON      CONTROLLINO_A0
#define IN_ENDPOINT_OFF     CONTROLLINO_A1
#define IN_MD_FAULT         CONTROLLINO_A2
#define IN_MD_READY         CONTROLLINO_A3
//OUTPUTS
#define OUT_WALL_START      CONTROLLINO_D0
#define OUT_WALL_DIRE       CONTROLLINO_D1
#define OUT_CURTAIN         CONTROLLINO_D2
//language INPUTS
//#define IN_LANG_BUT_DE      CONTROLLINO_A6    //08.07.25/che modified for Controllino Mini/Arduino Uno
//#define IN_LANG_BUT_EN      CONTROLLINO_A7    //08.07.25/che modified for Controllino Mini/Arduino Uno
//#define IN_LANG_BUT_FR      CONTROLLINO_A8    //08.07.25/che modified for Controllino Mini/Arduino Uno
//#define IN_LANG_BUT_IT      CONTROLLINO_A9    //08.07.25/che modified for Controllino Mini/Arduino Uno
//language OUTPUTS
//#define OUT_LANG_IND_DE     CONTROLLINO_D6    //08.07.25/che modified for Controllino Mini/Arduino Uno
//#define OUT_LANG_IND_EN     CONTROLLINO_D7    //08.07.25/che modified for Controllino Mini/Arduino Uno
//#define OUT_LANG_IND_FR     CONTROLLINO_D8    //08.07.25/che modified for Controllino Mini/Arduino Uno
//#define OUT_LANG_IND_IT     CONTROLLINO_D9    //08.07.25/che modified for Controllino Mini/Arduino Uno
//relay BLOCK B
#define REL_MIRROR          CONTROLLINO_D4 //CONTROLLINO_R6 //08.07.25/che modified for Controllino Mini/Arduino Uno


extern bool DEBUG;

extern void curtain(void);
extern bool Curtain;
extern bool CurtainChange;

extern void error(byte);

//extern void language(void);   //08.07.25/che modified for Controllino Mini/Arduino Uno
extern byte Language;

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