#include <Technorama.h>

bool Mirror;                    //global
bool MirrorChange;              //global

void mirror (void) {
  if (MirrorChange) {
    digitalWrite(CONTROLLINO_D4, Mirror); //on off mirror
    MirrorChange = false;
    error(0);
  }
}
