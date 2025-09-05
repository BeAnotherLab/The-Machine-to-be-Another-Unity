#include<Uduino.h>
Uduino uduino("myArduinoName"); // Declare and name your object

void setup()
{
  Serial.begin(9600);
  uduino.addCommand("returnValue", returnValue);
  uduino.addCommand("doSomething", doSomething);
}

void returnValue() {
  uduino.println(analogRead(A0));
}

void doSomething() {
  int numberOfParameters = uduino.getNumberOfParameters();
  
  if (numberOfParameters == 0)
    return;

  char * firstParameter = uduino.getParameter(0);
  int parameterAsInt = uduino.charToInt(firstParameter);

  char * secondParameter = uduino.getParameter(1);

  // ... do something with the values, or get other parameters
}

void loop()
{
  uduino.update();
  delay(10); // Delay of your choice or to match Unity's Read Timout

  if (uduino.isConnected()) {

    // ... your own code

    // Important: If you Serial.print values outside this loop,
    // the board will not be correclty detected on Unity !
  }
}
