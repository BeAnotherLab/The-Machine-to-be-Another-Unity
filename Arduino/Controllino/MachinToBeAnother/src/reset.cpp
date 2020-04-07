#include <Technorama.h>

bool Reset = true;           //global
int statusReset = 0;

void reset() {

  if (Reset) {                //sys_rst received

    switch (statusReset) {

      case 0: //start reset
        ResetWall = true;       //start wall reset
        ResetMirror = true;     //start mirror reset
        ResetCurtain = true;    //start curtain reset
        if (DEBUG) Serial.println("DEBUG Reset is running..");

        statusReset = 1;
        break;

      case 1: //wait for DuringResets
        statusReset = 2;
        break;

      case 2: //wait for resets done
        if (!DuringResetWall && !DuringResetMirror && !DuringResetCurtain) {
          statusReset = 3;
          if (DEBUG) Serial.println("DEBUG Reset is done");
        }
        break;

      case 3: //release the reset
        Reset = false;
        Serial.println("sys_rdy");  //reset executed
        statusReset = 0;
        break;
      
      default:
        break;
    }
  }

}
