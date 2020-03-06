#include<Uduino.h>
Uduino uduino("advancedBoard");

void setup()
{
  Serial.begin(9600);
  uduino.addCommand("command", myCommand);
}

void myCommand() {
  int numberOfParameters = uduino.getNumberOfParameters();
  if (numberOfParameters == 3) {
    int val1 = uduino.charToInt(uduino.getParameter(0));
    int val2 = uduino.charToInt(uduino.getParameter(1));
    int val3 = uduino.charToInt(uduino.getParameter(2));
    Serial.print("Received the parameters ");
    Serial.print(val1);
    Serial.print(", ");
    Serial.print(val2);
    Serial.print(", ");
    Serial.print(val3);
    Serial.println(".");

  } else {
    Serial.print("3 parameters expected but only");
    Serial.print(numberOfParameters);
    Serial.println(" received.");
  }
}


void loop() {
  uduino.update();
}
