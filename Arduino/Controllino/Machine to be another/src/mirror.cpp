#include <Technorama.h>

bool FirstRunMirror=true;
bool Mirror; //Global
bool MirrorChange; //Global

void mirror (void){
  if (FirstRunMirror){
    Serial.println("init mirror");
    Mirror=false;
    MirrorChange=true;
    FirstRunMirror=false;
  }

  if (MirrorChange) {
    digitalWrite(CONTROLLINO_D2, Mirror);
    MirrorChange=false;
    Serial.println("done");
  }
}
