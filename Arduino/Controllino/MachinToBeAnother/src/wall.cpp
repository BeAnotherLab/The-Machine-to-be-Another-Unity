#include <Technorama.h>

int status = 0;
int Timeout;
bool ResetWall = false;       //global
bool DuringResetWall = false; //global
bool Wall;               //global
bool WallChange;                //global

void wall(){
  if (ResetWall && MDReleased) {  //motor initialization
      if (DEBUG) Serial.println("DEBUG wal_rst");
      WallChange = true;
      Wall = false;
      ResetWall = false;
      DuringResetWall = true;
  }

  switch (status) {

    case 0: //Start
      if (WallChange) {  //receive wall
        if (MDReleased) {
          digitalWrite(CONTROLLINO_D1, Wall); //set direction
          status=2;
          Timeout=0;

          if(DEBUG) {
            Serial.println("DEBUG wall start");
            Serial.print("DEBUG direction ");
            if (Wall) Serial.println("ON");
            else Serial.println("OFF");
          }
        }
        else {
          status=6;
          if(DEBUG) Serial.println("DEBUG abborted");
        }
      }
    break;


    case 2: //start motor
      if (MDReleased) {
        digitalWrite(CONTROLLINO_D0, HIGH);
        status=3;

        if(DEBUG) Serial.println("DEBUG wall started ");
      }

      else {
        status=6;
        if(DEBUG) Serial.println("DEBUG abborted");
      }
    break;


    case 3: //waiting for endpoint detection
      if (MDReleased) {
        if (Wall) {
          if (!digitalRead(CONTROLLINO_A0)) { //endpoint ON
            status=4;
            if(DEBUG) Serial.println("DEBUG endpoint ON done");
          }
        }
        else {
          if (!digitalRead(CONTROLLINO_A1)) { //endpoint OFF
            status=4;
            if(DEBUG) Serial.println("DEBUG endpoint OFF done");
          }
        }
        if (Timeout==500) status=5; //5s timeout (500 x 10ms)
        Timeout++;
      }

      else {
        status=6;
        if(DEBUG) Serial.println("DEBUG abborted");
      }
    break;


    case 4: //stop wall
      digitalWrite(CONTROLLINO_D0, LOW); //stop motor
      WallChange=false;
      if (!Reset) error(0);
      else DuringResetWall = false;
      digitalWrite(CONTROLLINO_D1, !Wall); //preset other direction
      status=0;
    break;


    case 5: //TIMEOUT
      error(1);
      digitalWrite(CONTROLLINO_D0, LOW); //stop motor
      WallChange=false;
      status=0;
    break;


    case 6: //MD problems
      error(Error);
      digitalWrite(CONTROLLINO_D0, LOW); //stop motor
      WallChange=false;
      status=0;
    break;

  }

}
