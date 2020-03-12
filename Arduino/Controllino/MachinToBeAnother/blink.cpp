#include <Technorama.h>

void blink() {
    digitalWrite(CONTROLLINO_D7, !digitalRead(CONTROLLINO_D7));
}
