#include <mtba.h>

void error(byte Error) {
  switch (Error) {
    case 0:
      Serial.println("cmd_ok");     // command executed
      break;

    case 1:
      Serial.println("TIMEOUT");    // wall endpoints not reached
      break;

    case 2:
      Serial.println("MD_FAULT");   // machinery drive fault
      break;

    case 3:
      Serial.println("MD_BLOCK");   // machinery drive not ready
      break;

    case 4:
      Serial.println("ST_BLOCK");   // state machine blocked
      break;
  }
}
