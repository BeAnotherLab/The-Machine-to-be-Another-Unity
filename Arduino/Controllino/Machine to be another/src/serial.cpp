#include <Technorama.h>
#include <SerialCommand.h>

SerialCommand sCmd;
bool SystemReleased=false;
bool FirstRunSerial=true;

void serial_wall_on() {
  Serial.println("wal_on");
  if (SystemReleased&&WallReleased) {
    WallStart=true; //Global wall.cpp
    WallDire=true;  //Global wall.cpp
  }
  else Serial.println("System or Motor not released");
}

void serial_wall_off() {
  Serial.println("wal_off");
  if (SystemReleased&&WallReleased) {
    WallStart=true; //Global wall.cpp
    WallDire=false; //Global wall.cpp
  }
  else Serial.println("System or Motor not released");
}

void serial_mirror_on() {
  Serial.println("mir_on");
  if (SystemReleased) {
  Mirror=true; //Global mirror.cpp
  MirrorChange=true; //Global mirror.cpp
  }
  else Serial.println("System not released");
}

void serial_mirror_off() {
  Serial.println("mir_off");
  if (SystemReleased) {
    Mirror=false; //Global mirror.cpp
    MirrorChange=true;  //Global mirror.cpp
  }
  else Serial.println("System not released");
}

void serial_system_on() {
  Serial.println("sys_on");
  if (WallReleased) {
    SystemReleased = true;  //release system
    digitalWrite(CONTROLLINO_D6, HIGH);
    Serial.println("done");
  }
  else Serial.println("Motor not released");

}

void serial_system_off() {
  Serial.println("sys_off");
  SystemReleased = false;  //block system
  digitalWrite(CONTROLLINO_D6, LOW);
  Serial.println("done");
}

void serial_stop() {
  Serial.println("stop");
  SystemReleased=false; //block system
  WallReleased=false;
  Mirror=false;         //Global
  MirrorChange=true;

  digitalWrite(CONTROLLINO_D6, LOW);
}

void serial_system_reset() {
  Reset=true; //Global
}

void debug_modus() {
  DEBUG=!DEBUG;
  if (DEBUG) Serial.println("Debug Modus aktiviert");
  else Serial.println("Debug Modus deaktiviert");
}

// This gets set as the default handler, and gets called when no other command matches.
void unrecognized(const char *command) {
  Serial.println("bad command");
}

void serial (void) {
  if (FirstRunSerial){

    // Setup callbacks for SerialCommand commands for String. Use just 8 letters!!!
    sCmd.addCommand("wal_on",   serial_wall_on);        // Close Wall
    sCmd.addCommand("wal_off",  serial_wall_off);       // Open Wall
    sCmd.addCommand("mir_on",   serial_mirror_on);      // Open Mirror
    sCmd.addCommand("mir_off",  serial_mirror_off);     // Close Mirror
    sCmd.addCommand("sys_on",   serial_system_on);      // System on
    sCmd.addCommand("sys_off",  serial_system_off);     // System off
    sCmd.addCommand("sys_rst",  serial_system_reset);   // System Reset
    sCmd.addCommand("stop",     serial_stop);           // stop the system
    sCmd.addCommand("debug",    debug_modus);           // debug modus
    sCmd.setDefaultHandler(unrecognized);               // Handler for command that isn't matched  (says "What?")

    FirstRunSerial=false;
  }
  sCmd.readSerial();          // Read Serial Communication and call funcions
}
