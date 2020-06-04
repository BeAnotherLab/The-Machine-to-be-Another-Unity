#include <Technorama.h>
#include <SerialCommand.h>
#include <Uduino.h>

Uduino uduino("dataSender");
bool FirstRunSerial=true;

void serial_wall_on() {
  Wall=true;
  WallChange=true;
}

void serial_wall_off() {
  Wall=false;
  WallChange=true;
}

void serial_mirror_on() {
  Mirror=true;
  MirrorChange=true;
}

void serial_mirror_off() {
  Mirror=false;
  MirrorChange=true;
}

void serial_curtain_on() {
  Curtain=true;
  CurtainChange=true;
}

void serial_curtain_off() {
  Curtain=false;
  CurtainChange=true;
}

void language_de() {
  Language=0;
}

void language_en() {
  Language=1;
}

void language_fr() {
  Language=2;
}

void language_it() {
  Language=3;
}


void debug_modus() {
  DEBUG=!DEBUG;
  if (DEBUG) Serial.println("dbg_on");
  else Serial.println("dbg_off");
}

// This gets set as the default handler, and gets called when no other command matches.
void unrecognized() {
  Serial.println("cmd_bad");
}

void serial (void) {
  if (FirstRunSerial){

    // Setup callbacks for SerialCommand commands for String. Use just 8 letters!!!
    uduino.addCommand("wal_on",   serial_wall_on);        // close wall
    uduino.addCommand("wal_off",  serial_wall_off);       // open wall
    uduino.addCommand("mir_on",   serial_mirror_on);      // open mirror
    uduino.addCommand("mir_off",  serial_mirror_off);     // close mirror
    uduino.addCommand("cur_on",   serial_curtain_on);     // close curtain
    uduino.addCommand("cur_off",  serial_curtain_off);    // open curtain
    uduino.addCommand("lng_de",   language_de);           // language DE
    uduino.addCommand("lng_en",   language_en);           // language EN
    uduino.addCommand("lng_fr",   language_fr);           // language FR
    uduino.addCommand("lng_it",   language_it);           // language IT
    uduino.addCommand("debug",    debug_modus);           // debug modus 
    uduino.addDefaultHandler(unrecognized);               // Handler for command that isn't matched  (says "What?")

    if (MDReleased) Serial.println("sys_rdy");
    else {
      if (!MDOk) error(2);
      else error(3);
    }
    FirstRunSerial=false;
  }

  uduino.update();          // Read Serial Communication and call funcions
}
