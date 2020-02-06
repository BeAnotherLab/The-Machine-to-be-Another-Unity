#include <Technorama.h>

int status;
int Timeout;
bool FirstRunWall=true;
bool WallStart;     //Global
bool WallDire;      //Global
bool WallReleased;  //Global

void wall(){
  if (FirstRunWall&&FuReleased){ //Motor initialisieren
      Serial.println("init motor");
      WallStart=true;
      WallDire=false;
      WallReleased=true;
      FirstRunWall=false;
      status=0;
  }

  switch (status) {
    case 0:
      if (WallStart&&WallReleased&&FuReleased){
        digitalWrite(CONTROLLINO_D1, WallDire); //Fahrtrichtung definieren
        status=2;
        Timeout=0;
        WallReleased=false; //Motor sperren bis Endanschlag erreicht

        if(DEBUG)Serial.println("Motor Start");
        if(DEBUG)Serial.print("Fahrtrichtung ");
        if(DEBUG){
          if (WallDire)Serial.println("ON");
          else Serial.println("OFF");
        }
      }
      break;

    case 2:
      digitalWrite(CONTROLLINO_D0, HIGH); //Motor starten
      status=3;

      if(DEBUG)Serial.println("Motorstart ausgeführt ");

      break;

    case 3: //warten bis Endanschlag auslöst
      if (WallDire){
        if (!digitalRead(CONTROLLINO_A0)){ //Anschlag ON
          status=4;

          if(DEBUG)Serial.println("Endanschlag ON erreicht");
        }
      }
      else {
        if (!digitalRead(CONTROLLINO_A1)){ //Anschlag OFF
          status=4;

          if(DEBUG)Serial.println("Endanschlag OFF erreicht");
        }
      }

      if (Timeout==500) status=5; //5s (500 x 10ms) Timeout
      Timeout++;

      break;

    case 4: //Motor zurücksetzen
      digitalWrite(CONTROLLINO_D0, LOW); //Motor stoppen
      WallStart=false;
      WallReleased=true;  //Motor wieder freigeben
      Serial.println("done");

      digitalWrite(CONTROLLINO_D1, !WallDire); //FU für Richtungswechsel vorbereiten

      status=0;

      if(DEBUG)Serial.println("Fahren abgeschlossen");


      break;

    case 5: //TIMEOUT
      Serial.println("TIMEOUT");
      digitalWrite(CONTROLLINO_D0, LOW); //Motor stoppen
      WallStart=false;

      WallReleased=false;
      SystemReleased=false;
      digitalWrite(CONTROLLINO_D6, LOW);

      status=0;
    break;

  }

}
