#include <Technorama.h>

bool ResetCurtain = false;       //global
bool DuringResetCurtain = false; //global
bool Curtain;                    //global
bool CurtainChange;              //global

void curtain (void) {
  if (ResetCurtain) {
    if (DEBUG) Serial.println("DEBUG cur_rst");
    Curtain = false;
    CurtainChange = true;
    ResetCurtain = false;
    DuringResetCurtain = true;
  }

  if (CurtainChange) {
    digitalWrite(CONTROLLINO_D4, Curtain); //on off Curtain
    CurtainChange = false;
    if (!Reset) error(0);
    else DuringResetCurtain = false;
  }
}