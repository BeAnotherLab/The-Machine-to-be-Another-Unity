#include <Technorama.h>

bool ResetMirror = false;       //global
bool DuringResetMirror = false; //global
bool Mirror;                    //global
bool MirrorChange;              //global

void mirror (void) {
  if (ResetMirror) {
    if (DEBUG) Serial.println("DEBUG mir_rst");
    Mirror = false;
    MirrorChange = true;
    ResetMirror = false;
    DuringResetMirror = true;
  }

  if (MirrorChange) {
    digitalWrite(CONTROLLINO_D2, Mirror); //on off mirror
    MirrorChange = false;
    if (!Reset) error(0);
    else DuringResetMirror = false;
  }
}
