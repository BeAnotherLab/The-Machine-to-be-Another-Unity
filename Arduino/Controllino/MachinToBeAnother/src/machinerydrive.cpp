#include <Technorama.h>

bool MDReleased=false;                    //global
bool MDReady=true;
bool MDOk=false;
bool SendErrorOnce1=true;
bool SendErrorOnce2=true;

void machinerydrive(){
  if (digitalRead(CONTROLLINO_A3)) {      //MD ready
    MDReady=true;
    SendErrorOnce1=true;
  }
  else {
    MDReady=false;
    if (SendErrorOnce1) {
      SendErrorOnce1=false;
      error(3);                         //MD blocked
    }
  }

  if (digitalRead(CONTROLLINO_A2)) {    //MD OK
    MDOk=true;
    SendErrorOnce2=true;
  }
  else {
    MDOk=false;
    if (SendErrorOnce2) {
      SendErrorOnce2=false;
      error(2);                         //MD fault
    }
    digitalWrite(CONTROLLINO_D0, LOW);  //stop motor
  }

  MDReleased=MDReady&&MDOk;
  digitalWrite(CONTROLLINO_D6, MDReleased);
}
