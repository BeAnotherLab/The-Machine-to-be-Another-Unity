#include<Uduino.h>
Uduino uduino("firstArduino");

int variable = 1000;

void setup()
{
  Serial.begin(9600);
  uduino.addCommand("GetVariable", GetVariable);
}

void GetVariable() {
  Serial.println(variable);
}

void loop()
{
  uduino.update();

  if (uduino.isConnected()) {
    variable ++;
    if (variable == 2000) variable = 1000;
    delay(100);
  }
}
