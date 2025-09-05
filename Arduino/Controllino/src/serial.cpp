#include <mtba.h>
#include <Uduino.h>

Uduino uduino("MachineToBeAnother");
bool firstRunSerial = true;

// Wall control
void serial_wall_on() {
  requestWallMove(true);
}

void serial_wall_off() {
  requestWallMove(false);
}

// Mirror control
void serial_mirror_on() {
  requestMirrorChange(true);
}

void serial_mirror_off() {
  requestMirrorChange(false);
}

// Language selection
void language_de() { Language = 0; }
void language_en() { Language = 1; }
void language_fr() { Language = 2; }
void language_it() { Language = 3; }

// Toggle debug mode
void debug_modus() {
  DEBUG = !DEBUG;
  Serial.println(DEBUG ? "dbg_on" : "dbg_off");
}

// Handler for unknown commands
void unrecognized() {
  Serial.println("cmd_bad");
}

void serial(void) {
  if (firstRunSerial) {
    // Register serial commands (max 8 characters)
    uduino.addCommand("wal_on",   serial_wall_on);
    uduino.addCommand("wal_off",  serial_wall_off);
    uduino.addCommand("mir_on",   serial_mirror_on);
    uduino.addCommand("mir_off",  serial_mirror_off);
    uduino.addCommand("lng_de",   language_de);
    uduino.addCommand("lng_en",   language_en);
    uduino.addCommand("lng_fr",   language_fr);
    uduino.addCommand("lng_it",   language_it);
    uduino.addCommand("debug",    debug_modus);
    uduino.addDefaultHandler(unrecognized);

    // System status check
    if (requestDriveRelease()) Serial.println("sys_rdy");
    else {
      if (!requestDriveReady()) error(2);
      else error(3);
    }

    firstRunSerial = false;
  }

  uduino.update();  // Process serial input
}
