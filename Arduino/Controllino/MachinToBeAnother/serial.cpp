#include <Technorama.h>
#include <SerialCommand.h>

SerialCommand sCmd;
bool FirstRunSerial=true;

void serial_wall_on() {
  Serial.println("wal_on");
  WallStart=true;
  WallDire=true;
}

void serial_wall_off() {
  Serial.println("wal_off");
  WallStart=true;
  WallDire=false;
}

void serial_mirror_on() {
  Serial.println("mir_on");
  Mirror=true;
  MirrorChange=true;
}

void serial_mirror_off() {
  Serial.println("mir_off");
  Mirror=false;
  MirrorChange=true;
}

void serial_system_reset() {
  Serial.println("sys_rst");
  Reset=true;
}

void debug_modus() {
  DEBUG=!DEBUG;
  if (DEBUG) Serial.println("dbg_on");
  else Serial.println("dbg_off");
}

// This gets set as the default handler, and gets called when no other command matches.
void unrecognized(const char *command) {
  Serial.println("cmd_bad");
}

void serial (void) {
  if (FirstRunSerial){

    // Setup callbacks for SerialCommand commands for String. Use just 8 letters!!!
    sCmd.addCommand("wal_on",   serial_wall_on);        // Close Wall
    sCmd.addCommand("wal_off",  serial_wall_off);       // Open Wall
    sCmd.addCommand("mir_on",   serial_mirror_on);      // Open Mirror
    sCmd.addCommand("mir_off",  serial_mirror_off);     // Close Mirror
    sCmd.addCommand("sys_rst",  serial_system_reset);   // System Reset
    sCmd.addCommand("debug",    debug_modus);           // debug modus
    sCmd.setDefaultHandler(unrecognized);               // Handler for command that isn't matched  (says "What?")

    FirstRunSerial=false;
  }
  sCmd.readSerial();          // Read Serial Communication and call funcions
}
