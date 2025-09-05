#include <mtba.h>

struct MirrorRequest {
  bool state;       // global mirror state
  bool requested;   // flag for mirror state change
};

MirrorRequest mirrorRequest = {false, false};

// Public function to request mirror changing
void requestMirrorChange(bool state) {
  mirrorRequest.requested = true;
  mirrorRequest.state = state;
}

void mirror(void) {
  if (mirrorRequest.requested) {
    digitalWrite(REL_MIRROR, mirrorRequest.state);  // set mirror relay
    mirrorRequest.requested = false;
    error(0);                          // signal successful action
  }
}
