#include <Technorama.h>

bool FuStoerung=false; //Global
bool FuReleased=false;

void fu(){
  if (!digitalRead(CONTROLLINO_A3)&&!FuStoerung){
    Serial.println("FU Störung");
    FuStoerung=true;

    WallReleased=false;
    WallStart=false;
    digitalWrite(CONTROLLINO_D0, LOW); //Motor stoppen
    SystemReleased=false;
    digitalWrite(CONTROLLINO_D6, LOW);
  }

  if (analogRead(CONTROLLINO_A4)>400){ //A4 kann nur als Analog eingang benutzt werden
    FuReleased=true;
  }
  else {
    FuReleased=false;
    Serial.println("FU not released");
  }
}
