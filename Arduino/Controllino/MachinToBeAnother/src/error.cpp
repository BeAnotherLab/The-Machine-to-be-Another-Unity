#include <Technorama.h>

byte Error=0; //global

void error(byte ErrorNr){
  Error = ErrorNr;

  switch (ErrorNr) {

    case 0:
      Serial.println("sys_rdy");  //command executed
    break;

    case 1:
      Serial.println("TIMEOUT");  //endpoints of the wall not reached
    break;

    case 2:
      Serial.println("MD_FAULT"); //fault on machinery drive
    break;

    case 3:
      Serial.println("MD_BLOCK"); //machinery drive not ready
    break;
  }
}
