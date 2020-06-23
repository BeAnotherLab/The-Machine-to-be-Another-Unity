#include <Technorama.h>

bool Curtain;                    //global
bool CurtainChange;              //global

void curtain (void) {
  if (CurtainChange) {
    digitalWrite(OUT_CURTAIN, Curtain); //on off curtain
    CurtainChange = false;
    error(0);
  }
}