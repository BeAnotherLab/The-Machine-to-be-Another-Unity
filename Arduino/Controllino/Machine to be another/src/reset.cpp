#include <Technorama.h>

bool Reset=false;

void reset(){
  if (digitalRead(CONTROLLINO_A2)||Reset){ //externer Resetschalter
    Reset=false;
    Serial.println("reset");
    FirstRunWall=true;    //Wall neu initialisieren
    FirstRunMirror=true;  //Mirror neu initialisieren
    FuStoerung=false;     //FU zurücksetzen
  }
}
