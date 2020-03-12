#include <Technorama.h>

byte Error=0; //global

void error(byte ErrorNr){
  Error = ErrorNr;

  switch (ErrorNr) {

    case 0:
      Serial.println("sys_rdy");
    break;

    case 1:
      Serial.println("TIMEOUT");
    break;

    case 2:
      Serial.println("MD_FAULT");
    break;

    case 3:
      Serial.println("MD_BLOCK");
    break;
  }
}
