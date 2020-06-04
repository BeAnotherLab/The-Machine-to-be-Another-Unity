#include <Technorama.h>

bool MDReleased = false;                    //global
bool MDReady = true;                        //global
bool MDOk = false;                          //global

void machinerydrive(){
  MDReady = digitalRead(CONTROLLINO_A3);  //read MDReady
  MDOk = digitalRead(CONTROLLINO_A2);     //read MDOk
  MDReleased = MDReady && MDOk;
}
