#include <Technorama.h>

bool FirstRunMirror=true; //global
bool Mirror;              //global
bool MirrorChange;        //global

void mirror (void){
  if (FirstRunMirror){
    Serial.println("mir_off");
    Mirror=false;
    MirrorChange=true;
    FirstRunMirror=false;
  }

  if (MirrorChange) {
    digitalWrite(CONTROLLINO_D2, Mirror); //on off mirror
    MirrorChange=false;
    error(0);
  }
}
