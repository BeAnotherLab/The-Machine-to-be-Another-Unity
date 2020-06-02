#include <Technorama.h>

bool Curtain;                    //global
bool CurtainChange;              //global

void curtain (void) {
  if (CurtainChange) {
    digitalWrite(CONTROLLINO_D2, Curtain); //on off Curtain
    CurtainChange = false;
    error(0);
  }
}