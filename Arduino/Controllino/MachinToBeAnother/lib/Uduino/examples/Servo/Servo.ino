#include<Uduino.h>
Uduino uduino("advancedBoard");

void setup()
{
  Serial.begin(9600);
  uduino.addCommand("turnLeft", turnLeft);
  uduino.addCommand("off", disable);
}

void turnLeft() {
  // we retreive the argument by looking at the pointer, but we could use "uduino.getParameter(0)"
  char *arg;
  arg = uduino.next();
  myservo.write(atoi(arg));
}

void disable() {
  digitalWrite(13, LOW);
}

void loop()
{
  uduino.update();
  delay(15);
}
