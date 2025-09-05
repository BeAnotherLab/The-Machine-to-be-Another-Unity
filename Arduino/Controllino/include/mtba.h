#include <Arduino.h>
#include <Controllino.h>  // Hardware definitions for CONTROLLINO PLC

// === INPUTS ===
#define IN_ENDPOINT_ON      CONTROLLINO_A0  // Wall endpoint ON signal
#define IN_ENDPOINT_OFF     CONTROLLINO_A1  // Wall endpoint OFF signal
#define IN_DRIVE_FAULT      CONTROLLINO_A2  // Machinery drive fault
#define IN_DRIVE_READY      CONTROLLINO_A3  // Machinery drive ready

// === OUTPUTS ===
#define OUT_WALL_START      CONTROLLINO_D0  // Start wall movement
#define OUT_WALL_DIR        CONTROLLINO_D1  // Wall movement direction

// === LANGUAGE BUTTON INPUTS ===
#define IN_LANG_BUT_DE      CONTROLLINO_A6  // Select German
#define IN_LANG_BUT_EN      CONTROLLINO_A7  // Select English
#define IN_LANG_BUT_FR      CONTROLLINO_A8  // Select French
#define IN_LANG_BUT_IT      CONTROLLINO_A9  // Select Italian

// === LANGUAGE INDICATOR OUTPUTS ===
#define OUT_LANG_IND_DE     CONTROLLINO_D6  // Indicator for German
#define OUT_LANG_IND_EN     CONTROLLINO_D7  // Indicator for English
#define OUT_LANG_IND_FR     CONTROLLINO_D8  // Indicator for French
#define OUT_LANG_IND_IT     CONTROLLINO_D9  // Indicator for Italian

// === RELAY OUTPUTS (BLOCK B) ===
#define REL_MIRROR          CONTROLLINO_R6  // Mirror control relay

// === GLOBAL VARIABLES ===
extern bool DEBUG;           // Enable/disable debug output

// === FUNCTION DECLARATIONS ===
extern void error(byte);     // Error handling
extern void language(void);  // Language selection logic
extern byte Language;        // Current language

extern void drive(void);                     // Update drive status
extern bool requestDriveRelease(void);       // Drive released?
extern bool requestDriveReady(void);         // Drive ready?
extern bool requestDriveState(void);         // Drive OK?

extern void mirror(void);                    // Mirror control logic
extern void requestMirrorChange(bool state); // Request mirror state change

extern void wall(void);                      // Wall movement logic
extern void requestWallMove(bool direction); // Request wall movement

extern void serial(void);                    // Serial communication handler
