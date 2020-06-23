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
          digitalWrite(OUT_WALL_DIRE, Wall); //set direction
          status=2;
          Timeout=0;
        }
        else {
          if (!MDOk) status = 6;
          else status = 7;
        }
      }
    break;


    case 2: //start motor
      if (MDReleased) {
        digitalWrite(OUT_WALL_START, HIGH);
        status=3;
      }
      else {
          if (!MDOk) status = 6;
          else status = 7;
      }
    break;


    case 3: //waiting for endpoint detection
      if (MDReleased) {
        if (Wall) {
          if (!digitalRead(IN_ENDPOINT_ON)) { //endpoint ON
            status=4;
          }
        }
        else {
          if (!digitalRead(IN_ENDPOINT_OFF)) { //endpoint OFF
            status=4;
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
      digitalWrite(OUT_WALL_START, LOW); //stop motor
      WallChange=false;
      error(0);
      digitalWrite(OUT_WALL_DIRE, !Wall); //preset other direction
      status=0;
    break;


    case 5: //TIMEOUT
      digitalWrite(OUT_WALL_START, LOW); //stop motor
      error(1);
      WallChange=false;
      status=0;
    break;


    case 6: //MD_FAULT
      digitalWrite(OUT_WALL_START, LOW); //stop motor
      error(2);
      WallChange=false;
      status=0;
    break;


    case 7: //MD_BLOCK
      digitalWrite(OUT_WALL_START, LOW); //stop motor
      error(3);
      WallChange=false;
      status=0;
    break;

  }

}
