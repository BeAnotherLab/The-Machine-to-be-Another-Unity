#include <Technorama.h>

bool Reset=false;          //global

void reset() {
  if (Reset) {            //sys_rst received
    Reset=false;
    FirstRunWall=true;    //wall reinitialize
    FirstRunMirror=true;  //mirror reinitialize
  }
}
