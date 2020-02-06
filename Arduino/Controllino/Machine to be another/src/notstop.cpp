#include <Technorama.h>

void notstop() {
  Serial.println("NOTSTOP!");
  digitalWrite(CONTROLLINO_D0, LOW);  //alles abschalten
  digitalWrite(CONTROLLINO_D1, LOW);
  digitalWrite(CONTROLLINO_D2, LOW);
  digitalWrite(CONTROLLINO_D6, LOW);
  digitalWrite(CONTROLLINO_D7, LOW);
  while(1);
}
