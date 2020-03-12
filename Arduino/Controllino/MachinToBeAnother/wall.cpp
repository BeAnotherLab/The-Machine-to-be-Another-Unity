#include <Technorama.h>

int status=0;
int Timeout;
bool FirstRunWall=true;   //global
bool WallStart;           //global
bool WallDire;            //global
bool WallReleased;        //global

void wall(){
  if (FirstRunWall&&MDReleased) {  //motor initialization
      Serial.println("wal_off");
      WallStart=true;
      WallDire=false;
      FirstRunWall=false;
      status=0;
  }

  switch (status) {

    case 0: //Start
      if (WallStart) {  //receive wall
        if (MDReleased) {
          digitalWrite(CONTROLLINO_D1, WallDire); //set direction
          status=2;
          Timeout=0;

          if(DEBUG) {
            Serial.println("wall start");
            Serial.print("direction ");
            if (WallDire) Serial.println("ON");
            else Serial.println("OFF");
          }
        }
        else {
          status=6;
          if(DEBUG) Serial.println("abborted");
        }
      }
    break;


    case 2: //start motor
      if (MDReleased) {
        digitalWrite(CONTROLLINO_D0, HIGH);
        status=3;

        if(DEBUG) Serial.println("wall started ");
      }

      else {
        status=6;
        if(DEBUG) Serial.println("abborted");
      }
    break;


    case 3: //waiting for endpoint detection
      if (MDReleased) {
        if (WallDire) {
          if (!digitalRead(CONTROLLINO_A0)) { //endpoint ON
            status=4;
            if(DEBUG) Serial.println("endpoint ON done");
          }
        }
        else {
          if (!digitalRead(CONTROLLINO_A1)) { //endpoint OFF
            status=4;
            if(DEBUG) Serial.println("endpoint OFF done");
          }
        }
        if (Timeout==500) status=5; //5s timeout (500 x 10ms)
        Timeout++;
      }

      else {
        status=6;
        if(DEBUG) Serial.println("abborted");
      }
    break;


    case 4: //stop wall
      digitalWrite(CONTROLLINO_D0, LOW); //stop motor
      WallStart=false;
      error(0);
      digitalWrite(CONTROLLINO_D1, !WallDire); //preset other direction
      status=0;
    break;


    case 5: //TIMEOUT
      error(1);
      digitalWrite(CONTROLLINO_D0, LOW); //stop motor
      WallStart=false;
      status=0;
    break;


    case 6: //MD problems
      error(Error);
      digitalWrite(CONTROLLINO_D0, LOW); //stop motor
      WallStart=false;
      status=0;
    break;

  }

}
