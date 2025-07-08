#include<Uduino.h>
Uduino uduino("secondArduino");

int variable = 0;

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
    if (variable == 1000) variable = 0;
    delay(100);
  }
}
