#include <Technorama.h>

int status = 0;
int Timeout;
bool Wall;               //global
bool WallChange;         //global

void wall() {
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
          if (!MDOk) status = 6;
          else status = 7;
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
          if (!MDOk) status = 6;
          else status = 7;
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
        if (Timeout>=500) status=5; //5s timeout (500 x 10ms)
        Timeout++;
      }

      else {
          if (!MDOk) status = 6;
          else status = 7;
      }
    break;


    case 4: //stop wall
      digitalWrite(CONTROLLINO_D0, LOW); //stop motor
      if(DEBUG) Serial.println("DEBUG wall stopped");
      WallChange=false;
      error(0);
      digitalWrite(CONTROLLINO_D1, !Wall); //preset other direction
      status=0;
    break;


    case 5: //TIMEOUT
      digitalWrite(CONTROLLINO_D0, LOW); //stop motor
      error(1);
      WallChange=false;
      status=0;
    break;


    case 6: //MD_FAULT
      digitalWrite(CONTROLLINO_D0, LOW); //stop motor
      error(2);
      WallChange=false;
      status=0;
    break;


    case 7: //MD_BLOCK
      digitalWrite(CONTROLLINO_D0, LOW); //stop motor
      error(3);
      WallChange=false;
      status=0;
    break;

  }

}
