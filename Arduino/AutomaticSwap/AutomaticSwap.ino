#include<Uduino.h>
Uduino uduino("serialControl");

void setup()
{
  Serial.begin(9600);
  pinMode(13, OUTPUT);
  uduino.addCommand("wall_on", wallOn);
  uduino.addCommand("wall_off", wallOff);
}

void wallOn() {
  digitalWrite(13, HIGH);
}

void wallOff() {
  digitalWrite(13, LOW);
}

void loop()
{
  uduino.readSerial();
  delay(15);
}
