#include <Technorama.h>

bool Mirror;                    //global
bool MirrorChange;              //global

void mirror (void) {
  if (MirrorChange) {
    digitalWrite(REL_MIRROR, Mirror); //on off mirror
    MirrorChange = false;
    error(0);
  }
}
